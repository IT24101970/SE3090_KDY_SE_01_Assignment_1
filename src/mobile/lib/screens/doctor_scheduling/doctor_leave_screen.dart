import 'package:flutter/material.dart';
import '../../models/doctor_scheduling_models.dart';
import '../../services/doctor_scheduling_service.dart';

class DoctorLeaveScreen extends StatefulWidget {
  final int doctorId;

  const DoctorLeaveScreen({super.key, this.doctorId = 1});

  @override
  State<DoctorLeaveScreen> createState() => _DoctorLeaveScreenState();
}

class _DoctorLeaveScreenState extends State<DoctorLeaveScreen> {
  List<DoctorLeaveModel> _leaves = [];
  bool _isLoading = true;
  bool _isSubmitting = false;
  String? _errorMessage;

  final TextEditingController _reasonController = TextEditingController();
  DateTime? _startDate;
  DateTime? _endDate;

  @override
  void initState() {
    super.initState();
    _loadDoctorLeavesFromDb();
  }

  Future<void> _loadDoctorLeavesFromDb() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final leaves = await DoctorSchedulingService.getDoctorLeaves(widget.doctorId);
      if (!mounted) return;
      setState(() {
        _leaves = leaves;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _errorMessage = 'Failed to load leave records from database: $e';
      });
    }
  }

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  Future<void> _submitLeaveRequest() async {
    if (_startDate == null || _endDate == null || _reasonController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please complete all date and reason fields.')),
      );
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      await DoctorSchedulingService.submitDoctorLeave(
        doctorId: widget.doctorId,
        startDate: _startDate!,
        endDate: _endDate!,
        reason: _reasonController.text.trim(),
      );

      if (!mounted) return;
      _reasonController.clear();
      _startDate = null;
      _endDate = null;
      _isSubmitting = false;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Leave application saved to database successfully.'),
          backgroundColor: Colors.green,
        ),
      );

      _loadDoctorLeavesFromDb();
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to submit leave: $e'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Apply for Leave (DB)'),
        backgroundColor: Colors.indigo,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadDoctorLeavesFromDb,
          )
        ],
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
                        onPressed: _isSubmitting ? null : _submitLeaveRequest,
                        child: _isSubmitting
                            ? const SizedBox(
                                height: 20,
                                width: 20,
                                child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                              )
                            : const Text('Submit Application to DB'),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 20),
            const Text('My Leave History & Status (DB)',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
            const SizedBox(height: 10),
            if (_errorMessage != null)
              Text(_errorMessage!, style: const TextStyle(color: Colors.red))
            else if (_isLoading)
              const Center(child: CircularProgressIndicator())
            else if (_leaves.isEmpty)
              const Center(child: Text('No leave applications recorded in database.'))
            else
              Expanded(
                child: ListView.builder(
                  itemCount: _leaves.length,
                  itemBuilder: (context, index) {
                    final item = _leaves[index];
                    return Card(
                      margin: const EdgeInsets.only(bottom: 10),
                      child: ListTile(
                        title: Text('${item.startDate.day}/${item.startDate.month}/${item.startDate.year} - ${item.endDate.day}/${item.endDate.month}/${item.endDate.year}'),
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
