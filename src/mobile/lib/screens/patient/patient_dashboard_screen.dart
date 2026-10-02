import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../models/triage_models.dart';
import '../../models/workflow_models.dart';
import '../../providers/auth_provider.dart';
import '../../providers/triage_provider.dart';
import '../../providers/workflow_provider.dart';
import '../../widgets/notification_bell_widget.dart';
import '../../widgets/urgency_badge.dart';
import '../../widgets/ai_agent_pipeline_stepper.dart';
import '../triage_status_screen.dart';

class PatientDashboardScreen extends StatefulWidget {
  const PatientDashboardScreen({super.key});

  @override
  State<PatientDashboardScreen> createState() => _PatientDashboardScreenState();
}

class _PatientDashboardScreenState extends State<PatientDashboardScreen> {
  int _currentTabIndex = 0;

  // Form State for Request Appointment Tab
  final _symptomsController = TextEditingController();
  final _historyController = TextEditingController();
  final _allergiesController = TextEditingController();
  double _severityRating = 5.0;
  DateTime _onsetDate = DateTime.now();

  bool _isSubmitting = false;

  @override
  void initState() {
    super.initState();
    _refreshAppointmentsStatus();
  }

  @override
  void dispose() {
    _symptomsController.dispose();
    _historyController.dispose();
    _allergiesController.dispose();
    super.dispose();
  }

  Future<void> _refreshAppointmentsStatus() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final triageProvider = Provider.of<TriageProvider>(context, listen: false);
    final workflowProvider = Provider.of<WorkflowProvider>(context, listen: false);

    final patientId = authProvider.currentUser?.patientId ?? authProvider.currentUser?.id ?? 1;
    await triageProvider.fetchPatientAppointments(patientId);
    await workflowProvider.fetchWorkflows();
  }


  Future<void> _submitSymptomRequest() async {
    final rawSymptoms = _symptomsController.text.trim();
    if (rawSymptoms.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please describe your symptoms before submitting.'),
          backgroundColor: Colors.orange,
        ),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      final triageProvider = Provider.of<TriageProvider>(context, listen: false);
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      final patientId = authProvider.currentUser?.patientId ?? authProvider.currentUser?.id ?? 1;
      final authToken = authProvider.token;

      final success = await triageProvider.executeCompleteMobileWorkflow(
        patientId: patientId,
        reasonForVisit: rawSymptoms,
        medicalHistory: _historyController.text.trim(),
        allergies: _allergiesController.text.trim(),
        symptomList: [
          SymptomItem(
            symptomKeyword: rawSymptoms.length > 30 ? rawSymptoms.substring(0, 30) : rawSymptoms,
            severityRating: _severityRating.toInt(),
            durationInDays: 3,
          )
        ],
        authToken: authToken,
      );

      if (!mounted) return;
      setState(() => _isSubmitting = false);

      if (success) {
        _symptomsController.clear();
        _historyController.clear();
        _allergiesController.clear();

        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('AI Agent workflow finished! Appointment registered & verified.'),
            backgroundColor: Colors.green,
          ),
        );

        // Switch to Status tab to view live pipeline results
        setState(() => _currentTabIndex = 1);
        _refreshAppointmentsStatus();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(triageProvider.errorMessage ?? 'Submission failed. Please try again.'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSubmitting = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error: $e'), backgroundColor: Colors.red),
      );
    }
  }

  void _confirmLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Patient Portal Logout'),
        content: const Text('Are you sure you want to log out?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              Provider.of<AuthProvider>(context, listen: false).logout();
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Log Out', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authUser = Provider.of<AuthProvider>(context).currentUser;

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      appBar: AppBar(
        backgroundColor: const Color(0xFF1E293B),
        foregroundColor: Colors.white,
        elevation: 0,
        title: Row(
          children: [
            const Icon(Icons.favorite, color: Colors.cyanAccent),
            const SizedBox(width: 8),
            Text(
              authUser != null ? authUser.fullName : 'Patient Portal',
              style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Status',
            onPressed: _refreshAppointmentsStatus,
          ),
          const NotificationBellWidget(),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.redAccent),
            tooltip: 'Logout',
            onPressed: _confirmLogout,
          ),
        ],
      ),
      body: IndexedStack(
        index: _currentTabIndex,
        children: [
          _buildRequestAppointmentTab(),
          _buildAppointmentStatusTab(),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentTabIndex,
        backgroundColor: const Color(0xFF1E293B),
        selectedItemColor: Colors.cyanAccent,
        unselectedItemColor: Colors.white54,
        onTap: (index) => setState(() => _currentTabIndex = index),
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.add_task_outlined),
            activeIcon: Icon(Icons.add_task),
            label: 'Make Appointment',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.analytics_outlined),
            activeIcon: Icon(Icons.analytics),
            label: 'Appointment Status',
          ),
        ],
      ),
    );
  }

  // --- TAB 1: MAKE APPOINTMENT (SYMPTOMS REQUEST) ---
  Widget _buildRequestAppointmentTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // AI AI Agent Banner Explanation
          Container(
            padding: const EdgeInsets.all(16.0),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF0284C7), Color(0xFF0369A1)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Row(
              children: [
                const Icon(Icons.smart_toy_outlined, color: Colors.white, size: 36),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: const [
                      Text(
                        'AI Autonomous Scheduling',
                        style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                      ),
                      SizedBox(height: 4),
                      Text(
                        'Describe your symptoms below. AI Agents will triage urgency, select the appropriate specialist, and finalize your appointment slot automatically.',
                        style: TextStyle(fontSize: 12, color: Colors.white70, height: 1.3),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 20),

          const Text(
            'Describe Your Symptoms',
            style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.white),
          ),
          const SizedBox(height: 6),
          const Text(
            'Be as detailed as possible (e.g., severity, location, duration).',
            style: TextStyle(fontSize: 12, color: Colors.white60),
          ),
          const SizedBox(height: 10),

          // Symptoms Text Field
          TextFormField(
            controller: _symptomsController,
            maxLines: 4,
            style: const TextStyle(color: Colors.white),
            decoration: InputDecoration(
              hintText: 'e.g. Sharp chest pain radiating to left arm, shortness of breath for 2 days...',
              hintStyle: const TextStyle(color: Colors.white38),
              filled: true,
              fillColor: const Color(0xFF1E293B),
              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
              enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.white12)),
              focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.cyan)),
            ),
          ),

          const SizedBox(height: 18),

          // Severity Rating Slider
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Symptom Severity Rating',
                style: TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.cyan.withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  '${_severityRating.toInt()}/10',
                  style: const TextStyle(color: Colors.cyanAccent, fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          Slider(
            value: _severityRating,
            min: 1,
            max: 10,
            divisions: 9,
            activeColor: Colors.cyan,
            inactiveColor: Colors.white12,
            onChanged: (val) => setState(() => _severityRating = val),
          ),

          const SizedBox(height: 12),

          // Additional Optional Details
          Theme(
            data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
            child: ExpansionTile(
              title: const Text('Optional Medical Background', style: TextStyle(color: Colors.cyanAccent, fontSize: 14)),
              tilePadding: EdgeInsets.zero,
              children: [
                TextFormField(
                  controller: _historyController,
                  style: const TextStyle(color: Colors.white),
                  decoration: _inputDecoration('Existing Medical Conditions (e.g. Diabetes, Asthma)'),
                ),
                const SizedBox(height: 10),
                TextFormField(
                  controller: _allergiesController,
                  style: const TextStyle(color: Colors.white),
                  decoration: _inputDecoration('Allergies (e.g. Penicillin)'),
                ),
                const SizedBox(height: 10),
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Symptom Onset Date', style: TextStyle(color: Colors.white70, fontSize: 13)),
                  subtitle: Text(DateFormat('yyyy-MM-dd').format(_onsetDate), style: const TextStyle(color: Colors.cyanAccent)),
                  trailing: const Icon(Icons.calendar_today, color: Colors.cyan),
                  onTap: () async {
                    final picked = await showDatePicker(
                      context: context,
                      initialDate: _onsetDate,
                      firstDate: DateTime(2020),
                      lastDate: DateTime.now(),
                    );
                    if (picked != null) setState(() => _onsetDate = picked);
                  },
                ),
              ],
            ),
          ),

          const SizedBox(height: 24),

          // Submit Button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              icon: _isSubmitting
                  ? const SizedBox(height: 18, width: 18, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                  : const Icon(Icons.send_rounded),
              label: Text(
                _isSubmitting ? 'AI Agent Processing...' : 'Submit Symptoms for AI Appointment',
                style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
              ),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.cyan,
                foregroundColor: Colors.black,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              onPressed: _isSubmitting ? null : _submitSymptomRequest,
            ),
          ),
          const SizedBox(height: 20),
        ],
      ),
    );
  }

  void _openAppointmentStatus(PatientAppointment appt) async {
    final triageProvider = Provider.of<TriageProvider>(context, listen: false);
    await triageProvider.selectAppointment(appt);
    if (!mounted) return;
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => TriageStatusScreen(appointment: appt),
      ),
    );
  }

  // --- TAB 2: APPOINTMENT & AI WORKFLOW STATUS ---
  Widget _buildAppointmentStatusTab() {
    final triageProvider = Provider.of<TriageProvider>(context);

    final patientAppointments = triageProvider.patientAppointments;

    return RefreshIndicator(
      onRefresh: _refreshAppointmentsStatus,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: const [
                      Text(
                        'My Patient Appointments',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                      ),
                      SizedBox(height: 2),
                      Text(
                        'Tap any appointment card to view its AI agent triage status & details.',
                        style: TextStyle(fontSize: 12, color: Colors.white60),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.refresh, color: Colors.cyanAccent),
                  onPressed: _refreshAppointmentsStatus,
                  tooltip: 'Refresh Appointments',
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Live 5-Stage AI Agent Workflow Stepper Card
            if (triageProvider.workflowSteps.isNotEmpty) ...[
              AiAgentPipelineStepper(
                steps: triageProvider.workflowSteps,
                appointmentId: triageProvider.lastCreatedAppointmentId,
                assignedDoctor: triageProvider.assignedDoctorName,
                scheduleTime: triageProvider.assignedScheduleTime,
                isSafetyVerified: triageProvider.isSafetyVerified,
              ),
              const SizedBox(height: 20),
            ],

            // Patient Appointments List
            if (patientAppointments.isEmpty)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: Colors.white12),
                ),
                child: Column(
                  children: const [
                    Icon(Icons.assignment_outlined, size: 48, color: Colors.white38),
                    SizedBox(height: 12),
                    Text('No appointments found', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                    SizedBox(height: 4),
                    Text(
                      'Use the "Make Appointment" tab to submit your symptoms.',
                      style: TextStyle(color: Colors.white54, fontSize: 12),
                    ),
                  ],
                ),
              )
            else
              ListView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: patientAppointments.length,
                itemBuilder: (context, index) {
                  final appt = patientAppointments[index];
                  final statusLower = appt.status.toLowerCase();
                  final isConfirmed = statusLower == 'confirmed' || statusLower == 'completed';
                  final isCancelled = statusLower == 'cancelled';

                  Color badgeColor = Colors.amber;
                  String badgeText = appt.status.toUpperCase();
                  if (isConfirmed) {
                    badgeColor = Colors.green;
                    badgeText = appt.status == 'Completed' ? 'COMPLETED' : 'CONFIRMED';
                  } else if (isCancelled) {
                    badgeColor = Colors.red;
                    badgeText = 'CANCELLED';
                  } else if (statusLower == 'pending') {
                    badgeText = 'PAUSED FOR APPROVAL';
                  }

                  return Card(
                    color: const Color(0xFF1E293B),
                    margin: const EdgeInsets.only(bottom: 12),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                      side: BorderSide(
                        color: isConfirmed
                            ? Colors.green.withValues(alpha: 0.4)
                            : (isCancelled
                                ? Colors.red.withValues(alpha: 0.4)
                                : Colors.amber.withValues(alpha: 0.4)),
                      ),
                    ),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(14),
                      onTap: () => _openAppointmentStatus(appt),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Row(
                                  children: [
                                    Container(
                                      padding: const EdgeInsets.all(8),
                                      decoration: BoxDecoration(
                                        color: Colors.cyan.withValues(alpha: 0.15),
                                        shape: BoxShape.circle,
                                      ),
                                      child: const Icon(Icons.calendar_month, color: Colors.cyanAccent, size: 20),
                                    ),
                                    const SizedBox(width: 10),
                                    Text(
                                      'Appointment #${appt.id}',
                                      style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16),
                                    ),
                                  ],
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: badgeColor.withValues(alpha: 0.2),
                                    borderRadius: BorderRadius.circular(10),
                                    border: Border.all(color: badgeColor),
                                  ),
                                  child: Text(
                                    badgeText,
                                    style: TextStyle(
                                      color: badgeColor,
                                      fontSize: 11,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                            const Divider(color: Colors.white12, height: 20),
                            Row(
                              children: [
                                const Icon(Icons.medical_services_outlined, size: 16, color: Colors.cyanAccent),
                                const SizedBox(width: 8),
                                Text(
                                  'Specialty: ${appt.doctorSpecialty}',
                                  style: const TextStyle(color: Colors.white70, fontSize: 13, fontWeight: FontWeight.w600),
                                ),
                              ],
                            ),
                            const SizedBox(height: 6),
                            Row(
                              children: [
                                const Icon(Icons.badge_outlined, size: 16, color: Color(0xFF34D399)),
                                const SizedBox(width: 8),
                                Text(
                                  'Doctor: ${appt.doctorName}',
                                  style: const TextStyle(color: Color(0xFF34D399), fontSize: 13, fontWeight: FontWeight.bold),
                                ),
                              ],
                            ),
                            const SizedBox(height: 6),
                            Row(
                              children: [
                                const Icon(Icons.description_outlined, size: 16, color: Colors.white38),
                                const SizedBox(width: 8),
                                Expanded(
                                  child: Text(
                                    'Symptoms: ${appt.normalizedRawSymptoms.isNotEmpty ? appt.normalizedRawSymptoms : appt.reasonForVisit}',
                                    style: const TextStyle(color: Colors.white60, fontSize: 13),
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.end,
                              children: const [
                                Text('View AI Status & Trace', style: TextStyle(color: Colors.cyanAccent, fontSize: 12, fontWeight: FontWeight.bold)),
                                SizedBox(width: 4),
                                Icon(Icons.arrow_forward_ios, color: Colors.cyanAccent, size: 12),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }


  InputDecoration _inputDecoration(String label) {
    return InputDecoration(
      labelText: label,
      labelStyle: const TextStyle(color: Colors.white70),
      filled: true,
      fillColor: const Color(0xFF1E293B),
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: BorderSide.none),
      enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: Colors.white12)),
      focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: Colors.cyan)),
    );
  }
}
