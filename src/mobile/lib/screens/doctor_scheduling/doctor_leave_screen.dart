import 'package:flutter/material.dart';
import '../../models/doctor_scheduling_models.dart';

class DoctorLeaveScreen extends StatefulWidget {
  const DoctorLeaveScreen({Key? key}) : super(key: key);

  @override
  State<DoctorLeaveScreen> createState() => _DoctorLeaveScreenState();
}

class _DoctorLeaveScreenState extends State<DoctorLeaveScreen> {
  final List<DoctorLeaveModel> _leaves = [
    DoctorLeaveModel(
      id: 1,
      doctorId: 1,
      startDate: DateTime.now().add(const Duration(days: 5)),
      endDate: DateTime.now().add(const Duration(days: 7)),
      reason: 'Medical Seminar Attendance',
      status: LeaveStatus.pending,
    ),
  ];

  final TextEditingController _reasonController = TextEditingController();
  DateTime? _startDate;
  DateTime? _endDate;

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  void _submitLeaveRequest() {
    if (_startDate == null || _endDate == null || _reasonController.text.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please complete all date and reason fields.')),
      );
      return;
    }

    setState(() {
      _leaves.add(
        DoctorLeaveModel(
          id: _leaves.length + 1,
          doctorId: 1,
          startDate: _startDate!,
          endDate: _endDate!,
          reason: _reasonController.text,
          status: LeaveStatus.pending,
        ),
      );
      _reasonController.clear();
      _startDate = null;
      _endDate = null;
    });

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Leave application submitted for staff approval.'),
        backgroundColor: Colors.green,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Apply for Leave'),
        backgroundColor: Colors.indigo,
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Card(
              elevation: 2,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('New Leave Application',
                        style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        Expanded(
                          child: TextButton.icon(
                            icon: const Icon(Icons.calendar_today),
                            label: Text(_startDate == null
                                ? 'Start Date'
                                : '${_startDate!.day}/${_startDate!.month}/${_startDate!.year}'),
                            onPressed: () async {
                              final picked = await showDatePicker(
                                context: context,
                                initialDate: DateTime.now().add(const Duration(days: 1)),
                                firstDate: DateTime.now(),
                                lastDate: DateTime.now().add(const Duration(days: 90)),
                              );
                              if (picked != null) setState(() => _startDate = picked);
                            },
                          ),
                        ),
                        Expanded(
                          child: TextButton.icon(
                            icon: const Icon(Icons.calendar_today),
                            label: Text(_endDate == null
                                ? 'End Date'
                                : '${_endDate!.day}/${_endDate!.month}/${_endDate!.year}'),
                            onPressed: () async {
                              final picked = await showDatePicker(
                                context: context,
                                initialDate: DateTime.now().add(const Duration(days: 2)),
                                firstDate: DateTime.now(),
                                lastDate: DateTime.now().add(const Duration(days: 90)),
                              );
                              if (picked != null) setState(() => _endDate = picked);
                            },
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: _reasonController,
                      decoration: const InputDecoration(
                        labelText: 'Reason for Leave',
                        border: OutlineInputBorder(),
                      ),
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        style: ElevatedButton.styleFrom(backgroundColor: Colors.indigo),
                        onPressed: _submitLeaveRequest,
                        child: const Text('Submit Application'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),
            const Text('My Leave History & Status',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
            const SizedBox(height: 10),
            Expanded(
              child: ListView.builder(
                itemCount: _leaves.length,
                itemBuilder: (context, index) {
                  final item = _leaves[index];
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    child: ListTile(
                      title: Text('${item.startDate.day}/${item.startDate.month} - ${item.endDate.day}/${item.endDate.month}'),
                      subtitle: Text(item.reason),
                      trailing: Chip(
                        label: Text(item.status.name.toUpperCase()),
                        backgroundColor: item.status == LeaveStatus.approved
                            ? Colors.green.shade100
                            : item.status == LeaveStatus.rejected
                                ? Colors.red.shade100
                                : Colors.amber.shade100,
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
