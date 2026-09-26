import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/workflow_models.dart';

class WorkflowService {
  Future<List<EmergencyAlert>> fetchEmergencyAlerts(String? token) async {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }

    try {
      final response = await http
          .get(Uri.parse(ApiConfig.emergencyWorkflowsUrl), headers: headers)
          .timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final List<dynamic> list = jsonDecode(response.body);
        return list.map((item) => EmergencyAlert.fromJson(item)).toList();
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized access. Please login again.');
      } else {
        throw Exception('Failed to load emergency alerts (${response.statusCode})');
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<EmergencyAlert> fetchWorkflowStatus(String? token, int id) async {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }

    try {
      final response = await http
          .get(Uri.parse(ApiConfig.workflowStatusUrl(id)), headers: headers)
          .timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(response.body);
        return EmergencyAlert.fromJson(data);
      } else if (response.statusCode == 404) {
        throw Exception('Workflow #$id not found.');
      } else {
        throw Exception('Failed to load status for workflow #$id');
      }
    } catch (e) {
      rethrow;
    }
  }

  // Admin workflow list (if user is logged in as admin)
  Future<List<EmergencyAlert>> fetchAdminWorkflows(String? token, {String? status}) async {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    if (token != null && token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $token';
    }

    String url = ApiConfig.adminWorkflowsUrl;
    if (status != null && status.isNotEmpty) {
      url += '?status=$status';
    }

    try {
      final response = await http
          .get(Uri.parse(url), headers: headers)
          .timeout(ApiConfig.timeoutDuration);

      if (response.statusCode == 200) {
        final List<dynamic> list = jsonDecode(response.body);
        return list.map((item) => EmergencyAlert.fromJson(item)).toList();
      } else {
        throw Exception('Failed to load admin workflows (${response.statusCode})');
      }
    } catch (e) {
      rethrow;
    }
  }
}
