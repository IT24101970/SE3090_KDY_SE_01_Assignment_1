import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/triage_models.dart';
import '../providers/triage_provider.dart';
import '../widgets/urgency_badge.dart';
import '../widgets/ai_agent_pipeline_stepper.dart';
import 'symptom_wizard_screen.dart';
import 'triage_history_screen.dart';

class TriageStatusScreen extends StatelessWidget {
  final PatientAppointment? appointment;

  const TriageStatusScreen({super.key, this.appointment});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        title: const Text('Triage Status Tracker', style: TextStyle(color: Color(0xFF0F172A), fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        iconTheme: const IconThemeData(color: Color(0xFF2563EB)),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.history, color: Color(0xFF2563EB)),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const TriageHistoryScreen()),
              );
            },
          ),
        ],
      ),
      body: Consumer<TriageProvider>(
        builder: (context, provider, child) {
          final assessment = provider.currentAssessment;

          if (assessment == null) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.assignment_outlined, size: 64, color: Color(0xFF94A3B8)),
                  const SizedBox(height: 16),
                  const Text('No active triage assessment', style: TextStyle(color: Color(0xFF64748B), fontSize: 16)),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(builder: (_) => const SymptomWizardScreen()),
                      );
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF2563EB),
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                    ),
                    child: const Text('Start Symptom Assessment'),
                  ),
                ],
              ),
            );
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (provider.workflowSteps.isNotEmpty) ...[
                  AiAgentPipelineStepper(
                    steps: provider.workflowSteps,
                    appointmentId: provider.lastCreatedAppointmentId ?? assessment.appointmentId,
                    assignedDoctor: provider.assignedDoctorName,
                    scheduleTime: provider.assignedScheduleTime,
                    isSafetyVerified: provider.isSafetyVerified,
                  ),
                  const SizedBox(height: 20),
                ],

                if (provider.assignedDoctorName == 'Pending Doctor Assignment' || !provider.isSafetyVerified) ...[
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFFFBEB),
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: const Color(0xFFFCD34D)),
                    ),
                    child: Row(
                      children: [
                        const Icon(Icons.info_outline_rounded, color: Color(0xFFD97706), size: 28),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: const [
                              Text(
                                'No valid schedules currently available',
                                style: TextStyle(color: Color(0xFF92400E), fontWeight: FontWeight.bold, fontSize: 14),
                              ),
                              SizedBox(height: 4),
                              Text(
                                'Your appointment is registered and paused. You will receive a notification as soon as a clinical admin assigns a slot.',
                                style: TextStyle(color: Color(0xFF78350F), fontSize: 12, height: 1.3),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),
                ],

                // Urgency Card
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(20),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: const Color(0xFFE2E8F0)),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.04),
                        blurRadius: 10,
                        offset: const Offset(0, 4),
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Appointment #${assessment.appointmentId}',
                            style: const TextStyle(color: Color(0xFF64748B), fontSize: 13, fontWeight: FontWeight.bold),
                          ),
                          UrgencyBadge(
                            urgencyLevel: assessment.urgencyLevel,
                            urgencyScore: assessment.urgencyScore,
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      const Text(
                        'Recommended Clinical Specialty:',
                        style: TextStyle(color: Color(0xFF64748B), fontSize: 12),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        assessment.recommendedSpecialty,
                        style: const TextStyle(
                          color: Color(0xFF2563EB),
                          fontSize: 22,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 20),

                // Symptoms & Severity
                const Text(
                  'Recorded Symptoms',
                  style: TextStyle(color: Color(0xFF0F172A), fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 10),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: const Color(0xFFE2E8F0)),
                  ),
                  child: Text(
                    assessment.rawSymptoms,
                    style: const TextStyle(color: Color(0xFF0F172A), fontSize: 14, height: 1.4),
                  ),
                ),
                const SizedBox(height: 20),

                // AI Reasoning Trace
                const Text(
                  'AI Agent Reasoning Trace',
                  style: TextStyle(color: Color(0xFF0F172A), fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 10),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: const Color(0xFFEFF6FF),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: const Color(0xFFBFDBFE)),
                  ),
                  child: Text(
                    assessment.reasoningTrace,
                    style: const TextStyle(
                      color: Color(0xFF1E3A8A),
                      fontSize: 13,
                      fontFamily: 'monospace',
                      height: 1.5,
                    ),
                  ),
                ),
                const SizedBox(height: 30),

                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(builder: (_) => const SymptomWizardScreen()),
                      );
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF2563EB),
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    child: const Text('Start New Symptom Assessment', style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: Colors.white)),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
