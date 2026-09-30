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

    // Resolve the real Doctor table ID from the backend
    _syncRealDoctorIdFromDatabase();
  }

  /// Fetches the doctor list from the API and matches by email/name
  /// to get the correct Doctors.Id (not Users.Id) for FK-safe operations.
  Future<void> _syncRealDoctorIdFromDatabase() async {
    try {
      final doctorsList = await DoctorSchedulingService.getDoctors();
      if (!mounted || doctorsList.isEmpty) return;

      final authUser = Provider.of<AuthProvider>(context, listen: false).currentUser;
      final userEmail = authUser?.email.toLowerCase().trim() ?? _profile.email.toLowerCase().trim();

      DoctorProfile? matched;
      for (final doc in doctorsList) {
        if (doc.email.toLowerCase().trim() == userEmail ||
            doc.name.toLowerCase().contains(_profile.name.toLowerCase().replaceAll('dr.', '').trim())) {
          matched = doc;
          break;
        }
      }

      if (matched != null) {
        setState(() {
          _profile = matched!;
        });
        _loadUpcomingAppointments();
        _loadDoctorLeaves();
      }
    } catch (_) {}
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
      setState(() {
        _leaveReasonController.clear();
        _leaveStartDate = null;
        _leaveEndDate = null;
        _isSubmittingLeave = false;
      });

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
        backgroundColor: Colors.white,
        title: const Text('Doctor Portal Logout', style: TextStyle(color: Color(0xFF1E293B))),
        content: const Text('Are you sure you want to log out of your Doctor account?',
            style: TextStyle(color: Color(0xFF475569))),
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
      backgroundColor: const Color(0xFFF1F5F9), // Light slate background
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
      bottomNavigationBar: Container(
        decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(top: BorderSide(color: Color(0xFFE2E8F0), width: 1)),
        ),
        child: BottomNavigationBar(
          currentIndex: _currentTabIndex,
          backgroundColor: Colors.white,
          selectedItemColor: Colors.indigo,
          unselectedItemColor: const Color(0xFF64748B),
          selectedLabelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
          unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 12),
          elevation: 0,
          onTap: (index) => setState(() => _currentTabIndex = index),
          items: const [
            BottomNavigationBarItem(
              icon: Icon(Icons.calendar_month_outlined),
              activeIcon: Icon(Icons.calendar_month),
              label: 'Appointments',
            ),
            BottomNavigationBarItem(
              icon: Icon(Icons.event_busy_outlined),
              activeIcon: Icon(Icons.event_busy),
              label: 'Apply Leave',
            ),
          ],
        ),
      ),
    );
  }

  // --- TAB 1: UPCOMING APPOINTMENTS ---
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
            // Doctor Profile Info — Clean Light Blue Container
            Container(
              padding: const EdgeInsets.all(16.0),
              decoration: BoxDecoration(
                color: const Color(0xFFEEF2FF), // Light indigo
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: const Color(0xFFC7D2FE)),
              ),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 26,
                    backgroundColor: Colors.indigo.shade600,
                    child: const Icon(Icons.medical_services, color: Colors.white, size: 28),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          _profile.name,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Color(0xFF1E1B4B), // Dark indigo text
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          'Specialty: ${_profile.specialty}',
                          style: const TextStyle(
                            color: Color(0xFF3730A3),
                            fontWeight: FontWeight.w600,
                            fontSize: 13,
                          ),
                        ),
                        Text(
                          _profile.email,
                          style: const TextStyle(fontSize: 12, color: Color(0xFF4338CA)),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),

            if (_appointmentsError != null) ...[
              Container(
                padding: const EdgeInsets.all(12.0),
                decoration: BoxDecoration(
                  color: const Color(0xFFFEF2F2),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: const Color(0xFFFCA5A5)),
                ),
                child: Text(_appointmentsError!, style: const TextStyle(color: Colors.red, fontSize: 13)),
              ),
              const SizedBox(height: 16),
            ],

            if (_isLoadingAppointments)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 40),
                child: Center(child: CircularProgressIndicator(color: Colors.indigo)),
              )
            else ...[
              // Active Channel Sessions Header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Active Channel Sessions',
                    style: TextStyle(
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF1E293B),
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: const Color(0xFFE0E7FF),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      '${activeSchedules.length} Sessions',
                      style: const TextStyle(
                        color: Colors.indigo,
                        fontWeight: FontWeight.bold,
                        fontSize: 12,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 10),

              if (activeSchedules.isEmpty)
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: const Color(0xFFE2E8F0)),
                  ),
                  child: const Center(
                    child: Text(
                      'No active channel sessions currently scheduled.',
                      style: TextStyle(color: Color(0xFF475569), fontSize: 14, fontWeight: FontWeight.w500),
                    ),
                  ),
                )
              else
                ...activeSchedules.map((schedule) => _buildScheduleCard(schedule)),

              const SizedBox(height: 24),

              // Upcoming Patient Appointments Header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Upcoming Patient Appointments',
                    style: TextStyle(
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF1E293B),
                    ),
                  ),
                  Text(
                    '${_patients.length} Patient(s)',
                    style: const TextStyle(color: Color(0xFF16A34A), fontWeight: FontWeight.bold, fontSize: 13),
                  ),
                ],
              ),
              const SizedBox(height: 12),

              if (_patients.isEmpty)
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: const Color(0xFFE2E8F0)),
                  ),
                  child: Center(
                    child: Column(
                      children: const [
                        Icon(Icons.event_available, size: 42, color: Color(0xFF94A3B8)),
                        SizedBox(height: 8),
                        Text(
                          'No upcoming patient appointments for your schedule.',
                          style: TextStyle(color: Color(0xFF475569), fontSize: 14, fontWeight: FontWeight.w500),
                        ),
                      ],
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
                    return Container(
                      margin: const EdgeInsets.only(bottom: 12),
                      padding: const EdgeInsets.all(14.0),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: const Color(0xFFE2E8F0)),
                        boxShadow: const [
                          BoxShadow(color: Color(0x0F0F172A), blurRadius: 6, offset: Offset(0, 2)),
                        ],
                      ),
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
                                    style: const TextStyle(fontSize: 12, color: Color(0xFF64748B), fontWeight: FontWeight.bold),
                                  ),
                                ],
                              ),
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: patient.attendanceStatus == AttendanceStatus.present
                                      ? const Color(0xFFDCFCE7)
                                      : const Color(0xFFFEF3C7),
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  patient.attendanceStatus.name.toUpperCase(),
                                  style: TextStyle(
                                    color: patient.attendanceStatus == AttendanceStatus.present
                                        ? const Color(0xFF15803D)
                                        : const Color(0xFFB45309),
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
                            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF0F172A)),
                          ),
                          const SizedBox(height: 6),
                          Row(
                            children: [
                              const Icon(Icons.notes, size: 16, color: Colors.indigo),
                              const SizedBox(width: 6),
                              Expanded(
                                child: Text(
                                  'Symptoms: ${patient.symptoms.isNotEmpty ? patient.symptoms : "Standard Consultation"}',
                                  style: const TextStyle(fontSize: 13, color: Color(0xFF334155)),
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
                                padding: const EdgeInsets.symmetric(vertical: 12),
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
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(14.0),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFC7D2FE)),
        boxShadow: const [
          BoxShadow(color: Color(0x0F0F172A), blurRadius: 6, offset: Offset(0, 2)),
        ],
      ),
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
          const Divider(height: 16, color: Color(0xFFE2E8F0)),
          Text(
            'Time: ${_formatTime(schedule.startTime)} - ${_formatTime(schedule.endTime)}',
            style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: Color(0xFF1E293B)),
          ),
          const SizedBox(height: 10),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () => _toggleSession(schedule),
              style: ElevatedButton.styleFrom(
                backgroundColor: schedule.isSessionActive ? Colors.red : Colors.green,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 12),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
              ),
              child: Text(
                schedule.isSessionActive ? 'End Session' : 'Start Session',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ),
        ],
      ),
    );
  }

  // --- TAB 2: APPLY LEAVE ---
  Widget _buildApplyLeaveTab() {
    return RefreshIndicator(
      onRefresh: _loadDoctorLeaves,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Leave Application Form
            Container(
              padding: const EdgeInsets.all(16.0),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: const Color(0xFFE2E8F0)),
                boxShadow: const [
                  BoxShadow(color: Color(0x0F0F172A), blurRadius: 6, offset: Offset(0, 2)),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Apply for Doctor Leave',
                    style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
                  ),
                  const SizedBox(height: 4),
                  const Text(
                    'Submit leave requests to temporarily pause patient bookings.',
                    style: TextStyle(fontSize: 12, color: Color(0xFF64748B)),
                  ),
                  const SizedBox(height: 16),

                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton.icon(
                          icon: const Icon(Icons.calendar_today, size: 18, color: Colors.indigo),
                          label: Text(
                            _leaveStartDate == null
                                ? 'Start Date'
                                : '${_leaveStartDate!.day}/${_leaveStartDate!.month}/${_leaveStartDate!.year}',
                            style: const TextStyle(color: Color(0xFF334155)),
                          ),
                          style: OutlinedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 12),
                            side: const BorderSide(color: Color(0xFFCBD5E1)),
                            backgroundColor: Colors.white,
                          ),
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
                          icon: const Icon(Icons.calendar_today, size: 18, color: Colors.indigo),
                          label: Text(
                            _leaveEndDate == null
                                ? 'End Date'
                                : '${_leaveEndDate!.day}/${_leaveEndDate!.month}/${_leaveEndDate!.year}',
                            style: const TextStyle(color: Color(0xFF334155)),
                          ),
                          style: OutlinedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 12),
                            side: const BorderSide(color: Color(0xFFCBD5E1)),
                            backgroundColor: Colors.white,
                          ),
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
                    style: const TextStyle(color: Color(0xFF1E293B)),
                    decoration: InputDecoration(
                      labelText: 'Reason for Leave',
                      labelStyle: const TextStyle(color: Color(0xFF64748B)),
                      hintText: 'e.g. Medical Conference, Personal Leave',
                      hintStyle: const TextStyle(color: Color(0xFF94A3B8)),
                      filled: true,
                      fillColor: const Color(0xFFF8FAFC),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                        borderSide: const BorderSide(color: Color(0xFFCBD5E1)),
                      ),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                        borderSide: const BorderSide(color: Color(0xFFCBD5E1)),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                        borderSide: const BorderSide(color: Colors.indigo, width: 2),
                      ),
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
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                      ),
                      onPressed: _isSubmittingLeave ? null : _submitLeaveRequest,
                      child: _isSubmittingLeave
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                            )
                          : const Text('Submit Leave Application',
                              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),

            const Text(
              'My Leave Applications History',
              style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.indigo),
            ),
            const SizedBox(height: 10),

            if (_leaveError != null)
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: const Color(0xFFFEF2F2),
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: const Color(0xFFFCA5A5)),
                ),
                child: Text(_leaveError!, style: const TextStyle(color: Colors.red, fontSize: 13)),
              )
            else if (_isLoadingLeaves)
              const Padding(
                padding: EdgeInsets.symmetric(vertical: 20),
                child: Center(child: CircularProgressIndicator(color: Colors.indigo)),
              )
            else if (_leaves.isEmpty)
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(20.0),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: const Color(0xFFE2E8F0)),
                ),
                child: const Center(
                  child: Text(
                    'No previous leave applications recorded.',
                    style: TextStyle(color: Color(0xFF475569), fontSize: 14, fontWeight: FontWeight.w500),
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
                  return Container(
                    margin: const EdgeInsets.only(bottom: 10),
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: const Color(0xFFE2E8F0)),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                '${item.startDate.day}/${item.startDate.month}/${item.startDate.year} - ${item.endDate.day}/${item.endDate.month}/${item.endDate.year}',
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 14,
                                  color: Color(0xFF0F172A),
                                ),
                              ),
                              const SizedBox(height: 2),
                              Text(item.reason,
                                  style: const TextStyle(fontSize: 13, color: Color(0xFF475569))),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: item.status == LeaveStatus.approved
                                ? const Color(0xFFDCFCE7)
                                : item.status == LeaveStatus.rejected
                                    ? const Color(0xFFFEE2E2)
                                    : const Color(0xFFFEF3C7),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            item.status.name.toUpperCase(),
                            style: TextStyle(
                              color: item.status == LeaveStatus.approved
                                  ? const Color(0xFF15803D)
                                  : item.status == LeaveStatus.rejected
                                      ? const Color(0xFFB91C1C)
                                      : const Color(0xFFB45309),
                              fontSize: 11,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ],
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
