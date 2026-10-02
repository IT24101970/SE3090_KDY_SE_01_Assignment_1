enum WorkflowStepStatus { pending, inProgress, completed, failed }

class AgentWorkflowStepProgress {
  final int stepNumber;
  final String title;
  final String agentName;
  WorkflowStepStatus status;
  String? detail;

  AgentWorkflowStepProgress({
    required this.stepNumber,
    required this.title,
    required this.agentName,
    this.status = WorkflowStepStatus.pending,
    this.detail,
  });
}

class SymptomItem {
  final String symptomKeyword;
  final int severityRating;
  final int durationInDays;

  SymptomItem({
    required this.symptomKeyword,
    required this.severityRating,
    required this.durationInDays,
  });

  Map<String, dynamic> toJson() => {
    'symptomKeyword': symptomKeyword,
    'severityRating': severityRating,
    'durationInDays': durationInDays,
  };

  factory SymptomItem.fromJson(Map<String, dynamic> json) => SymptomItem(
    symptomKeyword: json['symptomKeyword'] ?? '',
    severityRating: json['severityRating'] ?? 1,
    durationInDays: json['durationInDays'] ?? 1,
  );
}

class TriageAssessment {
  final int id;
  final int appointmentId;
  final String rawSymptoms;
  final int urgencyScore;
  final String urgencyLevel;
  final String reasoningTrace;
  final String recommendedSpecialty;
  final List<SymptomItem> symptomLogs;
  final String createdAt;

  TriageAssessment({
    required this.id,
    required this.appointmentId,
    required this.rawSymptoms,
    required this.urgencyScore,
    required this.urgencyLevel,
    required this.reasoningTrace,
    required this.recommendedSpecialty,
    required this.symptomLogs,
    required this.createdAt,
  });

  factory TriageAssessment.fromJson(Map<String, dynamic> json) => TriageAssessment(
    id: json['id'] ?? 0,
    appointmentId: json['appointmentId'] ?? 0,
    rawSymptoms: json['rawSymptoms'] ?? '',
    urgencyScore: json['urgencyScore'] ?? 0,
    urgencyLevel: json['urgencyLevel']?.toString() ?? 'Low',
    reasoningTrace: json['reasoningTrace'] ?? '',
    recommendedSpecialty: json['recommendedSpecialty']?.toString() ?? 'General Medicine',
    symptomLogs: (json['symptomLogs'] as List<dynamic>?)
            ?.map((e) => SymptomItem.fromJson(e))
            .toList() ?? [],
    createdAt: json['createdAt'] ?? '',
  );
}

class PatientAppointment {
  final int id;
  final int patientId;
  final String patientName;
  final String doctorName;
  final String doctorSpecialty;
  final String roomName;
  final String appointmentDate;
  final String status;
  final String reasonForVisit;
  final String normalizedRawSymptoms;
  final int? triageAssessmentId;

  PatientAppointment({
    required this.id,
    required this.patientId,
    required this.patientName,
    required this.doctorName,
    required this.doctorSpecialty,
    required this.roomName,
    required this.appointmentDate,
    required this.status,
    required this.reasonForVisit,
    required this.normalizedRawSymptoms,
    this.triageAssessmentId,
  });

  factory PatientAppointment.fromJson(Map<String, dynamic> json) {
    String parseStatus(dynamic statusVal) {
      if (statusVal == null) return 'Pending';
      if (statusVal is int) {
        switch (statusVal) {
          case 0: return 'Pending';
          case 1: return 'Confirmed';
          case 2: return 'Completed';
          case 3: return 'Cancelled';
          default: return 'Pending';
        }
      }
      final s = statusVal.toString();
      if (s == '0') return 'Pending';
      if (s == '1') return 'Confirmed';
      if (s == '2') return 'Completed';
      if (s == '3') return 'Cancelled';
      return s;
    }

    return PatientAppointment(
      id: json['id'] ?? 0,
      patientId: json['patientId'] ?? 0,
      patientName: json['patientName'] ?? '',
      doctorName: (json['doctorName'] != null && json['doctorName'].toString().isNotEmpty && json['doctorName'].toString() != 'Unknown')
          ? json['doctorName'].toString()
          : 'Pending Doctor Assignment',
      doctorSpecialty: (json['doctorSpecialty'] != null && json['doctorSpecialty'].toString().isNotEmpty)
          ? json['doctorSpecialty'].toString()
          : (json['specialty'] ?? 'General Medicine'),
      roomName: json['roomName'] ?? 'Room TBD',
      appointmentDate: json['appointmentDate']?.toString() ?? '',
      status: parseStatus(json['status']),
      reasonForVisit: json['reasonForVisit'] ?? '',
      normalizedRawSymptoms: json['normalizedRawSymptoms'] ?? json['reasonForVisit'] ?? '',
      triageAssessmentId: json['triageAssessmentId'],
    );
  }
}


