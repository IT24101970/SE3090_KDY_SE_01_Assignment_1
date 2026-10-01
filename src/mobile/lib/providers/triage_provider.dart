import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/triage_models.dart';

class TriageProvider with ChangeNotifier {
  final String baseUrl;
  
  bool _isLoading = false;
  bool get isLoading => _isLoading;

  String? _errorMessage;
  String? get errorMessage => _errorMessage;

  TriageAssessment? _currentAssessment;
  TriageAssessment? get currentAssessment => _currentAssessment;

  final List<TriageAssessment> _history = [];
  List<TriageAssessment> get history => _history;

  TriageProvider({String? baseUrl}) : baseUrl = baseUrl ?? ApiConfig.baseUrl;

  Future<bool> submitIntakeAndProcessTriage({
    required int appointmentId,
    required String rawSymptoms,
    required String medicalHistory,
    required String allergies,
    required List<SymptomItem> symptomList,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      // 1. Submit pre-consultation intake questionnaire if medical history / allergies provided
      if (medicalHistory.isNotEmpty || allergies.isNotEmpty) {
        try {
          await http.post(
            Uri.parse('$baseUrl/Triage/questionnaires'),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'patientId': appointmentId,
              'responsesData': jsonEncode({
                'medicalHistory': medicalHistory,
                'allergies': allergies,
              }),
            }),
          ).timeout(const Duration(seconds: 4));
        } catch (_) {
          // Continue if questionnaire submission offline
        }
      }

      // 2. Prepare local fallback assessment structure
      final maxRating = symptomList.fold<int>(1, (max, item) => item.severityRating > max ? item.severityRating : max);
      final rawLower = rawSymptoms.toLowerCase();
      final hasEmergency = rawLower.contains('chest pain') || rawLower.contains('shortness of breath') || rawLower.contains('stroke');
      
      final score = hasEmergency ? 95 : (maxRating * 10);
      final level = score >= 80 ? 'Emergency' : (score >= 60 ? 'High' : (score >= 30 ? 'Medium' : 'Low'));

      String specialtyName = 'General Medicine';
      if (rawLower.contains('chest pain') || rawLower.contains('heart')) {
        specialtyName = 'Cardiology';
      } else if (rawLower.contains('rash') || rawLower.contains('skin')) {
        specialtyName = 'Dermatology';
      } else if (rawLower.contains('headache') || rawLower.contains('dizziness')) {
        specialtyName = 'Neurology';
      } else if (rawLower.contains('knee') || rawLower.contains('joint') || rawLower.contains('back pain')) {
        specialtyName = 'Orthopedics';
      }

      final fallbackAssessment = TriageAssessment(
        id: DateTime.now().millisecondsSinceEpoch % 10000,
        appointmentId: appointmentId,
        rawSymptoms: rawSymptoms,
        urgencyScore: score,
        urgencyLevel: level,
        reasoningTrace: '[Symptom Triage Agent] Parsed ${symptomList.length} symptom entries. Highest severity score: $maxRating/10. Emergency red flags: ${hasEmergency ? "Detected" : "None"}. Matched specialty: $specialtyName. Assessed urgency level: $level (Score: $score/100).',
        recommendedSpecialty: specialtyName,
        symptomLogs: symptomList,
        createdAt: DateTime.now().toIso8601String(),
      );

      // 3. Submit Triage Assessment to backend
      try {
        final response = await http.post(
          Uri.parse('$baseUrl/Triage/assessments'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'appointmentId': appointmentId,
            'rawSymptoms': rawSymptoms,
            'symptomList': symptomList.map((s) => s.toJson()).toList(),
          }),
        ).timeout(const Duration(seconds: 5));

        if (response.statusCode == 200 || response.statusCode == 201) {
          final data = jsonDecode(response.body);
          _currentAssessment = TriageAssessment.fromJson(data);
        } else {
          _currentAssessment = fallbackAssessment;
        }
      } catch (_) {
        _currentAssessment = fallbackAssessment;
      }

      _history.insert(0, _currentAssessment!);
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _errorMessage = e.toString();
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<void> fetchHistoryByAppointment(int appointmentId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/Triage/appointment/$appointmentId/history'),
      ).timeout(const Duration(seconds: 4));

      if (response.statusCode == 200) {
        final List<dynamic> list = jsonDecode(response.body);
        _history = list.map((e) => TriageAssessment.fromJson(e)).toList();
        if (_history.isNotEmpty) {
          _currentAssessment = _history.first;
        }
        notifyListeners();
      }
    } catch (_) {}
  }
}
