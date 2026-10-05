import 'package:flutter/material.dart';
import '../models/triage_models.dart';

class AiAgentPipelineStepper extends StatelessWidget {
  final List<AgentWorkflowStepProgress> steps;
  final int? appointmentId;
  final String? assignedDoctor;
  final String? scheduleTime;
  final bool isSafetyVerified;

  const AiAgentPipelineStepper({
    super.key,
    required this.steps,
    this.appointmentId,
    this.assignedDoctor,
    this.scheduleTime,
    this.isSafetyVerified = false,
  });

  @override
  Widget build(BuildContext context) {
    if (steps.isEmpty) return const SizedBox.shrink();

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFFE2E8F0)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 10,
            offset: const Offset(0, 4),
          )
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: const [
                  Icon(Icons.smart_toy, color: Color(0xFF2563EB), size: 20),
                  SizedBox(width: 8),
                  Text(
                    'Autonomous AI Agent Pipeline',
                    style: TextStyle(
                      color: Color(0xFF0F172A),
                      fontWeight: FontWeight.bold,
                      fontSize: 15,
                    ),
                  ),
                ],
              ),
              if (appointmentId != null && appointmentId! > 0)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: const Color(0xFFDBEAFE),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    'Appt #$appointmentId',
                    style: const TextStyle(
                      color: Color(0xFF1E40AF),
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 14),

          // Render 5 steps
          ...steps.map((step) => _buildStepTile(step)),

          if (assignedDoctor != null || scheduleTime != null || isSafetyVerified) ...[
            const Divider(color: Color(0xFFE2E8F0), height: 24),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xFFF0FDF4),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: const Color(0xFF86EFAC)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: const [
                      Icon(Icons.verified, color: Color(0xFF16A34A), size: 18),
                      SizedBox(width: 6),
                      Text(
                        'AI Orchestration Complete & Safe',
                        style: TextStyle(
                          color: Color(0xFF15803D),
                          fontWeight: FontWeight.bold,
                          fontSize: 13,
                        ),
                      ),
                    ],
                  ),
                  if (assignedDoctor != null) ...[
                    const SizedBox(height: 6),
                    Text(
                      'Assigned Doctor: $assignedDoctor',
                      style: const TextStyle(color: Color(0xFF0F172A), fontSize: 13, fontWeight: FontWeight.w600),
                    ),
                  ],
                  if (scheduleTime != null) ...[
                    const SizedBox(height: 2),
                    Text(
                      'Schedule: $scheduleTime',
                      style: const TextStyle(color: Color(0xFF475569), fontSize: 12),
                    ),
                  ],
                ],
              ),
            ),
          ]
        ],
      ),
    );
  }

  Widget _buildStepTile(AgentWorkflowStepProgress step) {
    Color iconColor;
    Widget statusIcon;

    switch (step.status) {
      case WorkflowStepStatus.completed:
        iconColor = const Color(0xFF16A34A);
        statusIcon = const Icon(Icons.check_circle, color: Color(0xFF16A34A), size: 20);
        break;
      case WorkflowStepStatus.inProgress:
        iconColor = const Color(0xFF2563EB);
        statusIcon = const SizedBox(
          width: 16,
          height: 16,
          child: CircularProgressIndicator(color: Color(0xFF2563EB), strokeWidth: 2),
        );
        break;
      case WorkflowStepStatus.failed:
        iconColor = const Color(0xFFDC2626);
        statusIcon = const Icon(Icons.error, color: Color(0xFFDC2626), size: 20);
        break;
      case WorkflowStepStatus.pending:
        iconColor = const Color(0xFF94A3B8);
        statusIcon = const Icon(Icons.radio_button_unchecked, color: Color(0xFF94A3B8), size: 20);
        break;
    }

    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.all(2),
            child: statusIcon,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'Stage ${step.stepNumber}: ${step.title}',
                      style: TextStyle(
                        color: step.status == WorkflowStepStatus.pending ? const Color(0xFF64748B) : const Color(0xFF0F172A),
                        fontWeight: FontWeight.bold,
                        fontSize: 13,
                      ),
                    ),
                    Text(
                      step.agentName,
                      style: TextStyle(
                        color: iconColor,
                        fontSize: 10,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
                if (step.detail != null && step.detail!.isNotEmpty) ...[
                  const SizedBox(height: 3),
                  Text(
                    step.detail!,
                    style: const TextStyle(
                      color: Color(0xFF475569),
                      fontSize: 11,
                      height: 1.3,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
