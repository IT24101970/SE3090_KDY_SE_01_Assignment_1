import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'workflow/emergency_alert_screen.dart';
import 'symptom_wizard_screen.dart';
import 'triage_history_screen.dart';
import 'doctor_scheduling/doctor_dashboard_screen.dart';

class HomeNavigationScreen extends StatefulWidget {
  const HomeNavigationScreen({Key? key}) : super(key: key);

  @override
  State<HomeNavigationScreen> createState() => _HomeNavigationScreenState();
}

class _HomeNavigationScreenState extends State<HomeNavigationScreen> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthProvider>(context);
    final user = auth.currentUser;

    // Define navigation items and pages based on user role
    final List<Widget> pages = [
      const EmergencyAlertScreen(),
      const SymptomWizardScreen(),
      const TriageHistoryScreen(),
    ];

    final List<BottomNavigationBarItem> navItems = [
      const BottomNavigationBarItem(
        icon: Icon(Icons.warning_amber_rounded),
        activeIcon: Icon(Icons.warning_amber),
        label: 'Safety Alerts',
      ),
      const BottomNavigationBarItem(
        icon: Icon(Icons.medical_services_outlined),
        activeIcon: Icon(Icons.medical_services),
        label: 'Triage Wizard',
      ),
      const BottomNavigationBarItem(
        icon: Icon(Icons.history),
        activeIcon: Icon(Icons.manage_history),
        label: 'History',
      ),
    ];

    // If Doctor or Staff or Admin, append Doctor Scheduling screen option
    if (auth.isDoctor || auth.isAdmin || auth.isStaff) {
      pages.add(const DoctorDashboardScreen());
      navItems.add(
        const BottomNavigationBarItem(
          icon: Icon(Icons.calendar_month_outlined),
          activeIcon: Icon(Icons.calendar_month),
          label: 'Scheduling',
        ),
      );
    }

    return Scaffold(
      appBar: AppBar(
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
        elevation: 2,
        title: Row(
          children: [
            const Icon(Icons.local_hospital, color: Colors.white),
            const SizedBox(width: 8),
            Text(
              user != null ? '${user.fullName} (${user.role})' : 'ChannelCenter App',
              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Logout',
            onPressed: () async {
              final confirm = await showDialog<bool>(
                context: context,
                builder: (ctx) => AlertDialog(
                  title: const Text('Confirm Logout'),
                  content: const Text('Are you sure you want to log out?'),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(ctx, false),
                      child: const Text('Cancel'),
                    ),
                    ElevatedButton(
                      onPressed: () => Navigator.pop(ctx, true),
                      child: const Text('Logout'),
                    ),
                  ],
                ),
              );

              if (confirm == true) {
                await auth.logout();
              }
            },
          ),
        ],
      ),
      body: IndexedStack(
        index: _currentIndex < pages.length ? _currentIndex : 0,
        children: pages,
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex < navItems.length ? _currentIndex : 0,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.indigo,
        unselectedItemColor: Colors.grey.shade600,
        onTap: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
        items: navItems,
      ),
    );
  }
}
