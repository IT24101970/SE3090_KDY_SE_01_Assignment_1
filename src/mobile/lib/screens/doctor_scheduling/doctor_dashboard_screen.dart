import 'package:flutter/material.dart';
import '../../models/doctor_scheduling_models.dart';
import '../../services/doctor_scheduling_service.dart';
import 'consultation_attendance_screen.dart';
import 'doctor_leave_screen.dart';
import 'doctor_login_screen.dart';

class DoctorDashboardScreen extends StatefulWidget {
  final DoctorProfile? doctorProfile;

  const DoctorDashboardScreen({Key? key, this.doctorProfile}) : super(key: key);

  @override
  State<DoctorDashboardScreen> createState() => _DoctorDashboardScreenState();
}

class _DoctorDashboardScreenState extends State<DoctorDashboardScreen> {
  late DoctorProfile _profile;

  List<DoctorScheduleModel> _schedules = [];
  List<PatientQueueItem> _patients = [];

  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _profile = widget.doctorProfile ??
        DoctorProfile(
          id: 1,
          name: 'Dr. Sarah Jenkins',
          specialty: 'Cardiology',
          email: 'sarah.jenkins@channelcenter.hospital',
        );

    _loadDashboardDataFromDb();
  }

  Future<void> _loadDashboardDataFromDb() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final schedules = await DoctorSchedulingService.getDoctorSchedules(_profile.id);
      final patients = await DoctorSchedulingService.getPatientQueue(_profile.id);

      if (!mounted) return;
      setState(() {
        _schedules = schedules;
        _patients = patients;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _errorMessage = 'Error loading database records: $e';
      });
    }
  }

  void _toggleSession(DoctorScheduleModel schedule) {
    if (schedule.isExpired) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cannot start session: This channel schedule has EXPIRED.'),
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
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const DoctorLoginScreen()),
              );
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
    final activeSchedules = _schedules.where((s) => !s.isExpired).toList();
    final expiredSchedules = _schedules.where((s) => s.isExpired).toList();

    return Scaffold(
      backgroundColor: Colors.grey.shade50,
      appBar: AppBar(
        title: const Text('Doctor Channel Dashboard'),
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
        actions: [
          TextButton(
            onPressed: _loadDashboardDataFromDb,
            child: const Text('Refresh', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ),
          TextButton(
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => DoctorLeaveScreen(doctorId: _profile.id),
                ),
              );
            },
            child: const Text('Apply Leave', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ),
          TextButton(
            onPressed: _confirmLogout,
            child: const Text('Logout', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadDashboardDataFromDb,
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Doctor Profile Card
              Card(
                color: Colors.indigo.shade50,
                elevation: 0,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        _profile.name,
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          color: Colors.indigo,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Specialty: ${_profile.specialty}',
                        style: TextStyle(
                          color: Colors.grey.shade800,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      Text(
                        _profile.email,
                        style: TextStyle(fontSize: 13, color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 20),

              if (_errorMessage != null) ...[
                Card(
                  color: Colors.red.shade50,
                  child: Padding(
                    padding: const EdgeInsets.all(12.0),
                    child: Text(_errorMessage!, style: const TextStyle(color: Colors.red)),
                  ),
                ),
                const SizedBox(height: 16),
              ],

              if (_isLoading)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 40),
                  child: Center(
                    child: CircularProgressIndicator(),
                  ),
                )
              else ...[
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Daily Channel Schedules',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.indigo),
                    ),
                    Chip(
                      label: Text('${_schedules.length} Total'),
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
                          'No active channel sessions found in database for today.',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ),
                    ),
                  )
                else
                  ...activeSchedules.map((schedule) => _buildScheduleCard(schedule, isExpired: false)),

                if (expiredSchedules.isNotEmpty) ...[
                  const SizedBox(height: 20),
                  Text(
                    'Expired / Past Channel Sessions (${expiredSchedules.length})',
                    style: TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                      color: Colors.grey.shade700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  ...expiredSchedules.map((schedule) => _buildScheduleCard(schedule, isExpired: true)),
                ],

                const SizedBox(height: 24),

                Builder(
                  builder: (context) {
                    final doctorPatients = _patients.where((p) => p.doctorId == 0 || p.doctorId == _profile.id).toList();
                    return Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text(
                              'Patient Attendance Queue',
                              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.indigo),
                            ),
                            Text(
                              '${doctorPatients.where((p) => p.attendanceStatus == AttendanceStatus.present).length} Present / ${doctorPatients.length} Total',
                              style: const TextStyle(color: Colors.green, fontWeight: FontWeight.bold),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),

                        if (doctorPatients.isEmpty)
                          Card(
                            child: Padding(
                              padding: const EdgeInsets.all(16.0),
                              child: Center(
                                child: Text(
                                  'No patient appointments in database for ${_profile.name}.',
                                  style: TextStyle(color: Colors.grey.shade600),
                                ),
                              ),
                            ),
                          )
                        else
                          ListView.builder(
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            itemCount: doctorPatients.length,
                            itemBuilder: (context, index) {
                              final patient = doctorPatients[index];
                              return Card(
                                margin: const EdgeInsets.only(bottom: 10),
                                child: ListTile(
                                  title: Text(
                                    'Queue #${patient.queueNumber}: ${patient.patientName}',
                                    style: const TextStyle(fontWeight: FontWeight.bold),
                                  ),
                                  subtitle: Text('Appointment ID: ${patient.appointmentId}'),
                                  trailing: ElevatedButton(
                                    onPressed: () async {
                                      await Navigator.push(
                                        context,
                                        MaterialPageRoute(
                                          builder: (_) => ConsultationAttendanceScreen(patient: patient),
                                        ),
                                      );
                                      _loadDashboardDataFromDb();
                                    },
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: patient.attendanceStatus == AttendanceStatus.present
                                          ? Colors.green
                                          : (patient.attendanceStatus == AttendanceStatus.noShow
                                              ? Colors.red
                                              : Colors.indigo),
                                      foregroundColor: Colors.white,
                                    ),
                                    child: Text(patient.attendanceStatus.name.toUpperCase()),
                                  ),
                                ),
                              );
                            },
                          ),
                      ],
                    );
                  },
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildScheduleCard(DoctorScheduleModel schedule, {required bool isExpired}) {
    return Card(
      elevation: isExpired ? 1 : 2,
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: isExpired ? Colors.grey.shade300 : Colors.indigo.shade100,
          width: 1,
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  schedule.roomName,
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: isExpired ? Colors.grey.shade700 : Colors.indigo,
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: isExpired
                        ? Colors.grey.shade600
                        : (schedule.isSessionActive ? Colors.green : Colors.orange),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    schedule.statusText,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 11,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
            const Divider(height: 16),
            if (schedule.floor.isNotEmpty)
              Text('Location: ${schedule.floor}', style: const TextStyle(fontSize: 14)),
            const SizedBox(height: 4),
            Text(
              'Time: ${_formatTime(schedule.startTime)} - ${_formatTime(schedule.endTime)}',
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600),
            ),
            const SizedBox(height: 4),
            Text('Capacity: ${_patients.length} / ${schedule.maxPatients} Patients Booked', style: const TextStyle(fontSize: 14)),
            if (!isExpired) ...[
              const SizedBox(height: 14),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () => _toggleSession(schedule),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: schedule.isSessionActive ? Colors.red : Colors.green,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                  child: Text(
                    schedule.isSessionActive ? 'End Channel Session' : 'Start Channel Session',
                    style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
            ],
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
