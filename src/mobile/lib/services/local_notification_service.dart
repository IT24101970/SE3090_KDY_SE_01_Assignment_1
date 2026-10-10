import 'package:flutter_local_notifications/flutter_local_notifications.dart';

class LocalNotificationService {
  static final FlutterLocalNotificationsPlugin _notificationsPlugin =
      FlutterLocalNotificationsPlugin();

  static bool _isInitialized = false;

  static const AndroidNotificationChannel _channel = AndroidNotificationChannel(
    'channel_center_alerts_v2', // channel id
    'ChannelCenter Notifications', // channel name
    description: 'High-priority emergency & appointment notifications for ChannelCenter Healthcare System',
    importance: Importance.max,
    playSound: true,
    enableVibration: true,
  );

  /// Initializes system notification channels for Android
  static Future<void> initialize() async {
    if (_isInitialized) return;

    const AndroidInitializationSettings initializationSettingsAndroid =
        AndroidInitializationSettings('@mipmap/ic_launcher');

    const InitializationSettings initializationSettings = InitializationSettings(
      android: initializationSettingsAndroid,
    );

    try {
      await _notificationsPlugin.initialize(
        initializationSettings,
      );

      final androidPlugin = _notificationsPlugin
          .resolvePlatformSpecificImplementation<AndroidFlutterLocalNotificationsPlugin>();
      if (androidPlugin != null) {
        // Explicitly register high-importance notification channel with Android system OS
        await androidPlugin.createNotificationChannel(_channel);
        // Request notification permissions for Android 13+ (API 33+)
        await androidPlugin.requestNotificationsPermission();
      }

      _isInitialized = true;
    } catch (_) {
      // Graceful fallback
    }
  }

  /// Trigger native OS status bar notification popup
  static Future<void> showNotification({
    required int id,
    required String title,
    required String body,
    String? channelId,
  }) async {
    if (!_isInitialized) {
      await initialize();
    }

    final AndroidNotificationDetails androidNotificationDetails =
        AndroidNotificationDetails(
      _channel.id,
      _channel.name,
      channelDescription: _channel.description,
      importance: Importance.max,
      priority: Priority.high,
      playSound: true,
      enableVibration: true,
      showWhen: true,
      icon: '@mipmap/ic_launcher',
    );

    final NotificationDetails notificationDetails = NotificationDetails(
      android: androidNotificationDetails,
    );

    try {
      await _notificationsPlugin.show(
        id,
        title,
        body,
        notificationDetails,
      );
    } catch (_) {
      // Fallback
    }
  }
}
