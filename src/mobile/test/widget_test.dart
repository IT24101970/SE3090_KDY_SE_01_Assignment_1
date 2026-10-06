import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:mobile_app/models/auth_model.dart';
import 'package:mobile_app/models/notification_model.dart';
import 'package:mobile_app/providers/auth_provider.dart';
import 'package:mobile_app/providers/notification_provider.dart';
import 'package:mobile_app/screens/auth/login_screen.dart';

class TestAuthProvider extends ChangeNotifier implements AuthProvider {
  @override
  bool get isLoading => false;

  @override
  String? get errorMessage => null;

  @override
  String? get token => null;

  @override
  AuthUser? get currentUser => null;

  @override
  bool get isAuthenticated => false;

  @override
  bool get isAdmin => false;

  @override
  bool get isDoctor => false;

  @override
  bool get isPatient => true;

  @override
  bool get isStaff => false;

  @override
  Future<bool> login({
    required String email,
    required String password,
    bool isAdmin = false,
  }) async {
    return false;
  }

  @override
  Future<void> logout() async {}

  @override
  dynamic noSuchMethod(Invocation invocation) => super.noSuchMethod(invocation);
}

class TestNotificationProvider extends ChangeNotifier implements NotificationProvider {
  @override
  int get unreadCount => 0;

  @override
  List<AppNotification> get notifications => [];

  @override
  dynamic noSuchMethod(Invocation invocation) => super.noSuchMethod(invocation);
}

Widget createLoginScreenWidget() {
  return MultiProvider(
    providers: [
      ChangeNotifierProvider<AuthProvider>(create: (_) => TestAuthProvider()),
      ChangeNotifierProvider<NotificationProvider>(create: (_) => TestNotificationProvider()),
    ],
    child: const MaterialApp(
      home: LoginScreen(),
    ),
  );
}

void main() {
  testWidgets('ChannelCenter LoginScreen renders header branding and input fields', (WidgetTester tester) async {
    await tester.pumpWidget(createLoginScreenWidget());
    await tester.pump();

    // Verify key UI elements on the login screen
    expect(find.byType(LoginScreen), findsOneWidget);
    expect(find.text('ChannelCenter Hospital'), findsOneWidget);
    expect(find.text('Unified Authentication Portal'), findsOneWidget);
    expect(find.byType(TextFormField), findsNWidgets(2)); // Email and Password inputs
    expect(find.widgetWithText(ElevatedButton, 'Sign In'), findsOneWidget);
  });

  testWidgets('LoginScreen shows validation errors for empty email and password', (WidgetTester tester) async {
    await tester.pumpWidget(createLoginScreenWidget());
    await tester.pump();

    // Tap the Sign In button without entering credentials
    final signInBtn = find.widgetWithText(ElevatedButton, 'Sign In');
    await tester.tap(signInBtn);
    await tester.pump();

    // Check for validation error hints
    expect(find.text('Please enter your email'), findsOneWidget);
    expect(find.text('Please enter your password'), findsOneWidget);
  });
}
