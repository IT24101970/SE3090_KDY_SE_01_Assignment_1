import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../models/notification_model.dart';
import '../providers/auth_provider.dart';
import '../providers/notification_provider.dart';

class NotificationBellWidget extends StatelessWidget {
  const NotificationBellWidget({Key? key}) : super(key: key);

  void _showNotificationSheet(BuildContext context, String userRole) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) {
        return Consumer<NotificationProvider>(
          builder: (context, notifProvider, _) {
            final roleNotifs = notifProvider.getForRole(userRole);

            return Container(
              height: MediaQuery.of(context).size.height * 0.7,
              decoration: const BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
              ),
              child: Column(
                children: [
                  // Modal Header
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                    decoration: BoxDecoration(
                      border: Border(bottom: BorderSide(color: Colors.grey.shade200)),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Row(
                          children: [
                            const Icon(Icons.notifications_active_rounded, color: Colors.indigo),
                            const SizedBox(width: 8),
                            Text(
                              'Notifications',
                              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                            ),
                            const SizedBox(width: 8),
                            if (roleNotifs.isNotEmpty)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                                decoration: BoxDecoration(
                                  color: Colors.indigo.shade50,
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Text(
                                  '${roleNotifs.length}',
                                  style: TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.indigo.shade700,
                                  ),
                                ),
                              ),
                          ],
                        ),
                        Row(
                          children: [
                            if (roleNotifs.isNotEmpty)
                              TextButton(
                                onPressed: () {
                                  notifProvider.markAllAsRead();
                                },
                                child: const Text('Mark read', style: TextStyle(fontSize: 12)),
                              ),
                            IconButton(
                              icon: const Icon(Icons.close),
                              onPressed: () => Navigator.pop(context),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),

                  // Notification List
                  Expanded(
                    child: roleNotifs.isEmpty
                        ? Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.notifications_none_rounded, size: 56, color: Colors.grey.shade400),
                                const SizedBox(height: 12),
                                Text(
                                  'No new notifications',
                                  style: TextStyle(color: Colors.grey.shade600, fontWeight: FontWeight.bold),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  'You will receive alerts here when updates occur.',
                                  style: TextStyle(color: Colors.grey.shade500, fontSize: 12),
                                ),
                              ],
                            ),
                          )
                        : ListView.separated(
                            padding: const EdgeInsets.all(16),
                            itemCount: roleNotifs.length,
                            separatorBuilder: (_, __) => const SizedBox(height: 10),
                            itemBuilder: (context, index) {
                              final notif = roleNotifs[index];
                              return _NotificationTile(
                                notification: notif,
                                onTap: () {
                                  notifProvider.markAsRead(notif.id);
                                },
                              );
                            },
                          ),
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final notifProvider = Provider.of<NotificationProvider>(context);

    final userRole = authProvider.currentUser?.role ?? 'Patient';
    final roleNotifs = notifProvider.getForRole(userRole);
    final unreadCount = roleNotifs.where((n) => !n.isRead).length;

    return Stack(
      alignment: Alignment.center,
      children: [
        IconButton(
          icon: const Icon(Icons.notifications_outlined),
          tooltip: 'Notifications',
          onPressed: () => _showNotificationSheet(context, userRole),
        ),
        if (unreadCount > 0)
          Positioned(
            right: 8,
            top: 8,
            child: Container(
              padding: const EdgeInsets.all(4),
              decoration: const BoxDecoration(
                color: Colors.red,
                shape: BoxShape.circle,
              ),
              constraints: const BoxConstraints(
                minWidth: 16,
                minHeight: 16,
              ),
              child: Text(
                '$unreadCount',
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),
            ),
          ),
      ],
    );
  }
}

class _NotificationTile extends StatelessWidget {
  final AppNotification notification;
  final VoidCallback onTap;

  const _NotificationTile({
    Key? key,
    required this.notification,
    required this.onTap,
  }) : super(key: key);

  IconData _getIcon() {
    switch (notification.type) {
      case 'safety_alert':
        return Icons.security_rounded;
      case 'appointment_finalized':
        return Icons.event_available_rounded;
      case 'upcoming_appointment':
        return Icons.medical_services_rounded;
      default:
        return Icons.info_outline_rounded;
    }
  }

  Color _getIconColor() {
    switch (notification.type) {
      case 'safety_alert':
        return Colors.red.shade700;
      case 'appointment_finalized':
        return Colors.green.shade700;
      case 'upcoming_appointment':
        return Colors.indigo.shade700;
      default:
        return Colors.blue.shade700;
    }
  }

  Color _getBgColor() {
    switch (notification.type) {
      case 'safety_alert':
        return Colors.red.shade50;
      case 'appointment_finalized':
        return Colors.green.shade50;
      case 'upcoming_appointment':
        return Colors.indigo.shade50;
      default:
        return Colors.blue.shade50;
    }
  }

  @override
  Widget build(BuildContext context) {
    final formattedTime = DateFormat('MMM d, h:mm a').format(notification.timestamp);

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: notification.isRead ? Colors.grey.shade50 : _getBgColor(),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: notification.isRead ? Colors.grey.shade200 : _getIconColor().withOpacity(0.3),
          ),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: Colors.white,
                shape: BoxShape.circle,
                border: Border.all(color: _getIconColor().withOpacity(0.2)),
              ),
              child: Icon(_getIcon(), size: 20, color: _getIconColor()),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          notification.title,
                          style: TextStyle(
                            fontWeight: notification.isRead ? FontWeight.normal : FontWeight.bold,
                            fontSize: 13,
                            color: Colors.black87,
                          ),
                        ),
                      ),
                      Text(
                        formattedTime,
                        style: TextStyle(fontSize: 10, color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    notification.message,
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey.shade700,
                      height: 1.3,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
