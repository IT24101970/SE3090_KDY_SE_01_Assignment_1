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
