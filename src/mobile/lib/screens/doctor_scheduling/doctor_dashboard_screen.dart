import 'package:flutter/material.dart';
import '../../models/doctor_scheduling_models.dart';
import 'consultation_attendance_screen.dart';
import 'doctor_leave_screen.dart';

class DoctorDashboardScreen extends StatefulWidget {
  const DoctorDashboardScreen({Key? key}) : super(key: key);

  @override
  State<DoctorDashboardScreen> createState() => _DoctorDashboardScreenState();
}

class _DoctorDashboardScreenState extends State<DoctorDashboardScreen> {
  final List<DoctorScheduleModel> _schedules = [
    DoctorScheduleModel(
      id: 101,
      doctorId: 1,
      doctorName: 'Dr. Sarah Jenkins',
      specialtyName: 'Cardiology',
      roomId: 1,
      roomName: 'Room 101',
      floor: '1st Floor',
      startTime: DateTime.now().subtract(const Duration(minutes: 30)),
      endTime: DateTime.now().add(const Duration(hours: 3)),
      maxPatients: 15,
      isSessionActive: false,
    ),
  ];

  final List<PatientQueueItem> _patients = [
    PatientQueueItem(appointmentId: 1001, patientName: 'John Doe', queueNumber: 1),
    PatientQueueItem(appointmentId: 1002, patientName: 'Jane Smith', queueNumber: 2),
    PatientQueueItem(appointmentId: 1003, patientName: 'Robert Johnson', queueNumber: 3),
    PatientQueueItem(appointmentId: 1004, patientName: 'Emily Davis', queueNumber: 4),
  ];

  void _toggleSession(DoctorScheduleModel schedule) {
    setState(() {
      schedule.isSessionActive = !schedule.isSessionActive;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          schedule.isSessionActive
              ? 'Channel Session Started at ${schedule.roomName}'
              : 'Channel Session Ended',
        ),
        backgroundColor: schedule.isSessionActive ? Colors.green : Colors.orange,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final schedule = _schedules.first;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Doctor Channel Dashboard'),
        backgroundColor: Colors.indigo,
        actions: [
          IconButton(
            icon: const Icon(Icons.beach_access),
            tooltip: 'Apply for Leave',
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const DoctorLeaveScreen()),
              );
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Welcome Header
            Card(
              color: Colors.indigo.shade50,
              elevation: 0,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  children: [
                    const CircleAvatar(
                      radius: 28,
                      backgroundColor: Colors.indigo,
                      child: Icon(Icons.person, color: Colors.white, size: 32),
                    ),
                    const SizedBox(width: 16),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          schedule.doctorName,
                          style: const TextStyle(
                              fontSize: 18, fontWeight: FontWeight.bold),
                        ),
                        Text(
                          'Specialty: ${schedule.specialtyName}',
                          style: TextStyle(color: Colors.grey.shade700),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),

            // Today's Channel Session Card
            Card(
              elevation: 3,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'Today\'s Channel Session',
                          style: TextStyle(
                              fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                        Chip(
                          label: Text(
                            schedule.isSessionActive ? 'IN PROGRESS' : 'NOT STARTED',
                            style: const TextStyle(
                                color: Colors.white, fontWeight: FontWeight.bold),
                          ),
                          backgroundColor:
                              schedule.isSessionActive ? Colors.green : Colors.grey,
                        ),
                      ],
                    ),
                    const Divider(height: 20),
                    Row(
                      children: [
                        const Icon(Icons.meeting_room, color: Colors.indigo),
                        const SizedBox(width: 8),
                        Text('${schedule.roomName} (${schedule.floor})'),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        const Icon(Icons.people, color: Colors.indigo),
                        const SizedBox(width: 8),
                        Text('${_patients.length} / ${schedule.maxPatients} Patients Booked'),
                      ],
                    ),
                    const SizedBox(height: 16),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: () => _toggleSession(schedule),
                        icon: Icon(schedule.isSessionActive
                            ? Icons.stop_circle
                            : Icons.play_circle_fill),
                        label: Text(schedule.isSessionActive
                            ? 'End Session'
                            : 'Start Channel Session'),
                        style: ElevatedButton.styleFrom(
                          backgroundColor:
                              schedule.isSessionActive ? Colors.red : Colors.green,
                          padding: const EdgeInsets.symmetric(vertical: 12),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Patient Queue List
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Patient Attendance Queue',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                Text(
                  '${_patients.where((p) => p.attendanceStatus == AttendanceStatus.present).length} Present',
                  style: const TextStyle(color: Colors.green, fontWeight: FontWeight.bold),
                ),
              ],
            ),
            const SizedBox(height: 12),

            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _patients.length,
              itemBuilder: (context, index) {
                final patient = _patients[index];
                return Card(
                  margin: const EdgeInsets.only(bottom: 10),
                  child: ListTile(
                    leading: CircleAvatar(
                      backgroundColor: Colors.indigo.shade100,
                      child: Text('#${patient.queueNumber}',
                          style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.indigo)),
                    ),
                    title: Text(patient.patientName,
                        style: const TextStyle(fontWeight: FontWeight.bold)),
                    subtitle: Text('Appt #${patient.appointmentId}'),
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Chip(
                          label: Text(patient.attendanceStatus.name.toUpperCase()),
                          backgroundColor: patient.attendanceStatus == AttendanceStatus.present
                              ? Colors.green.shade100
                              : patient.attendanceStatus == AttendanceStatus.noShow
                                  ? Colors.red.shade100
                                  : Colors.grey.shade200,
                        ),
                        const Icon(Icons.chevron_right),
                      ],
                    ),
                    onTap: () async {
                      await Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => ConsultationAttendanceScreen(patient: patient),
                        ),
                      );
                      setState(() {});
                    },
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
