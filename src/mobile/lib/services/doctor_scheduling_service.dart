import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/doctor_scheduling_models.dart';

class DoctorSchedulingService {
  // Fetch real doctors directly from PostgreSQL database via API.
  // If no doctors exist in DB, automatically seed a default doctor into database.
  static Future<List<DoctorProfile>> getDoctors() async {
    final response = await http
        .get(Uri.parse(ApiConfig.doctorsUrl))
        .timeout(ApiConfig.timeoutDuration);

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      List<DoctorProfile> doctors = data.map((json) => DoctorProfile.fromJson(json)).toList();

      if (doctors.isEmpty) {
        // Automatically put a default doctor into PostgreSQL database if empty
        final newDoc = await _seedDefaultDoctorToDb();
        if (newDoc != null) {
          doctors.add(newDoc);
        }
      }
      return doctors;
    } else {
      throw Exception('Failed to load doctors from database: ${response.body}');
    }
  }

  // Seed default doctor record into database via API
  static Future<DoctorProfile?> _seedDefaultDoctorToDb() async {
    try {
      final response = await http.post(
        Uri.parse(ApiConfig.doctorsUrl),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'userId': 2,
          'specialtyId': 1,
          'qualifications': 'MD, FACC, Board Certified Specialist'
        }),
      ).timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 201 || response.statusCode == 200) {
        return DoctorProfile.fromJson(jsonDecode(response.body));
      }
    } catch (_) {}
    return null;
  }

  // Fetch doctor schedules directly from database for logged in doctor
  static Future<List<DoctorScheduleModel>> getDoctorSchedules(int doctorId) async {
    final response = await http
        .get(Uri.parse('${ApiConfig.doctorSchedulesUrl}?doctorId=$doctorId'))
        .timeout(ApiConfig.timeoutDuration);

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      return data.map((json) => DoctorScheduleModel.fromJson(json)).toList();
    } else {
      throw Exception('Failed to fetch doctor schedules from database');
    }
  }

  // Fetch appointments / patient queue for doctor directly from database
  static Future<List<PatientQueueItem>> getPatientQueue(int doctorId) async {
    final response = await http
        .get(Uri.parse('${ApiConfig.appointmentsUrl}?doctorId=$doctorId'))
        .timeout(ApiConfig.timeoutDuration);

    if (response.statusCode == 200) {
      final Map<String, dynamic> data = jsonDecode(response.body);
      final List<dynamic> items = data['items'] ?? [];
      
      List<PatientQueueItem> queue = [];
      for (int i = 0; i < items.length; i++) {
        final itemJson = items[i];
        final apptId = itemJson['id'];
        
        // Fetch consultation details if available
        dynamic consultationJson;
        try {
          final consResponse = await http
              .get(Uri.parse('${ApiConfig.consultationsUrl}/appointment/$apptId'))
              .timeout(const Duration(seconds: 3));
          if (consResponse.statusCode == 200) {
            consultationJson = jsonDecode(consResponse.body);
          }
        } catch (_) {}

        if (consultationJson != null) {
          itemJson['attendanceStatus'] = consultationJson['attendanceStatus'];
          itemJson['clinicalNotes'] = consultationJson['clinicalNotes'];
          itemJson['prescriptionData'] = consultationJson['prescriptionData'];
        }

        queue.add(PatientQueueItem.fromJson(itemJson, i + 1));
      }
      return queue;
    } else {
      throw Exception('Failed to fetch patient queue from database');
    }
  }

  // Fetch leave history for logged in doctor directly from database
  static Future<List<DoctorLeaveModel>> getDoctorLeaves(int doctorId) async {
    final response = await http
        .get(Uri.parse('${ApiConfig.doctorLeavesUrl}?doctorId=$doctorId'))
        .timeout(ApiConfig.timeoutDuration);

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      return data.map((json) => DoctorLeaveModel.fromJson(json)).toList();
    } else {
      throw Exception('Failed to fetch leave records from database');
    }
  }

  // Submit new leave application to database
  static Future<DoctorLeaveModel> submitDoctorLeave({
    required int doctorId,
    required DateTime startDate,
    required DateTime endDate,
    required String reason,
  }) async {
    final response = await http.post(
      Uri.parse(ApiConfig.doctorLeavesUrl),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'doctorId': doctorId,
        'startDate': startDate.toUtc().toIso8601String(),
        'endDate': endDate.toUtc().toIso8601String(),
        'reason': reason,
      }),
    ).timeout(ApiConfig.timeoutDuration);

    if (response.statusCode == 201 || response.statusCode == 200) {
      return DoctorLeaveModel.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Failed to submit leave application: ${response.body}');
    }
  }

  // Update patient attendance & consultation notes in database
  static Future<void> updateConsultation({
    required int appointmentId,
    required AttendanceStatus status,
    required String clinicalNotes,
    required String prescriptionData,
  }) async {
    // Check if consultation record already exists for appointment
    int? consultationId;
    try {
      final checkRes = await http
          .get(Uri.parse('${ApiConfig.consultationsUrl}/appointment/$appointmentId'))
          .timeout(const Duration(seconds: 4));
      if (checkRes.statusCode == 200) {
        final body = jsonDecode(checkRes.body);
        consultationId = body['id'];
      }
    } catch (_) {}

    if (consultationId != null) {
      // Patch existing consultation
      final response = await http.patch(
        Uri.parse('${ApiConfig.consultationsUrl}/$consultationId/attendance'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'attendanceStatus': status.index,
        }),
      ).timeout(ApiConfig.timeoutDuration);

      if (response.statusCode != 200) {
        throw Exception('Failed to update consultation attendance in DB');
      }
    } else {
      // Create new consultation
      final response = await http.post(
        Uri.parse(ApiConfig.consultationsUrl),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'appointmentId': appointmentId,
          'clinicalNotes': clinicalNotes,
          'prescriptionData': prescriptionData,
          'attendanceStatus': status.index,
        }),
      ).timeout(ApiConfig.timeoutDuration);

      if (response.statusCode != 201 && response.statusCode != 200) {
        throw Exception('Failed to save consultation details in DB');
      }
    }
  }
}
