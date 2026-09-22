import 'package:flutter/material.dart';
import '../../models/doctor_scheduling_models.dart';

class ConsultationAttendanceScreen extends StatefulWidget {
  final PatientQueueItem patient;

  const ConsultationAttendanceScreen({Key? key, required this.patient})
      : super(key: key);

  @override
  State<ConsultationAttendanceScreen> createState() =>
      _ConsultationAttendanceScreenState();
}

class _ConsultationAttendanceScreenState
    extends State<ConsultationAttendanceScreen> {
  late AttendanceStatus _attendanceStatus;
  late TextEditingController _notesController;
  late TextEditingController _prescriptionController;

  @override
  void initState() {
    super.initState();
    _attendanceStatus = widget.patient.attendanceStatus;
    _notesController =
        TextEditingController(text: widget.patient.clinicalNotes);
    _prescriptionController =
        TextEditingController(text: widget.patient.prescriptionData);
  }

  @override
  void dispose() {
    _notesController.dispose();
    _prescriptionController.dispose();
    super.dispose();
  }

  void _saveConsultation() {
    setState(() {
      widget.patient.attendanceStatus = _attendanceStatus;
      widget.patient.clinicalNotes = _notesController.text;
      widget.patient.prescriptionData = _prescriptionController.text;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Consultation & Prescription saved successfully!'),
        backgroundColor: Colors.green,
      ),
    );

    Navigator.pop(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Consultation: Queue #${widget.patient.queueNumber}'),
        backgroundColor: Colors.indigo,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Card(
              elevation: 2,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  children: [
                    CircleAvatar(
                      backgroundColor: Colors.indigo.shade100,
                      radius: 26,
                      child: Text(
                        '#${widget.patient.queueNumber}',
                        style: const TextStyle(
                            fontWeight: FontWeight.bold,
                            color: Colors.indigo),
                      ),
                    ),
                    const SizedBox(width: 16),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          widget.patient.patientName,
                          style: const TextStyle(
                              fontSize: 18, fontWeight: FontWeight.bold),
                        ),
                        Text(
                          'Appointment ID: ${widget.patient.appointmentId}',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),
            const Text(
              'Patient Attendance Status',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            SegmentedButton<AttendanceStatus>(
              segments: const [
                ButtonSegment(
                  value: AttendanceStatus.pending,
                  label: Text('Pending'),
                  icon: Icon(Icons.hourglass_empty),
                ),
                ButtonSegment(
                  value: AttendanceStatus.present,
                  label: Text('Present'),
                  icon: Icon(Icons.check_circle),
                ),
                ButtonSegment(
                  value: AttendanceStatus.noShow,
                  label: Text('No-Show'),
                  icon: Icon(Icons.cancel),
                ),
              ],
              selected: {_attendanceStatus},
              onSelectionChanged: (newSelection) {
                setState(() {
                  _attendanceStatus = newSelection.first;
                });
              },
            ),
            const SizedBox(height: 20),
            const Text(
              'Clinical Notes',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _notesController,
              maxLines: 4,
              decoration: InputDecoration(
                hintText: 'Enter clinical observations, symptoms, diagnosis...',
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
            ),
            const SizedBox(height: 20),
            const Text(
              'Digital Prescription (Medications & Dosage)',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            TextField(
              controller: _prescriptionController,
              maxLines: 4,
              decoration: InputDecoration(
                hintText: 'e.g. Paracetamol 500mg - 1 tablet 3x daily for 5 days',
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton.icon(
                onPressed: _saveConsultation,
                icon: const Icon(Icons.save),
                label: const Text(
                  'Save Consultation & Issue Prescription',
                  style: TextStyle(fontSize: 16),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.indigo,
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10)),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
