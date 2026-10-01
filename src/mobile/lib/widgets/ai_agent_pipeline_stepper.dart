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
        color: const Color(0xFF1E293B),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.cyan.withValues(alpha: 0.3)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.3),
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
                  Icon(Icons.smart_toy, color: Colors.cyanAccent, size: 20),
                  SizedBox(width: 8),
                  Text(
                    'Autonomous AI Agent Pipeline',
                    style: TextStyle(
                      color: Colors.white,
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
                    color: Colors.cyan.withValues(alpha: 0.2),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    'Appt #$appointmentId',
                    style: const TextStyle(
                      color: Colors.cyanAccent,
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
            const Divider(color: Colors.white12, height: 24),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.green.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.green.withValues(alpha: 0.3)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: const [
                      Icon(Icons.verified, color: Colors.greenAccent, size: 18),
                      SizedBox(width: 6),
                      Text(
                        'AI Orchestration Complete & Safe',
                        style: TextStyle(
                          color: Colors.greenAccent,
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
                      style: const TextStyle(color: Colors.white, fontSize: 13),
                    ),
                  ],
                  if (scheduleTime != null) ...[
                    const SizedBox(height: 2),
                    Text(
                      'Schedule: $scheduleTime',
                      style: const TextStyle(color: Colors.white70, fontSize: 12),
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
        iconColor = Colors.greenAccent;
        statusIcon = const Icon(Icons.check_circle, color: Colors.greenAccent, size: 20);
        break;
      case WorkflowStepStatus.inProgress:
        iconColor = Colors.cyanAccent;
        statusIcon = const SizedBox(
          width: 16,
          height: 16,
          child: CircularProgressIndicator(color: Colors.cyanAccent, strokeWidth: 2),
        );
        break;
      case WorkflowStepStatus.failed:
        iconColor = Colors.redAccent;
        statusIcon = const Icon(Icons.error, color: Colors.redAccent, size: 20);
        break;
      case WorkflowStepStatus.pending:
        iconColor = Colors.white38;
        statusIcon = const Icon(Icons.radio_button_unchecked, color: Colors.white38, size: 20);
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
                        color: step.status == WorkflowStepStatus.pending ? Colors.white54 : Colors.white,
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
                      color: Colors.white70,
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
