import 'package:flutter/material.dart';
import 'screens/doctor_scheduling/doctor_dashboard_screen.dart';

void main() {
  runApp(const DoctorSchedulingApp());
}

class DoctorSchedulingApp extends StatelessWidget {
  const DoctorSchedulingApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Doctor Channeling & Consultation Management',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        primarySwatch: Colors.indigo,
        useMaterial3: true,
      ),
      home: const DoctorDashboardScreen(),
    );
  }
}
