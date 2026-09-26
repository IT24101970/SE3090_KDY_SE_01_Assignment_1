import 'package:flutter/material.dart';

enum WorkflowStatus {
  running,
  pausedForApproval,
  approved,
  rejected,
  revisionRequested,
  completed,
  safeFailed,
  cancelled,
  unknown,
}

WorkflowStatus parseWorkflowStatus(dynamic raw) {
  if (raw == null) return WorkflowStatus.unknown;
  final str = raw.toString();
  switch (str) {
    case 'Running':
    case '0':
      return WorkflowStatus.running;
    case 'PausedForApproval':
    case '1':
      return WorkflowStatus.pausedForApproval;
    case 'Approved':
    case '2':
      return WorkflowStatus.approved;
    case 'Rejected':
    case '3':
      return WorkflowStatus.rejected;
    case 'RevisionRequested':
    case '4':
      return WorkflowStatus.revisionRequested;
    case 'Completed':
    case '5':
      return WorkflowStatus.completed;
    case 'SafeFailed':
    case '6':
      return WorkflowStatus.safeFailed;
    case 'Cancelled':
    case '7':
      return WorkflowStatus.cancelled;
    default:
      return WorkflowStatus.unknown;
  }
}

String getStatusDisplayName(WorkflowStatus status) {
  switch (status) {
    case WorkflowStatus.running:
      return 'Running';
    case WorkflowStatus.pausedForApproval:
      return 'Paused for Human Review';
    case WorkflowStatus.approved:
      return 'Approved';
    case WorkflowStatus.rejected:
      return 'Rejected';
    case WorkflowStatus.revisionRequested:
      return 'Revision Requested';
    case WorkflowStatus.completed:
      return 'Completed';
    case WorkflowStatus.safeFailed:
      return 'Safe-Failed';
    case WorkflowStatus.cancelled:
      return 'Cancelled';
    default:
      return 'Unknown';
  }
}

Color getStatusColor(WorkflowStatus status) {
  switch (status) {
    case WorkflowStatus.running:
      return Colors.blue;
    case WorkflowStatus.pausedForApproval:
      return Colors.amber.shade800;
    case WorkflowStatus.approved:
    case WorkflowStatus.completed:
      return Colors.green.shade700;
    case WorkflowStatus.rejected:
    case WorkflowStatus.safeFailed:
    case WorkflowStatus.cancelled:
      return Colors.red.shade700;
    case WorkflowStatus.revisionRequested:
      return Colors.orange.shade800;
    default:
      return Colors.grey;
  }
}

IconData getStatusIcon(WorkflowStatus status) {
  switch (status) {
    case WorkflowStatus.running:
      return Icons.sync;
    case WorkflowStatus.pausedForApproval:
      return Icons.pause_circle_filled;
    case WorkflowStatus.approved:
      return Icons.check_circle;
    case WorkflowStatus.completed:
      return Icons.task_alt;
    case WorkflowStatus.rejected:
      return Icons.cancel;
    case WorkflowStatus.safeFailed:
      return Icons.warning_amber_rounded;
    case WorkflowStatus.revisionRequested:
      return Icons.edit_note;
    case WorkflowStatus.cancelled:
      return Icons.stop_circle;
    default:
      return Icons.help_outline;
  }
}

class WorkflowAuditSummary {
  final int id;
  final String agentName;
  final String toolName;
  final String actionSummary;
  final bool isViolation;
  final String createdAt;

  WorkflowAuditSummary({
    required this.id,
    required this.agentName,
    required this.toolName,
    required this.actionSummary,
    required this.isViolation,
    required this.createdAt,
  });

  factory WorkflowAuditSummary.fromJson(Map<String, dynamic> json) {
    return WorkflowAuditSummary(
      id: json['id'] ?? 0,
      agentName: json['agentName'] ?? 'Agent',
      toolName: json['toolName'] ?? 'System',
      actionSummary: json['actionSummary'] ?? json['details'] ?? '',
      isViolation: json['isViolation'] ?? false,
      createdAt: json['createdAt'] ?? '',
    );
  }
}

class EmergencyAlert {
  final int id;
  final String objective;
  final WorkflowStatus status;
  final bool requiresHumanApproval;
  final String correlationId;
  final String contractVersion;
  final String riskLevel; // "Emergency", "High", "Medium", "Low"
  final String planSummary;
  final String validationSummary;
  final String finalOutcome;
  final String? errorCode;
  final String? errorMessage;
  final int? appointmentId;
  final String createdAt;
  final List<WorkflowAuditSummary> auditLogs;

  EmergencyAlert({
    required this.id,
    required this.objective,
    required this.status,
    required this.requiresHumanApproval,
    required this.correlationId,
    required this.contractVersion,
    required this.riskLevel,
    required this.planSummary,
    required this.validationSummary,
    required this.finalOutcome,
    this.errorCode,
    this.errorMessage,
    this.appointmentId,
    required this.createdAt,
    required this.auditLogs,
  });

  factory EmergencyAlert.fromJson(Map<String, dynamic> json) {
    return EmergencyAlert(
      id: json['id'] ?? json['workflowId'] ?? 0,
      objective: json['objective'] ?? 'Safety Audit Alert',
      status: parseWorkflowStatus(json['status']),
      requiresHumanApproval: json['requiresHumanApproval'] ?? false,
      correlationId: json['correlationId'] ?? '',
      contractVersion: json['contractVersion'] ?? 'v1',
      riskLevel: json['riskLevel'] ?? 'High',
      planSummary: json['planSummary'] ?? '',
      validationSummary: json['validationSummary'] ?? '',
      finalOutcome: json['finalOutcome'] ?? '',
      errorCode: json['errorCode'],
      errorMessage: json['errorMessage'],
      appointmentId: json['appointmentId'],
      createdAt: json['createdAt'] ?? '',
      auditLogs: (json['auditLogs'] as List<dynamic>?)
              ?.map((e) => WorkflowAuditSummary.fromJson(e))
              .toList() ??
          [],
    );
  }

  bool get isEmergency => riskLevel.toLowerCase() == 'emergency';
  bool get isHighRisk => riskLevel.toLowerCase() == 'high' || isEmergency;

  Color get riskColor {
    switch (riskLevel.toLowerCase()) {
      case 'emergency':
        return const Color(0xFFD32F2F); // Deep Red
      case 'high':
        return const Color(0xFFE65100); // Dark Orange
      case 'medium':
        return const Color(0xFFF57C00); // Orange
      case 'low':
        return const Color(0xFF388E3C); // Green
      default:
        return Colors.blueGrey;
    }
  }

  IconData get riskIcon {
    switch (riskLevel.toLowerCase()) {
      case 'emergency':
        return Icons.emergency;
      case 'high':
        return Icons.warning_amber_sharp;
      case 'medium':
        return Icons.info_outline;
      default:
        return Icons.check_circle_outline;
    }
  }

  String get requiredActionText {
    if (status == WorkflowStatus.pausedForApproval) {
      return 'Action Required: Clinical Admin approval pending for emergency safety review.';
    } else if (status == WorkflowStatus.safeFailed) {
      return 'Critical: Workflow triggered safe-failure due to safety constraint violation.';
    } else if (status == WorkflowStatus.approved) {
      return 'Resolved: Admin has approved the clinical workflow execution.';
    } else if (status == WorkflowStatus.rejected) {
      return 'Terminated: Admin rejected workflow due to clinical risk.';
    } else if (status == WorkflowStatus.revisionRequested) {
      return 'Revision: Admin requested adjustments before execution.';
    }
    return 'Status: ${getStatusDisplayName(status)}';
  }
}
