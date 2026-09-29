// Dart models for Student 2: Doctor Scheduling & Consultation Management

enum AttendanceStatus { pending, present, noShow }
enum LeaveStatus { pending, approved, rejected }

AttendanceStatus parseAttendanceStatus(dynamic value) {
  if (value is int) {
    if (value >= 0 && value < AttendanceStatus.values.length) {
      return AttendanceStatus.values[value];
    }
  } else if (value is String) {
    final lower = value.toLowerCase();
    if (lower == 'present' || lower == '1') return AttendanceStatus.present;
    if (lower == 'noshow' || lower == 'no-show' || lower == '2') return AttendanceStatus.noShow;
  }
  return AttendanceStatus.pending;
}

LeaveStatus parseLeaveStatus(dynamic value) {
  if (value is int) {
    if (value >= 0 && value < LeaveStatus.values.length) {
      return LeaveStatus.values[value];
    }
  } else if (value is String) {
    final lower = value.toLowerCase();
    if (lower == 'approved' || lower == '1') return LeaveStatus.approved;
    if (lower == 'rejected' || lower == '2') return LeaveStatus.rejected;
  }
  return LeaveStatus.pending;
}

class DoctorProfile {
  final int id;
  final String name;
  final String specialty;
  final String email;

  DoctorProfile({
    required this.id,
    required this.name,
    required this.specialty,
    required this.email,
  });

  factory DoctorProfile.fromJson(Map<String, dynamic> json) {
    final doctorName = json['doctorName'] ?? json['name'] ?? 'Doctor';
    final emailSlug = doctorName.toString().toLowerCase().replaceAll(' ', '.').replaceAll('dr.', '').replaceAll('..', '.');
    return DoctorProfile(
      id: json['id'] ?? 0,
      name: doctorName,
      specialty: json['specialtyName'] ?? json['specialty'] ?? 'General',
      email: json['email'] ?? '${emailSlug.trim()}@channelcenter.com',
    );
  }
}

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

  bool get isExpired => DateTime.now().isAfter(endTime);

  String get statusText {
    if (isExpired) return 'EXPIRED';
    if (isSessionActive) return 'IN PROGRESS';
    return 'SCHEDULED';
  }

  factory DoctorScheduleModel.fromJson(Map<String, dynamic> json) {
    return DoctorScheduleModel(
      id: json['id'] ?? 0,
      doctorId: json['doctorId'] ?? 0,
      doctorName: json['doctorName'] ?? 'Doctor',
      specialtyName: json['specialtyName'] ?? 'General',
      roomId: json['roomId'] ?? 0,
      roomName: json['roomName'] ?? 'Room',
      floor: json['floor'] ?? json['roomFloor'] ?? '',
      startTime: json['startTime'] != null ? DateTime.parse(json['startTime']).toLocal() : DateTime.now(),
      endTime: json['endTime'] != null ? DateTime.parse(json['endTime']).toLocal() : DateTime.now().add(const Duration(hours: 2)),
      maxPatients: json['maxPatients'] ?? 10,
      isSessionActive: json['isSessionActive'] ?? false,
    );
  }
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

  factory PatientQueueItem.fromJson(Map<String, dynamic> json, int queueIndex) {
    return PatientQueueItem(
      appointmentId: json['id'] ?? json['appointmentId'] ?? 0,
      patientName: json['patientName'] ?? 'Patient',
      queueNumber: queueIndex,
      attendanceStatus: parseAttendanceStatus(json['attendanceStatus'] ?? json['status']),
      clinicalNotes: json['clinicalNotes'] ?? '',
      prescriptionData: json['prescriptionData'] ?? '{}',
    );
  }
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

  factory DoctorLeaveModel.fromJson(Map<String, dynamic> json) {
    return DoctorLeaveModel(
      id: json['id'] ?? 0,
      doctorId: json['doctorId'] ?? 0,
      startDate: json['startDate'] != null ? DateTime.parse(json['startDate']).toLocal() : DateTime.now(),
      endDate: json['endDate'] != null ? DateTime.parse(json['endDate']).toLocal() : DateTime.now(),
      reason: json['reason'] ?? '',
      status: parseLeaveStatus(json['status']),
    );
  }
}
