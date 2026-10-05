import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';

class PatientRegisterScreen extends StatefulWidget {
  const PatientRegisterScreen({super.key});

  @override
  State<PatientRegisterScreen> createState() => _PatientRegisterScreenState();
}

class _PatientRegisterScreenState extends State<PatientRegisterScreen> {
  final _formKey = GlobalKey<FormState>();

  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _nicController = TextEditingController();
  final _phoneController = TextEditingController();
  final _emergencyController = TextEditingController();
  final _allergiesController = TextEditingController();
  final _medicalHistoryController = TextEditingController();

  DateTime? _selectedDob;
  String _selectedGender = 'Male';
  String _selectedBloodGroup = 'O+';

  final List<String> _genders = ['Male', 'Female', 'Other'];
  final List<String> _bloodGroups = ['O+', 'O-', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'Unknown'];

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _nicController.dispose();
    _phoneController.dispose();
    _emergencyController.dispose();
    _allergiesController.dispose();
    _medicalHistoryController.dispose();
    super.dispose();
  }

  Future<void> _pickDateOfBirth() async {
    final now = DateTime.now();
    final initialDate = DateTime(now.year - 25, now.month, now.day);
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDob ?? initialDate,
      firstDate: DateTime(1900),
      lastDate: now,
    );
    if (picked != null) {
      setState(() {
        _selectedDob = picked;
      });
    }
  }

  Future<void> _submitRegistration() async {
    if (!_formKey.currentState!.validate()) return;

    if (_selectedDob == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please select your date of birth'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final success = await authProvider.registerPatient(
      fullName: _fullNameController.text.trim(),
      email: _emailController.text.trim(),
      password: _passwordController.text.trim(),
      nic: _nicController.text.trim(),
      phoneNumber: _phoneController.text.trim(),
      dateOfBirth: _selectedDob!,
      gender: _selectedGender,
      emergencyContact: _emergencyController.text.trim(),
      bloodGroup: _selectedBloodGroup,
      allergies: _allergiesController.text.trim().isNotEmpty ? _allergiesController.text.trim() : null,
      medicalHistory: _medicalHistoryController.text.trim().isNotEmpty ? _medicalHistoryController.text.trim() : null,
    );

    if (success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Registration successful! Welcome, ${authProvider.currentUser?.fullName}'),
          backgroundColor: Colors.green.shade700,
        ),
      );
      Navigator.of(context).pop(); // Return to previous screen or main app flow
    }
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);

    return Scaffold(
      backgroundColor: Colors.grey.shade100,
      appBar: AppBar(
        title: const Text('Patient Registration'),
        backgroundColor: Colors.teal,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20.0),
          child: Card(
            elevation: 3,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            child: Padding(
              padding: const EdgeInsets.all(20.0),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.person_add_alt_1_rounded, size: 36, color: Colors.teal),
                        const SizedBox(width: 12),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Create Patient Account',
                              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                    fontWeight: FontWeight.bold,
                                    color: Colors.teal.shade900,
                                  ),
                            ),
                            const Text(
                              'Register for doctor channeling & medical services',
                              style: TextStyle(fontSize: 12, color: Colors.grey),
                            ),
                          ],
                        ),
                      ],
                    ),
                    const Divider(height: 32),

                    // Error banner
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

                    // Full Name
                    TextFormField(
                      controller: _fullNameController,
                      decoration: InputDecoration(
                        labelText: 'Full Legal Name *',
                        prefixIcon: const Icon(Icons.person_outline),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      validator: (val) => val == null || val.trim().length < 2
                          ? 'Enter a valid full name'
                          : null,
                    ),
                    const SizedBox(height: 14),

                    // Email Address
                    TextFormField(
                      controller: _emailController,
                      keyboardType: TextInputType.emailAddress,
                      decoration: InputDecoration(
                        labelText: 'Email Address *',
                        prefixIcon: const Icon(Icons.email_outlined),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      validator: (val) {
                        if (val == null || val.trim().isEmpty) return 'Email is required';
                        if (!RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$').hasMatch(val.trim())) {
                          return 'Enter a valid email address';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 14),

                    // Password
                    TextFormField(
                      controller: _passwordController,
                      obscureText: true,
                      decoration: InputDecoration(
                        labelText: 'Password *',
                        prefixIcon: const Icon(Icons.lock_outline),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      validator: (val) => val == null || val.length < 6
                          ? 'Password must be at least 6 characters'
                          : null,
                    ),
                    const SizedBox(height: 14),

                    // Confirm Password
                    TextFormField(
                      controller: _confirmPasswordController,
                      obscureText: true,
                      decoration: InputDecoration(
                        labelText: 'Confirm Password *',
                        prefixIcon: const Icon(Icons.lock_reset_outlined),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      validator: (val) {
                        if (val == null || val.isEmpty) return 'Confirm your password';
                        if (val != _passwordController.text) return 'Passwords do not match';
                        return null;
                      },
                    ),
                    const SizedBox(height: 14),

                    // NIC & Phone Row
                    Row(
                      children: [
                        Expanded(
                          child: TextFormField(
                            controller: _nicController,
                            decoration: InputDecoration(
                              labelText: 'NIC / Gov ID *',
                              prefixIcon: const Icon(Icons.badge_outlined),
                              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                            ),
                            validator: (val) => val == null || val.trim().isEmpty
                                ? 'NIC required'
                                : null,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: TextFormField(
                            controller: _phoneController,
                            keyboardType: TextInputType.phone,
                            decoration: InputDecoration(
                              labelText: 'Phone Number *',
                              prefixIcon: const Icon(Icons.phone_outlined),
                              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                            ),
                            validator: (val) => val == null || val.trim().isEmpty
                                ? 'Phone required'
                                : null,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 14),

                    // Date of Birth picker
                    InkWell(
                      onTap: _pickDateOfBirth,
                      borderRadius: BorderRadius.circular(12),
                      child: InputDecorator(
                        decoration: InputDecoration(
                          labelText: 'Date of Birth *',
                          prefixIcon: const Icon(Icons.calendar_today_outlined),
                          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                        child: Text(
                          _selectedDob == null
                              ? 'Tap to select date of birth'
                              : DateFormat('yyyy-MM-dd').format(_selectedDob!),
                          style: TextStyle(
                            color: _selectedDob == null ? Colors.grey.shade600 : Colors.black,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 14),

                    // Gender & Blood Group Row
                    Row(
                      children: [
                        Expanded(
                          child: DropdownButtonFormField<String>(
                            initialValue: _selectedGender,
                            decoration: InputDecoration(
                              labelText: 'Gender *',
                              prefixIcon: const Icon(Icons.wc_outlined),
                              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                            ),
                            items: _genders.map((g) => DropdownMenuItem(value: g, child: Text(g))).toList(),
                            onChanged: (val) {
                              if (val != null) setState(() => _selectedGender = val);
                            },
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: DropdownButtonFormField<String>(
                            initialValue: _selectedBloodGroup,
                            decoration: InputDecoration(
                              labelText: 'Blood Group',
                              prefixIcon: const Icon(Icons.invert_colors_outlined),
                              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                            ),
                            items: _bloodGroups.map((bg) => DropdownMenuItem(value: bg, child: Text(bg))).toList(),
                            onChanged: (val) {
                              if (val != null) setState(() => _selectedBloodGroup = val);
                            },
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 14),

                    // Emergency Contact
                    TextFormField(
                      controller: _emergencyController,
                      decoration: InputDecoration(
                        labelText: 'Emergency Contact (Name & Phone) *',
                        prefixIcon: const Icon(Icons.contact_phone_outlined),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      validator: (val) => val == null || val.trim().isEmpty
                          ? 'Emergency contact is required'
                          : null,
                    ),
                    const SizedBox(height: 14),

                    // Allergies
                    TextFormField(
                      controller: _allergiesController,
                      decoration: InputDecoration(
                        labelText: 'Known Allergies (Optional)',
                        prefixIcon: const Icon(Icons.warning_amber_rounded),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                    ),
                    const SizedBox(height: 14),

                    // Medical History
                    TextFormField(
                      controller: _medicalHistoryController,
                      maxLines: 2,
                      decoration: InputDecoration(
                        labelText: 'Medical History / Chronic Conditions (Optional)',
                        prefixIcon: const Icon(Icons.notes_outlined),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Submit Button
                    ElevatedButton(
                      onPressed: authProvider.isLoading ? null : _submitRegistration,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.teal,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: authProvider.isLoading
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(
                                color: Colors.white,
                                strokeWidth: 2,
                              ),
                            )
                          : const Text(
                              'Complete Patient Registration',
                              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                            ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
