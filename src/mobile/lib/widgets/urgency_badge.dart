import 'package:flutter/material.dart';

class UrgencyBadge extends StatelessWidget {
  final String? level;
  final String? urgencyLevel;
  final int? urgencyScore;

  const UrgencyBadge({
    Key? key,
    this.level,
    this.urgencyLevel,
    this.urgencyScore,
  }) : super(key: key);

  String get _displayLevel => urgencyLevel ?? level ?? 'Low';

  Color _getBadgeColor() {
    switch (_displayLevel.toLowerCase()) {
      case 'emergency':
        return Colors.red.shade700;
      case 'high':
        return Colors.orange.shade800;
      case 'medium':
        return Colors.amber.shade800;
      case 'low':
        return Colors.green.shade700;
      default:
        return Colors.blue;
    }
  }

  @override
  Widget build(BuildContext context) {
    final color = _getBadgeColor();
    final text = urgencyScore != null
        ? '${_displayLevel.toUpperCase()} ($urgencyScore/100)'
        : _displayLevel.toUpperCase();

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color, width: 1),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.bold,
          fontSize: 11,
        ),
      ),
    );
  }
}
