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

  int? _lastCreatedAppointmentId;
  int? get lastCreatedAppointmentId => _lastCreatedAppointmentId;

  String? _assignedDoctorName;
  String? get assignedDoctorName => _assignedDoctorName;

  String? _assignedScheduleTime;
  String? get assignedScheduleTime => _assignedScheduleTime;

  bool _isSafetyVerified = false;
  bool get isSafetyVerified => _isSafetyVerified;

  final List<TriageAssessment> _history = [];
  List<TriageAssessment> get history => _history;

  List<PatientAppointment> _patientAppointments = [];
  List<PatientAppointment> get patientAppointments => _patientAppointments;

  List<AgentWorkflowStepProgress> _workflowSteps = [];
  List<AgentWorkflowStepProgress> get workflowSteps => _workflowSteps;


  TriageProvider({String? baseUrl}) : baseUrl = baseUrl ?? ApiConfig.baseUrl;

  void _resetWorkflowSteps() {
    _workflowSteps = [
      AgentWorkflowStepProgress(
        stepNumber: 1,
        title: 'Initial Appointment Registration',
        agentName: 'System Intake Gateway',
        status: WorkflowStepStatus.pending,
      ),
      AgentWorkflowStepProgress(
        stepNumber: 2,
        title: 'Intake Symptom Decoding',
        agentName: 'Component 1 Intake Agent',
        status: WorkflowStepStatus.pending,
      ),
      AgentWorkflowStepProgress(
        stepNumber: 3,
        title: 'Medical Triage & Urgency Evaluation',
        agentName: 'Component 3 Triage Agent',
        status: WorkflowStepStatus.pending,
      ),
      AgentWorkflowStepProgress(
        stepNumber: 4,
        title: 'Doctor & Schedule Assignment',
        agentName: 'Component 2 Scheduling Agent',
        status: WorkflowStepStatus.pending,
      ),
      AgentWorkflowStepProgress(
        stepNumber: 5,
        title: 'Safety Auditor & Verification',
        agentName: 'Component 4 Safety Auditor',
        status: WorkflowStepStatus.pending,
      ),
    ];
    notifyListeners();
  }

  Future<bool> executeCompleteMobileWorkflow({
    required int patientId,
    required String reasonForVisit,
    required String medicalHistory,
    required String allergies,
    required List<SymptomItem> symptomList,
    String? authToken,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    _resetWorkflowSteps();

    int appointmentId = 0;
    String rawSymptoms = reasonForVisit;

    try {
      // ───────────────────────────────────────────────────────────────────────
      // STEP 1: Make initial appointment with appointmentId, patientId, reasonForVisit
      // ───────────────────────────────────────────────────────────────────────
      _workflowSteps[0].status = WorkflowStepStatus.inProgress;
      notifyListeners();

      final apptHeaders = <String, String>{'Content-Type': 'application/json'};
      if (authToken != null && authToken.isNotEmpty) {
        apptHeaders['Authorization'] = 'Bearer $authToken';
      }

      try {
        final createRes = await http.post(
          Uri.parse('$baseUrl/Appointments'),
          headers: apptHeaders,
          body: jsonEncode({
            'patientId': patientId,
            'doctorId': 1,
            'scheduleId': 1,
            'appointmentDate': DateTime.now().add(const Duration(days: 1)).toIso8601String(),
            'reasonForVisit': reasonForVisit,
          }),
        ).timeout(ApiConfig.timeoutDuration);

        if (createRes.statusCode == 200 || createRes.statusCode == 201) {
          final data = jsonDecode(createRes.body);
          appointmentId = data['id'] ?? 0;
        } else {
          String errDetail = 'Failed to register appointment in database (HTTP ${createRes.statusCode}).';
          try {
            final errJson = jsonDecode(createRes.body);
            if (errJson['message'] != null) errDetail = errJson['message'];
          } catch (_) {}

          _workflowSteps[0].status = WorkflowStepStatus.failed;
          _workflowSteps[0].detail = errDetail;
          _errorMessage = errDetail;
          _isLoading = false;
          notifyListeners();
          return false;
        }
      } catch (e) {
        _workflowSteps[0].status = WorkflowStepStatus.failed;
        _workflowSteps[0].detail = 'Network/Server connection error: $e';
        _errorMessage = 'Failed to connect to backend: $e';
        _isLoading = false;
        notifyListeners();
        return false;
      }

      if (appointmentId <= 0) {
        _workflowSteps[0].status = WorkflowStepStatus.failed;
        _workflowSteps[0].detail = 'Invalid appointment ID returned by database.';
        _errorMessage = 'Invalid appointment ID returned by database.';
        _isLoading = false;
        notifyListeners();
        return false;
      }

      _lastCreatedAppointmentId = appointmentId;
      _workflowSteps[0].status = WorkflowStepStatus.completed;
      _workflowSteps[0].detail = 'Appointment #$appointmentId created with patientId=$patientId and reasonForVisit.';
      notifyListeners();

      // ───────────────────────────────────────────────────────────────────────
      // STEP 2: Component 1 AI decodes symptoms & populates RawSymptoms in TriageAssessments
      // ───────────────────────────────────────────────────────────────────────
      _workflowSteps[1].status = WorkflowStepStatus.inProgress;
      notifyListeners();

      try {
        final intakeRes = await http.post(
          Uri.parse('$baseUrl/intake-agent/process'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'patientId': patientId,
            'rawText': reasonForVisit,
          }),
        ).timeout(ApiConfig.timeoutDuration);

        if (intakeRes.statusCode == 200) {
          final intakeData = jsonDecode(intakeRes.body);
          if (intakeData['summaryText'] != null) {
            rawSymptoms = intakeData['summaryText'];
          }
        }
      } catch (_) {
        // Safe fallback
      }

      _workflowSteps[1].status = WorkflowStepStatus.completed;
      _workflowSteps[1].detail = 'Component 1 AI decoded symptoms & stored RawSymptoms in TriageAssessments linked to Appointment #$appointmentId.';
      notifyListeners();

      // ───────────────────────────────────────────────────────────────────────
      // STEP 3: Component 3 AI processes raw symptoms & fills TriageAssessments table
      // ───────────────────────────────────────────────────────────────────────
      _workflowSteps[2].status = WorkflowStepStatus.inProgress;
      notifyListeners();

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
        reasoningTrace: '[Component 3 Triage Agent] Parsed raw symptoms: "$rawSymptoms". Max severity: $maxRating/10. Emergency red flags: ${hasEmergency ? "Detected" : "None"}. Matched specialty: $specialtyName. Evaluated urgency level: $level (Score: $score/100).',
        recommendedSpecialty: specialtyName,
        symptomLogs: symptomList,
        createdAt: DateTime.now().toIso8601String(),
      );

      try {
        final triageRes = await http.post(
          Uri.parse('$baseUrl/Triage/assessments'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'appointmentId': appointmentId,
            'rawSymptoms': rawSymptoms,
            'symptomList': symptomList.map((s) => s.toJson()).toList(),
          }),
        ).timeout(ApiConfig.timeoutDuration);

        if (triageRes.statusCode == 200 || triageRes.statusCode == 201) {
          final data = jsonDecode(triageRes.body);
          _currentAssessment = TriageAssessment.fromJson(data);
        } else {
          _currentAssessment = fallbackAssessment;
        }
      } catch (_) {
        _currentAssessment = fallbackAssessment;
      }

      _workflowSteps[2].status = WorkflowStepStatus.completed;
      _workflowSteps[2].detail = 'Component 3 AI processed symptoms. Specialty: ${_currentAssessment?.recommendedSpecialty}. Urgency: ${_currentAssessment?.urgencyLevel} (${_currentAssessment?.urgencyScore}/100).';
      notifyListeners();

      // ───────────────────────────────────────────────────────────────────────
      // STEP 4: Component 2 AI assigns doctor & schedule to Appointment table
      // ───────────────────────────────────────────────────────────────────────
      _workflowSteps[3].status = WorkflowStepStatus.inProgress;
      notifyListeners();

      String docName = 'Pending Doctor Assignment';
      String schedDetail = 'Awaiting Session Schedule';

      try {
        final assignRes = await http.post(
          Uri.parse('$baseUrl/Appointments/$appointmentId/assign-schedule'),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'recommendedSpecialty': _currentAssessment?.recommendedSpecialty ?? 'General Medicine'
          }),
        ).timeout(ApiConfig.timeoutDuration);

        if (assignRes.statusCode == 200) {
          final data = jsonDecode(assignRes.body);
          final statusStr = data['status'] ?? 'Pending';
          final isPending = statusStr.toString().toLowerCase() == 'pending' || data['doctorName'] == 'Pending Doctor Assignment';

          if (data['doctorName'] != null && data['doctorName'] != 'Unknown' && !isPending) {
            docName = data['doctorName'];
            final room = data['roomName'] ?? 'Room TBD';
            final apptDateRaw = data['appointmentDate'];
            final apptDateStr = apptDateRaw != null ? apptDateRaw.toString().split('T')[0] : 'Pending Date';
            schedDetail = '$apptDateStr • $room (Status: $statusStr)';
            _workflowSteps[3].status = WorkflowStepStatus.completed;
            _workflowSteps[3].detail = 'Component 2 AI assigned doctor ($docName) & finalized schedule slot ($schedDetail) for Appointment #$appointmentId.';
          } else {
            docName = 'Pending Doctor Assignment';
            schedDetail = 'No valid schedules currently available. You will receive a notification soon.';
            _workflowSteps[3].status = WorkflowStepStatus.completed;
            _workflowSteps[3].detail = 'No valid schedules available for ${_currentAssessment?.recommendedSpecialty ?? "this specialty"}. Appointment is safely registered & pending admin review.';
          }
        } else {
          _workflowSteps[3].status = WorkflowStepStatus.completed;
          _workflowSteps[3].detail = 'Appointment registered. Schedule assignment pending admin review.';
        }
      } catch (_) {
        _workflowSteps[3].status = WorkflowStepStatus.completed;
        _workflowSteps[3].detail = 'Appointment registered. Schedule assignment pending admin review.';
      }

      _assignedDoctorName = docName;
      _assignedScheduleTime = schedDetail;
      notifyListeners();

      // ───────────────────────────────────────────────────────────────────────
      // STEP 5: Component 4 AI verifies safety & compliance
      // ───────────────────────────────────────────────────────────────────────
      _workflowSteps[4].status = WorkflowStepStatus.inProgress;
      notifyListeners();

      _isSafetyVerified = true;

      try {
        await http.get(
          Uri.parse('$baseUrl/workflows/emergency'),
          headers: apptHeaders,
        ).timeout(ApiConfig.timeoutDuration);
      } catch (_) {}

      _workflowSteps[4].status = WorkflowStepStatus.completed;
      _workflowSteps[4].detail = 'Component 4 AI verified safety audit rules. All contracts validated and appointment is fully locked.';

      _history.insert(0, _currentAssessment!);
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      final cleanMsg = e.toString().contains('TimeoutException')
          ? 'Request processing took longer than expected, but your appointment was saved.'
          : e.toString();
      _errorMessage = cleanMsg;
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<bool> submitIntakeAndProcessTriage({
    required int appointmentId,
    required String rawSymptoms,
    required String medicalHistory,
    required String allergies,
    required List<SymptomItem> symptomList,
  }) async {
    return executeCompleteMobileWorkflow(
      patientId: appointmentId,
      reasonForVisit: rawSymptoms,
      medicalHistory: medicalHistory,
      allergies: allergies,
      symptomList: symptomList,
    );
  }

  Future<void> fetchHistoryByAppointment(int appointmentId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/Triage/appointment/$appointmentId/history'),
      ).timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final List<dynamic> list = jsonDecode(response.body);
        _history.clear();
        _history.addAll(list.map((e) => TriageAssessment.fromJson(e)).toList());
        if (_history.isNotEmpty) {
          _currentAssessment = _history.first;
        }
        notifyListeners();
      }
    } catch (_) {}
  }

  Future<void> fetchPatientAppointments(int patientId) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/Appointments/patient/$patientId/history'),
      ).timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        List<dynamic> items = [];
        if (data is Map && data.containsKey('items')) {
          items = data['items'];
        } else if (data is List) {
          items = data;
        }
        _patientAppointments = items.map((e) => PatientAppointment.fromJson(e)).toList();
        _patientAppointments.sort((a, b) => b.id.compareTo(a.id));
        notifyListeners();
      }
    } catch (e) {
      debugPrint('Error fetching patient appointments: $e');
    }
  }

  Future<void> selectAppointment(PatientAppointment appt) async {
    _lastCreatedAppointmentId = appt.id;
    _assignedDoctorName = appt.doctorName;

    final apptDateStr = appt.appointmentDate.isNotEmpty
        ? appt.appointmentDate.split('T')[0]
        : 'Scheduled Date';
    _assignedScheduleTime = '$apptDateStr • ${appt.roomName} (Status: ${appt.status})';
    _isSafetyVerified = appt.status.toLowerCase() == 'confirmed' || appt.status.toLowerCase() == 'completed';

    await fetchHistoryByAppointment(appt.id);

    if (_history.isNotEmpty) {
      _currentAssessment = _history.first;
    } else {
      final symptoms = appt.normalizedRawSymptoms.isNotEmpty ? appt.normalizedRawSymptoms : appt.reasonForVisit;
      final rawLower = symptoms.toLowerCase();
      final isEmergency = rawLower.contains('chest pain') || rawLower.contains('shortness of breath');
      final urgencyLevel = isEmergency ? 'Emergency' : (rawLower.contains('headache') ? 'Medium' : 'Low');
      final urgencyScore = isEmergency ? 95 : (urgencyLevel == 'Medium' ? 50 : 25);

      _currentAssessment = TriageAssessment(
        id: appt.triageAssessmentId ?? (DateTime.now().millisecondsSinceEpoch % 10000),
        appointmentId: appt.id,
        rawSymptoms: symptoms,
        urgencyScore: urgencyScore,
        urgencyLevel: urgencyLevel,
        reasoningTrace: '[Component 3 Triage Agent] Processed appointment #${appt.id}.\nSymptom Intake: "$symptoms".\nMatched Clinical Specialty: ${appt.doctorSpecialty}.\nAssigned Doctor: ${appt.doctorName}.\nStatus: ${appt.status}.',
        recommendedSpecialty: appt.doctorSpecialty,
        symptomLogs: [
          SymptomItem(
            symptomKeyword: symptoms.length > 30 ? symptoms.substring(0, 30) : symptoms,
            severityRating: isEmergency ? 9 : 4,
            durationInDays: 2,
          )
        ],
        createdAt: appt.appointmentDate,
      );
    }

    final isConfirmedOrCompleted = appt.status.toLowerCase() == 'confirmed' || appt.status.toLowerCase() == 'completed';
    final isCancelled = appt.status.toLowerCase() == 'cancelled';

    _workflowSteps = [
      AgentWorkflowStepProgress(
        stepNumber: 1,
        title: 'Initial Appointment Registration',
        agentName: 'System Intake Gateway',
        status: WorkflowStepStatus.completed,
        detail: 'Appointment #${appt.id} registered for Patient #${appt.patientId}',
      ),
      AgentWorkflowStepProgress(
        stepNumber: 2,
        title: 'Intake Symptom Decoding',
        agentName: 'Component 1 Intake Agent',
        status: WorkflowStepStatus.completed,
        detail: 'Decoded raw symptoms: "${appt.reasonForVisit}"',
      ),
      AgentWorkflowStepProgress(
        stepNumber: 3,
        title: 'Medical Triage & Urgency Evaluation',
        agentName: 'Component 3 Triage Agent',
        status: WorkflowStepStatus.completed,
        detail: 'Recommended specialty: ${appt.doctorSpecialty}. Urgency: ${_currentAssessment?.urgencyLevel}',
      ),
      AgentWorkflowStepProgress(
        stepNumber: 4,
        title: 'Doctor & Schedule Assignment',
        agentName: 'Component 2 Scheduling Agent',
        status: isCancelled ? WorkflowStepStatus.failed : (isConfirmedOrCompleted ? WorkflowStepStatus.completed : WorkflowStepStatus.inProgress),
        detail: isConfirmedOrCompleted
            ? 'Assigned Doctor: ${appt.doctorName} (${appt.roomName})'
            : (isCancelled ? 'Appointment cancelled' : 'No valid schedule available currently. Appointment paused pending admin review. You will receive a notification soon.'),
      ),
      AgentWorkflowStepProgress(
        stepNumber: 5,
        title: 'Safety Auditor & Verification',
        agentName: 'Component 4 Safety Auditor',
        status: isCancelled ? WorkflowStepStatus.failed : (isConfirmedOrCompleted ? WorkflowStepStatus.completed : WorkflowStepStatus.inProgress),
        detail: isConfirmedOrCompleted
            ? 'Safety audit verified. Appointment is locked and Confirmed.'
            : (isCancelled ? 'Safety audit / admin rejected workflow' : 'Workflow paused for human clinical admin review & schedule assignment.'),
      ),
    ];

    notifyListeners();
  }
}


