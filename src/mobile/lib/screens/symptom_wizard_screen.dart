import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../models/triage_models.dart';
import '../providers/triage_provider.dart';
import 'triage_status_screen.dart';

class SymptomWizardScreen extends StatefulWidget {
  const SymptomWizardScreen({Key? key}) : super(key: key);

  @override
  State<SymptomWizardScreen> createState() => _SymptomWizardScreenState();
}

class _SymptomWizardScreenState extends State<SymptomWizardScreen> {
  int _currentStep = 0;

  final _formKeyStep1 = GlobalKey<FormState>();
  final _formKeyStep2 = GlobalKey<FormState>();

  final _medicalHistoryController = TextEditingController();
  final _allergiesController = TextEditingController();
  final _rawSymptomsController = TextEditingController();
  final _symptomKeywordController = TextEditingController();

  int _appointmentId = 42;
  double _severityRating = 5;
  int _durationInDays = 3;
  DateTime _onsetDate = DateTime.now();

  final List<SymptomItem> _symptomItems = [];

  void _addSymptomItem() {
    final keyword = _symptomKeywordController.text.trim();
    if (keyword.isNotEmpty) {
      setState(() {
        _symptomItems.add(SymptomItem(
          symptomKeyword: keyword,
          severityRating: _severityRating.toInt(),
          durationInDays: _durationInDays,
        ));
        _symptomKeywordController.clear();
      });
    }
  }

  void _submitWizard() async {
    if (_symptomItems.isEmpty && _rawSymptomsController.text.trim().isNotEmpty) {
      _symptomItems.add(SymptomItem(
        symptomKeyword: _rawSymptomsController.text.trim(),
        severityRating: _severityRating.toInt(),
        durationInDays: _durationInDays,
      ));
    }

    final provider = Provider.of<TriageProvider>(context, listen: false);
    final success = await provider.submitIntakeAndProcessTriage(
      appointmentId: _appointmentId,
      rawSymptoms: _rawSymptomsController.text.trim(),
      medicalHistory: _medicalHistoryController.text.trim(),
      allergies: _allergiesController.text.trim(),
      symptomList: _symptomItems,
    );

    if (success && mounted) {
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (_) => const TriageStatusScreen()),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      appBar: AppBar(
        title: const Text('Symptom Intake & AI Triage'),
        backgroundColor: const Color(0xFF1E293B),
        elevation: 0,
      ),
      body: Consumer<TriageProvider>(
        builder: (context, provider, child) {
          if (provider.isLoading) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(color: Colors.cyan),
                  SizedBox(height: 16),
                  Text(
                    'Symptom Triage Agent is evaluating your intake...',
                    style: TextStyle(color: Colors.white70, fontSize: 14),
                  ),
                ],
              ),
            );
          }

          return Stepper(
            type: StepperType.horizontal,
            currentStep: _currentStep,
            onStepContinue: () {
              if (_currentStep == 0) {
                if (_formKeyStep1.currentState!.validate()) {
                  setState(() => _currentStep = 1);
                }
              } else if (_currentStep == 1) {
                if (_rawSymptomsController.text.trim().isEmpty && _symptomItems.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Please describe your symptoms or add a symptom entry.')),
                  );
                  return;
                }
                setState(() => _currentStep = 2);
              } else {
                _submitWizard();
              }
            },
            onStepCancel: () {
              if (_currentStep > 0) {
                setState(() => _currentStep -= 1);
              }
            },
            steps: [
              // Step 1: Pre-Consultation Questionnaire
              Step(
                title: const Text('Intake'),
                isActive: _currentStep >= 0,
                content: Form(
                  key: _formKeyStep1,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Pre-Consultation Questionnaire',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _medicalHistoryController,
                        style: const TextStyle(color: Colors.white),
                        decoration: _inputDecoration('Medical History (e.g. Hypertension, Diabetes)'),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _allergiesController,
                        style: const TextStyle(color: Colors.white),
                        decoration: _inputDecoration('Known Allergies (e.g. Penicillin, Pollen)'),
                      ),
                    ],
                  ),
                ),
              ),

              // Step 2: Symptom Assessment & Onset
              Step(
                title: const Text('Symptoms'),
                isActive: _currentStep >= 1,
                content: Form(
                  key: _formKeyStep2,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Symptom Description & Severity',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _rawSymptomsController,
                        style: const TextStyle(color: Colors.white),
                        maxLines: 2,
                        decoration: _inputDecoration('Describe your main symptoms (e.g. chest pain, rash, fever)'),
                      ),
                      const SizedBox(height: 16),

                      Row(
                        children: [
                          Expanded(
                            child: TextFormField(
                              controller: _symptomKeywordController,
                              style: const TextStyle(color: Colors.white),
                              decoration: _inputDecoration('Add symptom keyword'),
                            ),
                          ),
                          const SizedBox(width: 8),
                          ElevatedButton(
                            onPressed: _addSymptomItem,
                            style: ElevatedButton.styleFrom(backgroundColor: Colors.cyan),
                            child: const Text('Add'),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),

                      if (_symptomItems.isNotEmpty)
                        Wrap(
                          spacing: 8,
                          children: _symptomItems.map((item) => Chip(
                            label: Text('${item.symptomKeyword} (${item.severityRating}/10)'),
                            backgroundColor: Colors.cyan.withOpacity(0.2),
                            labelStyle: const TextStyle(color: Colors.cyanAccent),
                          )).toList(),
                        ),

                      const SizedBox(height: 16),
                      Text(
                        'Severity Rating: ${_severityRating.toInt()}/10',
                        style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.w600),
                      ),
                      Slider(
                        value: _severityRating,
                        min: 1,
                        max: 10,
                        divisions: 9,
                        activeColor: Colors.cyan,
                        label: '${_severityRating.toInt()}',
                        onChanged: (val) => setState(() => _severityRating = val),
                      ),

                      const SizedBox(height: 8),
                      ListTile(
                        contentPadding: EdgeInsets.zero,
                        title: const Text('Symptom Onset Date', style: TextStyle(color: Colors.white)),
                        subtitle: Text(DateFormat('yyyy-MM-dd').format(_onsetDate), style: const TextStyle(color: Colors.cyanAccent)),
                        trailing: const Icon(Icons.calendar_month, color: Colors.cyan),
                        onTap: () async {
                          final picked = await showDatePicker(
                            context: context,
                            initialDate: _onsetDate,
                            firstDate: DateTime(2020),
                            lastDate: DateTime.now(),
                          );
                          if (picked != null) setState(() => _onsetDate = picked);
                        },
                      ),
                    ],
                  ),
                ),
              ),

              // Step 3: Review & AI Submission
              Step(
                title: const Text('Review'),
                isActive: _currentStep >= 2,
                content: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Confirm Intake Submission',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                    ),
                    const SizedBox(height: 12),
                    _summaryRow('Symptoms', _rawSymptomsController.text.trim()),
                    _summaryRow('History', _medicalHistoryController.text.trim().isEmpty ? 'None' : _medicalHistoryController.text.trim()),
                    _summaryRow('Severity Rating', '${_severityRating.toInt()}/10'),
                    _summaryRow('Onset Date', DateFormat('yyyy-MM-dd').format(_onsetDate)),
                    const SizedBox(height: 16),
                    const Text(
                      'Submitting will trigger the AI Symptom Triage Agent to score urgency and match you to a specialist.',
                      style: TextStyle(color: Colors.white60, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  InputDecoration _inputDecoration(String label) {
    return InputDecoration(
      labelText: label,
      labelStyle: const TextStyle(color: Colors.white70),
      filled: true,
      fillColor: const Color(0xFF1E293B),
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: BorderSide.none),
      enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: Colors.white12)),
      focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: Colors.cyan)),
    );
  }

  Widget _summaryRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(width: 120, child: Text('$label:', style: const TextStyle(color: Colors.white70, fontWeight: FontWeight.bold))),
          Expanded(child: Text(value, style: const TextStyle(color: Colors.white))),
        ],
      ),
    );
  }
}
