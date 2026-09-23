import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/triage_provider.dart';
import '../widgets/urgency_badge.dart';

class TriageHistoryScreen extends StatelessWidget {
  const TriageHistoryScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      appBar: AppBar(
        title: const Text('Triage History'),
        backgroundColor: const Color(0xFF1E293B),
      ),
      body: Consumer<TriageProvider>(
        builder: (context, provider, child) {
          final history = provider.history;

          if (history.isEmpty) {
            return const Center(
              child: Text('No historical triage records found', style: TextStyle(color: Colors.white60)),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: history.length,
            itemBuilder: (context, index) {
              final item = history[index];
              return Card(
                color: const Color(0xFF1E293B),
                margin: const EdgeInsets.only(bottom: 12),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('Appt #${item.appointmentId}', style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold)),
                          UrgencyBadge(urgencyLevel: item.urgencyLevel, urgencyScore: item.urgencyScore),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(item.rawSymptoms, style: const TextStyle(color: Colors.white, fontSize: 14)),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          const Icon(Icons.medical_services_outlined, size: 14, color: Colors.cyanAccent),
                          const SizedBox(width: 4),
                          Text(item.recommendedSpecialty, style: const TextStyle(color: Colors.cyanAccent, fontSize: 13, fontWeight: FontWeight.w600)),
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
