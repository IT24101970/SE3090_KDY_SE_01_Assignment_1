import 'dart:async';
import 'package:flutter/foundation.dart';
import '../models/workflow_models.dart';
import '../services/workflow_service.dart';

class WorkflowProvider with ChangeNotifier {
  final WorkflowService _workflowService = WorkflowService();

  bool _isLoading = false;
  bool get isLoading => _isLoading;

  String? _errorMessage;
  String? get errorMessage => _errorMessage;

  List<EmergencyAlert> _alerts = [];
  List<EmergencyAlert> get alerts => _alerts;

  List<EmergencyAlert> get activeEmergencyAlerts =>
      _alerts.where((a) => a.isHighRisk || a.status == WorkflowStatus.pausedForApproval).toList();

  EmergencyAlert? _selectedWorkflow;
  EmergencyAlert? get selectedWorkflow => _selectedWorkflow;

  DateTime? _lastRefreshedAt;
  DateTime? get lastRefreshedAt => _lastRefreshedAt;

  Timer? _pollingTimer;
  bool _isPollingActive = false;
  bool get isPollingActive => _isPollingActive;

  String? _notificationMessage;
  String? get notificationMessage => _notificationMessage;

  // Track previous status map to detect changes
  final Map<int, WorkflowStatus> _previousStatuses = {};

  void startAutoPolling(String? token) {
    if (_isPollingActive) return;
    _isPollingActive = true;
    _pollingTimer = Timer.periodic(const Duration(seconds: 10), (_) {
      refreshAlerts(token, isBackground: true);
    });
    notifyListeners();
  }

  void stopAutoPolling() {
    _pollingTimer?.cancel();
    _pollingTimer = null;
    _isPollingActive = false;
    notifyListeners();
  }

  Future<void> fetchAlerts(String? token) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    await refreshAlerts(token, isBackground: false);
    _isLoading = false;
    notifyListeners();
  }

  Future<void> refreshAlerts(String? token, {bool isBackground = false}) async {
    try {
      final fetched = await _workflowService.fetchEmergencyAlerts(token);
      
      // Detect changes for in-app alert notification
      for (var item in fetched) {
        if (_previousStatuses.containsKey(item.id)) {
          final oldStatus = _previousStatuses[item.id];
          if (oldStatus != item.status) {
            _notificationMessage =
                'Workflow #${item.id} status updated from ${getStatusDisplayName(oldStatus!)} to ${getStatusDisplayName(item.status)}';
          }
        }
        _previousStatuses[item.id] = item.status;
      }

      _alerts = fetched;
      _lastRefreshedAt = DateTime.now();
      _errorMessage = null;
    } catch (e) {
      if (!isBackground) {
        _errorMessage = e.toString().replaceAll('Exception: ', '');
      }
    } finally {
      notifyListeners();
    }
  }

  Future<void> fetchWorkflowStatus(String? token, int id) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _selectedWorkflow = await _workflowService.fetchWorkflowStatus(token, id);
    } catch (e) {
      _errorMessage = e.toString().replaceAll('Exception: ', '');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void clearNotification() {
    _notificationMessage = null;
    notifyListeners();
  }

  @override
  void dispose() {
    stopAutoPolling();
    super.dispose();
  }
}
