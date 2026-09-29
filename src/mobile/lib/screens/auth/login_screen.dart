import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/doctor_scheduling_models.dart';
import '../../providers/auth_provider.dart';
import '../../services/doctor_scheduling_service.dart';
import '../doctor_scheduling/doctor_dashboard_screen.dart';

enum LoginRole { doctor, patient, admin }

class LoginScreen extends StatefulWidget {
  const LoginScreen({Key? key}) : super(key: key);

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  
  LoginRole _selectedRole = LoginRole.doctor;
  bool _isLoadingDoctor = false;
  List<DoctorProfile> _doctors = [];

  @override
  void initState() {
    super.initState();
    _applyRoleDefaults(_selectedRole);
    _loadDoctorsFromDatabase();
  }

  Future<void> _loadDoctorsFromDatabase() async {
    try {
      final fetched = await DoctorSchedulingService.getDoctors();
      if (!mounted) return;
      setState(() {
        _doctors = fetched;
        if (_selectedRole == LoginRole.doctor && _doctors.isNotEmpty) {
          _emailController.text = _doctors.first.email;
        }
      });
    } catch (_) {}
  }

  void _applyRoleDefaults(LoginRole role) {
    setState(() {
      _selectedRole = role;
      if (role == LoginRole.doctor) {
        _emailController.text = _doctors.isNotEmpty
            ? _doctors.first.email
            : 'sarah.jenkins@channelcenter.hospital';
        _passwordController.text = 'DoctorPass123!';
      } else if (role == LoginRole.patient) {
        _emailController.text = 'john.doe@example.com';
        _passwordController.text = 'Password123!';
      } else {
        _emailController.text = 'admin@channelcenter.hospital';
        _passwordController.text = 'Admin123!';
      }
    });
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submitLogin() async {
    if (!_formKey.currentState!.validate()) return;

    if (_selectedRole == LoginRole.doctor) {
      setState(() {
        _isLoadingDoctor = true;
      });

      final email = _emailController.text.trim().toLowerCase();
      DoctorProfile doctorProfile;

      final matched = _doctors.where(
        (d) => d.email.toLowerCase() == email || d.name.toLowerCase().contains(email.replaceAll('.', ' ')),
      ).toList();

      if (matched.isNotEmpty) {
        doctorProfile = matched.first;
      } else if (_doctors.isNotEmpty) {
        doctorProfile = _doctors.first;
      } else {
        doctorProfile = DoctorProfile(
          id: 1,
          name: 'Dr. Sarah Jenkins',
          specialty: 'Cardiology',
          email: email.isNotEmpty ? email : 'sarah.jenkins@channelcenter.hospital',
        );
      }

      await Future.delayed(const Duration(milliseconds: 300));
      if (!mounted) return;
      setState(() {
        _isLoadingDoctor = false;
      });

      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => DoctorDashboardScreen(doctorProfile: doctorProfile),
        ),
      );
      return;
    }

    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final success = await authProvider.login(
      email: _emailController.text.trim(),
      password: _passwordController.text.trim(),
      isAdmin: _selectedRole == LoginRole.admin,
    );

    if (success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Welcome back, ${authProvider.currentUser?.fullName}!'),
          backgroundColor: Colors.green.shade700,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final isBusy = authProvider.isLoading || _isLoadingDoctor;

    return Scaffold(
      backgroundColor: Colors.grey.shade100,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Card(
              elevation: 4,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(20),
              ),
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Form(
                  key: _formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      // Header Logo & Title
                      Icon(
                        Icons.medical_services_rounded,
                        size: 56,
                        color: _selectedRole == LoginRole.doctor
                            ? Colors.indigo
                            : (_selectedRole == LoginRole.admin ? Colors.deepOrange : Colors.teal),
                      ),
                      const SizedBox(height: 12),
                      Text(
                        'ChannelCenter Hospital',
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              fontWeight: FontWeight.bold,
                              color: Colors.indigo,
                            ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Unified Authentication Portal',
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey.shade600,
                        ),
                      ),
                      const SizedBox(height: 24),

                      // Role Switcher
                      SegmentedButton<LoginRole>(
                        segments: const [
                          ButtonSegment<LoginRole>(
                            value: LoginRole.doctor,
                            label: Text('Doctor'),
                            icon: Icon(Icons.medical_services),
                          ),
                          ButtonSegment<LoginRole>(
                            value: LoginRole.patient,
                            label: Text('Patient'),
                            icon: Icon(Icons.person),
                          ),
                          ButtonSegment<LoginRole>(
                            value: LoginRole.admin,
                            label: Text('Admin'),
                            icon: Icon(Icons.admin_panel_settings),
                          ),
                        ],
                        selected: {_selectedRole},
                        onSelectionChanged: (Set<LoginRole> selection) {
                          _applyRoleDefaults(selection.first);
                        },
                      ),
                      const SizedBox(height: 20),

                      // Error message banner
                      if (authProvider.errorMessage != null) ...[
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.red.shade50,
                            borderRadius: BorderRadius.circular(10),
                            border: Border.all(color: Colors.red.shade200),
                          ),
                          child: Row(
                            children: [
                              const Icon(Icons.error_outline, color: Colors.red, size: 20),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  authProvider.errorMessage!,
                                  style: const TextStyle(color: Colors.red, fontSize: 13),
                                ),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 16),
                      ],

                      // Email input
                      TextFormField(
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        decoration: InputDecoration(
                          labelText: _selectedRole == LoginRole.doctor
                              ? 'Doctor Email'
                              : 'Email Address',
                          prefixIcon: const Icon(Icons.email_outlined),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        validator: (val) =>
                            val == null || val.isEmpty ? 'Please enter email' : null,
                      ),
                      const SizedBox(height: 16),

                      // Password input
                      TextFormField(
                        controller: _passwordController,
                        obscureText: true,
                        decoration: InputDecoration(
                          labelText: 'Password',
                          prefixIcon: const Icon(Icons.lock_outline),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        validator: (val) =>
                            val == null || val.isEmpty ? 'Please enter password' : null,
                      ),
                      const SizedBox(height: 24),

                      // Login Button
                      ElevatedButton(
                        onPressed: isBusy ? null : _submitLogin,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: _selectedRole == LoginRole.doctor
                              ? Colors.indigo
                              : (_selectedRole == LoginRole.admin ? Colors.deepOrange : Colors.teal),
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        child: isBusy
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(
                                  color: Colors.white,
                                  strokeWidth: 2,
                                ),
                              )
                            : Text(
                                _selectedRole == LoginRole.doctor
                                    ? 'Log In as Specialist Doctor'
                                    : (_selectedRole == LoginRole.admin ? 'Sign In as Admin' : 'Sign In as Patient'),
                                style: const TextStyle(
                                  fontSize: 15,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
