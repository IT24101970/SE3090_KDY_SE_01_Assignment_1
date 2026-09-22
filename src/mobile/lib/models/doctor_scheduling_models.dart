// Dart models for Student 2: Doctor Scheduling & Consultation Management

enum AttendanceStatus { pending, present, noShow }
enum LeaveStatus { pending, approved, rejected }

class DoctorScheduleModel {
  final int id;
  final int doctorId;
  final String doctorName;
  final String specialtyName;
  final int roomId;
  final String roomName;
  final String floor;
  final DateTime startTime;
  final DateTime endTime;
  final int maxPatients;
  bool isSessionActive;

  DoctorScheduleModel({
    required this.id,
    required this.doctorId,
    required this.doctorName,
    required this.specialtyName,
    required this.roomId,
    required this.roomName,
    required this.floor,
    required this.startTime,
    required this.endTime,
    required this.maxPatients,
    this.isSessionActive = false,
  });
}

class PatientQueueItem {
  final int appointmentId;
  final String patientName;
  final int queueNumber;
  AttendanceStatus attendanceStatus;
  String clinicalNotes;
  String prescriptionData;

  PatientQueueItem({
    required this.appointmentId,
    required this.patientName,
    required this.queueNumber,
    this.attendanceStatus = AttendanceStatus.pending,
    this.clinicalNotes = '',
    this.prescriptionData = '{}',
  });
}

class DoctorLeaveModel {
  final int id;
  final int doctorId;
  final DateTime startDate;
  final DateTime endDate;
  final String reason;
  final LeaveStatus status;

  DoctorLeaveModel({
    required this.id,
    required this.doctorId,
    required this.startDate,
    required this.endDate,
    required this.reason,
    required this.status,
  });
}
