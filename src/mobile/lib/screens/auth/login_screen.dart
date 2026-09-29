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
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 28.0, vertical: 24.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text(
                  'ChannelCenter',
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: Colors.indigo,
                    letterSpacing: -0.5,
                  ),
                ),
                const SizedBox(height: 4),
                const Text(
                  'Hospital Management Portal',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 14, color: Colors.black54),
                ),
                const SizedBox(height: 28),

                Container(
                  padding: const EdgeInsets.all(24.0),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade50,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: Colors.grey.shade300),
                  ),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        const Text(
                          'Select Account Role',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                          ),
                        ),
                        const SizedBox(height: 12),

                        SegmentedButton<LoginRole>(
                          segments: const [
                            ButtonSegment<LoginRole>(
                              value: LoginRole.doctor,
                              label: Text('Doctor'),
                            ),
                            ButtonSegment<LoginRole>(
                              value: LoginRole.patient,
                              label: Text('Patient'),
                            ),
                            ButtonSegment<LoginRole>(
                              value: LoginRole.admin,
                              label: Text('Admin'),
                            ),
                          ],
                          selected: {_selectedRole},
                          onSelectionChanged: (Set<LoginRole> selection) {
                            _applyRoleDefaults(selection.first);
                          },
                        ),
                        const SizedBox(height: 20),

                        if (authProvider.errorMessage != null) ...[
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: Colors.red.shade50,
                              borderRadius: BorderRadius.circular(8),
                              border: Border.all(color: Colors.red.shade200),
                            ),
                            child: Text(
                              authProvider.errorMessage!,
                              style: const TextStyle(color: Colors.red, fontSize: 13),
                            ),
                          ),
                          const SizedBox(height: 16),
                        ],

                        TextFormField(
                          controller: _emailController,
                          keyboardType: TextInputType.emailAddress,
                          decoration: InputDecoration(
                            labelText: _selectedRole == LoginRole.doctor
                                ? 'Doctor Email'
                                : 'Email Address',
                            filled: true,
                            fillColor: Colors.white,
                            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10),
                            ),
                          ),
                          validator: (val) =>
                              val == null || val.isEmpty ? 'Please enter email' : null,
                        ),
                        const SizedBox(height: 16),

                        TextFormField(
                          controller: _passwordController,
                          obscureText: true,
                          decoration: InputDecoration(
                            labelText: 'Password',
                            filled: true,
                            fillColor: Colors.white,
                            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10),
                            ),
                          ),
                          validator: (val) =>
                              val == null || val.isEmpty ? 'Please enter password' : null,
                        ),
                        const SizedBox(height: 24),

                        SizedBox(
                          height: 50,
                          child: ElevatedButton(
                            onPressed: isBusy ? null : _submitLogin,
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Colors.indigo,
                              foregroundColor: Colors.white,
                              elevation: 0,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(10),
                              ),
                            ),
                            child: isBusy
                                ? const SizedBox(
                                    height: 22,
                                    width: 22,
                                    child: CircularProgressIndicator(
                                      color: Colors.white,
                                      strokeWidth: 2,
                                    ),
                                  )
                                : Text(
                                    _selectedRole == LoginRole.doctor
                                        ? 'Log In as Doctor'
                                        : (_selectedRole == LoginRole.admin ? 'Sign In as Admin' : 'Sign In as Patient'),
                                    style: const TextStyle(
                                      fontSize: 16,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
