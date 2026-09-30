import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/doctor_scheduling_models.dart';
import '../../providers/auth_provider.dart';
import '../../services/doctor_scheduling_service.dart';
import '../../widgets/notification_bell_widget.dart';
import '../doctor_scheduling/consultation_attendance_screen.dart';

class DoctorDashboardScreen extends StatefulWidget {
  final DoctorProfile? doctorProfile;

  const DoctorDashboardScreen({super.key, this.doctorProfile});

  @override
  State<DoctorDashboardScreen> createState() => _DoctorDashboardScreenState();
}

class _DoctorDashboardScreenState extends State<DoctorDashboardScreen> {
  int _currentTabIndex = 0;
  late DoctorProfile _profile;

  // Tab 1 Data: Schedules & Queue
  List<DoctorScheduleModel> _schedules = [];
  List<PatientQueueItem> _patients = [];
  bool _isLoadingAppointments = true;
  String? _appointmentsError;

  // Tab 2 Data: Leaves
  List<DoctorLeaveModel> _leaves = [];
  bool _isLoadingLeaves = true;
  bool _isSubmittingLeave = false;
  String? _leaveError;

  final TextEditingController _leaveReasonController = TextEditingController();
  DateTime? _leaveStartDate;
  DateTime? _leaveEndDate;

  @override
  void initState() {
    super.initState();
    _initDoctorProfile();
    _loadUpcomingAppointments();
    _loadDoctorLeaves();
  }

  void _initDoctorProfile() {
    final authUser = Provider.of<AuthProvider>(context, listen: false).currentUser;
    _profile = widget.doctorProfile ??
        DoctorProfile(
          id: authUser?.id ?? 1,
          name: authUser?.fullName.startsWith('Dr.') == true
              ? authUser!.fullName
              : 'Dr. ${authUser?.fullName ?? 'Sarah Jenkins'}',
          specialty: 'Cardiology',
          email: authUser?.email ?? 'doctor@channelcenter.hospital',
        );
  }

  @override
  void dispose() {
    _leaveReasonController.dispose();
    super.dispose();
  }

  Future<void> _loadUpcomingAppointments() async {
    setState(() {
      _isLoadingAppointments = true;
      _appointmentsError = null;
    });

    try {
      final schedules = await DoctorSchedulingService.getDoctorSchedules(_profile.id);
      final patients = await DoctorSchedulingService.getPatientQueue(_profile.id);

      if (!mounted) return;
      setState(() {
        _schedules = schedules;
        _patients = patients;
        _isLoadingAppointments = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isLoadingAppointments = false;
        _appointmentsError = 'Failed to load upcoming appointments: $e';
      });
    }
  }

  Future<void> _loadDoctorLeaves() async {
    setState(() {
      _isLoadingLeaves = true;
      _leaveError = null;
    });

    try {
      final leaves = await DoctorSchedulingService.getDoctorLeaves(_profile.id);
      if (!mounted) return;
      setState(() {
        _leaves = leaves;
        _isLoadingLeaves = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isLoadingLeaves = false;
        _leaveError = 'Failed to load leave records: $e';
      });
    }
  }

  Future<void> _submitLeaveRequest() async {
    if (_leaveStartDate == null || _leaveEndDate == null || _leaveReasonController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select start date, end date, and enter a reason.')),
      );
      return;
    }

    setState(() {
      _isSubmittingLeave = true;
    });

    try {
      await DoctorSchedulingService.submitDoctorLeave(
        doctorId: _profile.id,
        startDate: _leaveStartDate!,
        endDate: _leaveEndDate!,
        reason: _leaveReasonController.text.trim(),
      );

      if (!mounted) return;
      _leaveReasonController.clear();
      _leaveStartDate = null;
      _leaveEndDate = null;
      _isSubmittingLeave = false;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Leave application submitted successfully.'),
          backgroundColor: Colors.green,
        ),
      );

      _loadDoctorLeaves();
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSubmittingLeave = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to submit leave: $e'), backgroundColor: Colors.red),
      );
    }
  }

  void _toggleSession(DoctorScheduleModel schedule) {
    if (schedule.isExpired) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cannot start session: Schedule has expired.'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() {
      schedule.isSessionActive = !schedule.isSessionActive;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          schedule.isSessionActive
              ? 'Channel Session STARTED in ${schedule.roomName}'
              : 'Channel Session ENDED',
        ),
        backgroundColor: schedule.isSessionActive ? Colors.green : Colors.orange,
      ),
    );
  }

  void _confirmLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Doctor Portal Logout'),
        content: const Text('Are you sure you want to log out of your Doctor account?'),
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
    return Scaffold(
      backgroundColor: Colors.grey.shade50,
      appBar: AppBar(
        title: const Text('Doctor Portal'),
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
        elevation: 1,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh',
            onPressed: _currentTabIndex == 0 ? _loadUpcomingAppointments : _loadDoctorLeaves,
          ),
          const NotificationBellWidget(),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white),
            tooltip: 'Logout',
            onPressed: _confirmLogout,
          ),
        ],
      ),
      body: IndexedStack(
        index: _currentTabIndex,
        children: [
          _buildUpcomingAppointmentsTab(),
          _buildApplyLeaveTab(),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentTabIndex,
        selectedItemColor: Colors.indigo,
        unselectedItemColor: Colors.grey.shade600,
        onTap: (index) => setState(() => _currentTabIndex = index),
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.calendar_month_outlined),
            activeIcon: Icon(Icons.calendar_month),
            label: 'Upcoming Appointments',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.event_busy_outlined),
            activeIcon: Icon(Icons.event_busy),
            label: 'Apply Leave',
          ),
        ],
      ),
    );
  }

  // --- TAB 1: UPCOMING APPOINTMENTS DETAILS ---
  Widget _buildUpcomingAppointmentsTab() {
    final activeSchedules = _schedules.where((s) => !s.isExpired).toList();

    return RefreshIndicator(
      onRefresh: _loadUpcomingAppointments,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Doctor Profile Info Card
            Card(
              color: Colors.indigo.shade50,
              elevation: 0,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  children: [
                    CircleAvatar(
                      radius: 26,
                      backgroundColor: Colors.indigo.shade200,
                      child: const Icon(Icons.medical_services, color: Colors.indigo, size: 28),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            _profile.name,
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.indigo),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Specialty: ${_profile.specialty}',
                            style: TextStyle(color: Colors.grey.shade800, fontWeight: FontWeight.w600, fontSize: 13),
                          ),
                          Text(
                            _profile.email,
                            style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),

            if (_appointmentsError != null) ...[
              Card(
                color: Colors.red.shade50,
                child: Padding(
                  padding: const EdgeInsets.all(12.0),
                  child: Text(_appointmentsError!, style: const TextStyle(color: Colors.red)),
                ),
              ),
              const SizedBox(height: 16),
            ],

            if (_isLoadingAppointments)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 40),
                child: Center(child: CircularProgressIndicator(color: Colors.indigo)),
              )
            else ...[
              // Active Channel Session Cards
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Active Channel Sessions',
                    style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
                  ),
                  Chip(
                    label: Text('${activeSchedules.length} Sessions'),
                    backgroundColor: Colors.indigo.shade100,
                  ),
                ],
              ),
              const SizedBox(height: 10),

              if (activeSchedules.isEmpty)
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Center(
                      child: Text(
                        'No active channel sessions currently scheduled.',
                        style: TextStyle(color: Colors.grey.shade600),
                      ),
                    ),
                  ),
                )
              else
                ...activeSchedules.map((schedule) => _buildScheduleCard(schedule)),

              const SizedBox(height: 24),

              // Upcoming Patient Appointments List
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Upcoming Patient Appointments',
                    style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
                  ),
                  Text(
                    '${_patients.length} Patient(s)',
                    style: const TextStyle(color: Colors.green, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              const SizedBox(height: 12),

              if (_patients.isEmpty)
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(20.0),
                    child: Center(
                      child: Column(
                        children: [
                          Icon(Icons.event_available, size: 40, color: Colors.grey.shade400),
                          const SizedBox(height: 8),
                          Text(
                            'No upcoming patient appointments for your schedule.',
                            style: TextStyle(color: Colors.grey.shade600),
                          ),
                        ],
                      ),
                    ),
                  ),
                )
              else
                ListView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: _patients.length,
                  itemBuilder: (context, index) {
                    final patient = _patients[index];
                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      elevation: 2,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      child: Padding(
                        padding: const EdgeInsets.all(14.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Row(
                                  children: [
                                    Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                      decoration: BoxDecoration(
                                        color: Colors.indigo,
                                        borderRadius: BorderRadius.circular(20),
                                      ),
                                      child: Text(
                                        'Queue #${patient.queueNumber}',
                                        style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 12),
                                      ),
                                    ),
                                    const SizedBox(width: 8),
                                    Text(
                                      'Apt #${patient.appointmentId}',
                                      style: TextStyle(fontSize: 12, color: Colors.grey.shade600, fontWeight: FontWeight.bold),
                                    ),
                                  ],
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                  decoration: BoxDecoration(
                                    color: patient.attendanceStatus == AttendanceStatus.present
                                        ? Colors.green.shade100
                                        : Colors.amber.shade100,
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                  child: Text(
                                    patient.attendanceStatus.name.toUpperCase(),
                                    style: TextStyle(
                                      color: patient.attendanceStatus == AttendanceStatus.present ? Colors.green.shade800 : Colors.amber.shade900,
                                      fontSize: 11,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 10),
                            Text(
                              patient.patientName,
                              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                            ),
                            const SizedBox(height: 6),
                            Row(
                              children: [
                                const Icon(Icons.notes, size: 16, color: Colors.indigo),
                                const SizedBox(width: 6),
                                Expanded(
                                  child: Text(
                                    'Symptoms: ${patient.symptoms.isNotEmpty ? patient.symptoms : "Standard Consultation"}',
                                    style: TextStyle(fontSize: 13, color: Colors.grey.shade800),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton.icon(
                                icon: const Icon(Icons.check_circle_outline, size: 18),
                                label: const Text('Update Attendance / Consultation'),
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: Colors.indigo,
                                  foregroundColor: Colors.white,
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                                ),
                                onPressed: () async {
                                  await Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (_) => ConsultationAttendanceScreen(patient: patient),
                                    ),
                                  );
                                  _loadUpcomingAppointments();
                                },
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildScheduleCard(DoctorScheduleModel schedule) {
    return Card(
      elevation: 2,
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(color: Colors.indigo.shade100),
      ),
      child: Padding(
        padding: const EdgeInsets.all(14.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  schedule.roomName,
                  style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.indigo),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: schedule.isSessionActive ? Colors.green : Colors.orange,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    schedule.statusText,
                    style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                  ),
                ),
              ],
            ),
            const Divider(height: 16),
            Text(
              'Time: ${_formatTime(schedule.startTime)} - ${_formatTime(schedule.endTime)}',
              style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600),
            ),
            const SizedBox(height: 10),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => _toggleSession(schedule),
                style: ElevatedButton.styleFrom(
                  backgroundColor: schedule.isSessionActive ? Colors.red : Colors.green,
                  foregroundColor: Colors.white,
                ),
                child: Text(
                  schedule.isSessionActive ? 'End Session' : 'Start Session',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  // --- TAB 2: APPLY LEAVE SCREEN ---
  Widget _buildApplyLeaveTab() {
    return RefreshIndicator(
      onRefresh: _loadDoctorLeaves,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Leave Application Form Card
            Card(
              elevation: 2,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Apply for Doctor Leave',
                      style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Submit leave requests to temporarily pause patient bookings.',
                      style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                    ),
                    const SizedBox(height: 16),

                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton.icon(
                            icon: const Icon(Icons.calendar_today, size: 18),
                            label: Text(_leaveStartDate == null
                                ? 'Start Date'
                                : '${_leaveStartDate!.day}/${_leaveStartDate!.month}/${_leaveStartDate!.year}'),
                            onPressed: () async {
                              final picked = await showDatePicker(
                                context: context,
                                initialDate: DateTime.now().add(const Duration(days: 1)),
                                firstDate: DateTime.now(),
                                lastDate: DateTime.now().add(const Duration(days: 90)),
                              );
                              if (picked != null) setState(() => _leaveStartDate = picked);
                            },
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: OutlinedButton.icon(
                            icon: const Icon(Icons.calendar_today, size: 18),
                            label: Text(_leaveEndDate == null
                                ? 'End Date'
                                : '${_leaveEndDate!.day}/${_leaveEndDate!.month}/${_leaveEndDate!.year}'),
                            onPressed: () async {
                              final picked = await showDatePicker(
                                context: context,
                                initialDate: DateTime.now().add(const Duration(days: 2)),
                                firstDate: DateTime.now(),
                                lastDate: DateTime.now().add(const Duration(days: 90)),
                              );
                              if (picked != null) setState(() => _leaveEndDate = picked);
                            },
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _leaveReasonController,
                      maxLines: 2,
                      decoration: const InputDecoration(
                        labelText: 'Reason for Leave',
                        hintText: 'e.g. Medical Conference, Personal Leave',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.indigo,
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(vertical: 12),
                        ),
                        onPressed: _isSubmittingLeave ? null : _submitLeaveRequest,
                        child: _isSubmittingLeave
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                              )
                            : const Text('Submit Leave Application', style: TextStyle(fontWeight: FontWeight.bold)),
                      ),
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: 24),

            const Text(
              'My Leave Applications History',
              style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
            ),
            const SizedBox(height: 10),

            if (_leaveError != null)
              Text(_leaveError!, style: const TextStyle(color: Colors.red))
            else if (_isLoadingLeaves)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 20),
                child: Center(child: CircularProgressIndicator(color: Colors.indigo)),
              )
            else if (_leaves.isEmpty)
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Center(
                    child: Text(
                      'No previous leave applications recorded.',
                      style: TextStyle(color: Colors.grey.shade600),
                    ),
                  ),
                ),
              )
            else
              ListView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: _leaves.length,
                itemBuilder: (context, index) {
                  final item = _leaves[index];
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    child: ListTile(
                      title: Text(
                        '${item.startDate.day}/${item.startDate.month}/${item.startDate.year} - ${item.endDate.day}/${item.endDate.month}/${item.endDate.year}',
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      subtitle: Text(item.reason),
                      trailing: Chip(
                        label: Text(item.status.name.toUpperCase()),
                        backgroundColor: item.status == LeaveStatus.approved
                            ? Colors.green.shade100
                            : item.status == LeaveStatus.rejected
                                ? Colors.red.shade100
                                : Colors.amber.shade100,
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

  String _formatTime(DateTime dt) {
    final hour = dt.hour % 12 == 0 ? 12 : dt.hour % 12;
    final minute = dt.minute.toString().padLeft(2, '0');
    final period = dt.hour >= 12 ? 'PM' : 'AM';
    return '$hour:$minute $period';
  }
}
