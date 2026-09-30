import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'providers/workflow_provider.dart';
import 'providers/triage_provider.dart';
import 'screens/auth/login_screen.dart';
import 'screens/doctor_scheduling/doctor_dashboard_screen.dart';
import 'screens/home_navigation_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const DoctorSchedulingMobileApp());
}

class DoctorSchedulingMobileApp extends StatelessWidget {
  const DoctorSchedulingMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider<AuthProvider>(
          create: (_) => AuthProvider(),
        ),
        ChangeNotifierProvider<WorkflowProvider>(
          create: (_) => WorkflowProvider(),
        ),
        ChangeNotifierProvider<TriageProvider>(
          create: (_) => TriageProvider(),
        ),
      ],
      child: MaterialApp(
        title: 'ChannelCenter Hospital App',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          useMaterial3: true,
          colorScheme: ColorScheme.fromSeed(
            seedColor: Colors.indigo,
            brightness: Brightness.light,
          ),
          appBarTheme: const AppBarTheme(
            centerTitle: false,
            elevation: 0,
          ),
          cardTheme: CardThemeData(
            elevation: 2,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
        ),
        home: Consumer<AuthProvider>(
          builder: (context, auth, _) {
            if (auth.isLoading) {
              return const Scaffold(
                body: Center(
                  child: CircularProgressIndicator(),
                ),
              );
            }
            if (auth.isAuthenticated) {
              if (auth.isDoctor) {
                return const DoctorDashboardScreen();
              }
              return const HomeNavigationScreen();
            }
            return const LoginScreen();
          },
        ),
      ),
    );
  }
}
