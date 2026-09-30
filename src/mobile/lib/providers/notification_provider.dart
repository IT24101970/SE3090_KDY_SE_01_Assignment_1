import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/notification_model.dart';

class NotificationProvider with ChangeNotifier {
  final List<AppNotification> _notifications = [];

  List<AppNotification> get notifications => List.unmodifiable(_notifications);

  int get unreadCount => _notifications.where((n) => !n.isRead).length;

  List<AppNotification> getForRole(String role) {
    final lowerRole = role.toLowerCase();
    return _notifications.where((n) {
      final target = n.targetRole.toLowerCase();
      return target == 'all' || target == lowerRole;
    }).toList();
  }

  void addNotification(AppNotification notification) {
    // Avoid duplicate notifications with same ID
    if (!_notifications.any((n) => n.id == notification.id)) {
      _notifications.insert(0, notification);
      notifyListeners();
    }
  }

  void markAsRead(String id) {
    final idx = _notifications.indexWhere((n) => n.id == id);
    if (idx != -1) {
      _notifications[idx].isRead = true;
      notifyListeners();
    }
  }

  void markAllAsRead() {
    for (var n in _notifications) {
      n.isRead = true;
    }
    notifyListeners();
  }

  void clearAll() {
    _notifications.clear();
    notifyListeners();
  }

  /// Polls or syncs role-specific notifications from the backend APIs
  Future<void> refreshNotifications({
    required String role,
    int? userId,
    int? patientId,
    int? doctorId,
  }) async {
    final lowerRole = role.toLowerCase();

    try {
      if (lowerRole == 'admin') {
        // 1. Admin Notifications: Check for Safety Auditor High-Risk Paused/SafeFailed workflows
        await _fetchAdminSafetyAlerts();
      } else if (lowerRole == 'patient') {
        // 2. Patient Notifications: Check for Finalized Appointments by AI Agents
        await _fetchPatientFinalizedAppointments(patientId ?? userId ?? 0);
      } else if (lowerRole == 'doctor') {
        // 3. Doctor Notifications: Check for Upcoming Scheduled Appointments
        await _fetchDoctorUpcomingAppointments(doctorId ?? userId ?? 1);
      }
    } catch (_) {
      // Graceful fallback: local notification state preserved
    }
  }

  Future<void> _fetchAdminSafetyAlerts() async {
    try {
      final response = await http
          .get(Uri.parse(ApiConfig.adminWorkflowsUrl))
          .timeout(const Duration(seconds: 4));

      if (response.statusCode == 200) {
        final List<dynamic> items = jsonDecode(response.body);
        for (var wf in items) {
          final status = wf['status']?.toString() ?? '';
          final risk = wf['risk']?.toString() ?? '';
          final id = wf['id'];

          if (status == 'PausedForApproval' || status == 'SafeFailed' || risk.toLowerCase() == 'high') {
            addNotification(
              AppNotification(
                id: 'admin_safety_alert_$id',
                title: '🛡️ Safety Auditor High-Risk Alert',
                message: 'Workflow WF-$id requires human approval. Reason: ${wf['pauseReason'] ?? wf['objective'] ?? 'High risk detected'}',
                timestamp: wf['createdAt'] != null ? DateTime.parse(wf['createdAt']) : DateTime.now(),
                type: 'safety_alert',
                targetRole: 'admin',
                metadata: {'workflowId': id},
              ),
            );
          }
        }
      }
    } catch (_) {}
  }

  Future<void> _fetchPatientFinalizedAppointments(int patientId) async {
    try {
      final url = patientId > 0
          ? '${ApiConfig.appointmentsUrl}?patientId=$patientId'
          : ApiConfig.appointmentsUrl;
      final response = await http.get(Uri.parse(url)).timeout(const Duration(seconds: 4));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final List<dynamic> items = data is Map ? (data['items'] ?? []) : data;

        for (var appt in items) {
          final status = appt['status']?.toString() ?? '';
          final apptId = appt['id'];
          final docName = appt['doctorName'] ?? 'Specialist Doctor';

          if (status == 'Confirmed' || status == 'Completed') {
            addNotification(
              AppNotification(
                id: 'patient_appt_finalized_$apptId',
                title: '✅ Appointment Finalized',
                message: 'Your appointment #$apptId with $docName has been confirmed by AI Agents.',
                timestamp: appt['createdAt'] != null ? DateTime.parse(appt['createdAt']) : DateTime.now(),
                type: 'appointment_finalized',
                targetRole: 'patient',
                metadata: {'appointmentId': apptId},
              ),
            );
          }
        }
      }
    } catch (_) {}
  }

  Future<void> _fetchDoctorUpcomingAppointments(int doctorId) async {
    try {
      final response = await http
          .get(Uri.parse('${ApiConfig.appointmentsUrl}?doctorId=$doctorId'))
          .timeout(const Duration(seconds: 4));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final List<dynamic> items = data is Map ? (data['items'] ?? []) : data;

        for (var appt in items) {
          final apptId = appt['id'];
          final patientName = appt['patientName'] ?? 'Patient';
          final reason = appt['reasonForVisit'] ?? appt['symptoms'] ?? 'Consultation';

          addNotification(
            AppNotification(
              id: 'doctor_upcoming_appt_$apptId',
              title: '🩺 Upcoming Patient Appointment',
              message: 'Scheduled appointment with $patientName ($reason).',
              timestamp: appt['appointmentDate'] != null
                  ? DateTime.parse(appt['appointmentDate'])
                  : DateTime.now(),
              type: 'upcoming_appointment',
              targetRole: 'doctor',
              metadata: {'appointmentId': apptId},
            ),
          );
        }
      }
    } catch (_) {}
  }
}
