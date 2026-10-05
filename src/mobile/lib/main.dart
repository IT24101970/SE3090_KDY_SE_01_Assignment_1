import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'providers/notification_provider.dart';
import 'providers/triage_provider.dart';
import 'providers/workflow_provider.dart';
import 'screens/admin/admin_system_overview_screen.dart';
import 'screens/auth/login_screen.dart';
import 'screens/doctor/doctor_dashboard_screen.dart';
import 'screens/patient/patient_dashboard_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const ChannelCenterMobileApp());
}

class ChannelCenterMobileApp extends StatelessWidget {
  const ChannelCenterMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider<AuthProvider>(
          create: (_) => AuthProvider(),
        ),
        ChangeNotifierProvider<NotificationProvider>(
          create: (_) => NotificationProvider(),
        ),
        ChangeNotifierProvider<WorkflowProvider>(
          create: (_) => WorkflowProvider(),
        ),
        ChangeNotifierProvider<TriageProvider>(
          create: (_) => TriageProvider(),
        ),
      ],
      child: MaterialApp(
        title: 'ChannelCenter Healthcare System',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          useMaterial3: true,
          colorScheme: ColorScheme.fromSeed(
            seedColor: const Color(0xFF2563EB),
            brightness: Brightness.light,
          ),
          scaffoldBackgroundColor: const Color(0xFFF8FAFC),
          appBarTheme: const AppBarTheme(
            centerTitle: false,
            elevation: 0,
            backgroundColor: Colors.white,
            foregroundColor: Color(0xFF0F172A),
            iconTheme: IconThemeData(color: Color(0xFF2563EB)),
          ),
          cardTheme: CardThemeData(
            elevation: 1,
            color: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
              side: const BorderSide(color: Color(0xFFE2E8F0)),
            ),
          ),
        ),
        home: Consumer<AuthProvider>(
          builder: (context, auth, _) {
            if (auth.isLoading) {
              return const Scaffold(
                backgroundColor: Color(0xFFF8FAFC),
                body: Center(
                  child: CircularProgressIndicator(color: Color(0xFF2563EB)),
                ),
              );
            }
            if (auth.isAuthenticated) {
              // Unified role-based redirection
              if (auth.isAdmin) {
                return const AdminSystemOverviewScreen();
              } else if (auth.isDoctor) {
                return const DoctorDashboardScreen();
              } else {
                return const PatientDashboardScreen();
              }
            }
            return const LoginScreen();
          },
        ),
      ),
    );
  }
}

