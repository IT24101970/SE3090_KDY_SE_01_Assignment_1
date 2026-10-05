import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/triage_provider.dart';
import '../widgets/urgency_badge.dart';

class TriageHistoryScreen extends StatelessWidget {
  const TriageHistoryScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8FAFC),
      appBar: AppBar(
        title: const Text('Triage History', style: TextStyle(color: Color(0xFF0F172A), fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        iconTheme: const IconThemeData(color: Color(0xFF2563EB)),
        elevation: 0,
      ),
      body: Consumer<TriageProvider>(
        builder: (context, provider, child) {
          final history = provider.history;

          if (history.isEmpty) {
            return const Center(
              child: Text('No historical triage records found', style: TextStyle(color: Color(0xFF64748B))),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: history.length,
            itemBuilder: (context, index) {
              final item = history[index];
              return Card(
                color: Colors.white,
                margin: const EdgeInsets.only(bottom: 12),
                elevation: 1,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: const BorderSide(color: Color(0xFFE2E8F0)),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('Appt #${item.appointmentId}', style: const TextStyle(color: Color(0xFF64748B), fontWeight: FontWeight.bold)),
                          UrgencyBadge(urgencyLevel: item.urgencyLevel, urgencyScore: item.urgencyScore),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(item.rawSymptoms, style: const TextStyle(color: Color(0xFF0F172A), fontSize: 14)),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          const Icon(Icons.medical_services_outlined, size: 14, color: Color(0xFF2563EB)),
                          const SizedBox(width: 4),
                          Text(item.recommendedSpecialty, style: const TextStyle(color: Color(0xFF2563EB), fontSize: 13, fontWeight: FontWeight.w600)),
                        ],
                      ),
                    ],
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
