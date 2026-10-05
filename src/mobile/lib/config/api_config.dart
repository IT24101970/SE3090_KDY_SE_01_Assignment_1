import 'package:flutter/foundation.dart';

class ApiConfig {
  // Default base URL for ASP.NET Core API server
  static String get baseUrl {
    if (kReleaseMode) {
      return 'https://channel-center-api-c3hkfmhfdhe5gghb.eastasia-01.azurewebsites.net/api';
    } else if (kIsWeb) {
      return 'http://localhost:5066/api';
    } else if (defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5066/api';
    } else {
      return 'http://localhost:5066/api';
    }
  }

  // Auth endpoints
  static String get loginUrl => '$baseUrl/Auth/login';
  static String get registerUrl => '$baseUrl/Auth/register';
  static String get adminLoginUrl => '$baseUrl/Auth/admin/login';
  static String get devLoginUrl => '$baseUrl/Auth/dev-login';
  static String get currentUserUrl => '$baseUrl/Auth/me';

  // Component 4 Operational & Emergency endpoints
  static String get emergencyWorkflowsUrl => '$baseUrl/workflows/emergency';
  static String workflowStatusUrl(int id) => '$baseUrl/workflows/$id/status';

  // Component 4 Admin endpoints (for admin role in mobile)
  static String get adminWorkflowsUrl => '$baseUrl/admin/workflows';
  static String adminWorkflowDetailUrl(int id) => '$baseUrl/admin/workflows/$id';

  // Student 2 Doctor Scheduling endpoints
  static String get doctorsUrl => '$baseUrl/doctor-scheduling/doctors';
  static String get doctorSchedulesUrl => '$baseUrl/doctor-scheduling/doctorschedules';
  static String get doctorLeavesUrl => '$baseUrl/doctor-scheduling/doctorleaves';
  static String get consultationsUrl => '$baseUrl/doctor-scheduling/consultations';
  static String get appointmentsUrl => '$baseUrl/appointments';

  // Request timeout
  static const Duration timeoutDuration = Duration(seconds: 10);
}
