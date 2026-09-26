import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../providers/auth_provider.dart';
import '../../providers/workflow_provider.dart';
import '../../models/workflow_models.dart';
import '../../widgets/status_badge.dart';
import '../../widgets/workflow_timeline_widget.dart';

class WorkflowStatusDetailScreen extends StatefulWidget {
  final int workflowId;

  const WorkflowStatusDetailScreen({
    Key? key,
    required this.workflowId,
  }) : super(key: key);

  @override
  State<WorkflowStatusDetailScreen> createState() =>
      _WorkflowStatusDetailScreenState();
}

class _WorkflowStatusDetailScreenState
    extends State<WorkflowStatusDetailScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadDetails();
    });
  }

  Future<void> _loadDetails() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final wfProvider = Provider.of<WorkflowProvider>(context, listen: false);
    await wfProvider.fetchWorkflowStatus(authProvider.token, widget.workflowId);
  }

  String _formatTimestamp(String raw) {
    if (raw.isEmpty) return 'N/A';
    try {
      final dt = DateTime.parse(raw).toLocal();
      return DateFormat('MMM dd, yyyy • HH:mm:ss').format(dt);
    } catch (_) {
      return raw;
    }
  }

  @override
  Widget build(BuildContext context) {
    final wfProvider = Provider.of<WorkflowProvider>(context);
    final alert = wfProvider.selectedWorkflow;

    return Scaffold(
      appBar: AppBar(
        title: Text('Workflow #${widget.workflowId} Details'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Status',
            onPressed: wfProvider.isLoading ? null : _loadDetails,
          ),
        ],
      ),
      body: wfProvider.isLoading
          ? const Center(child: CircularProgressIndicator())
          : wfProvider.errorMessage != null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.error_outline,
                            size: 48, color: Colors.red),
                        const SizedBox(height: 12),
                        Text(
                          wfProvider.errorMessage!,
                          textAlign: TextAlign.center,
                          style: const TextStyle(fontSize: 14),
                        ),
                        const SizedBox(height: 16),
                        ElevatedButton.icon(
                          onPressed: _loadDetails,
                          icon: const Icon(Icons.refresh),
                          label: const Text('Retry'),
                        ),
                      ],
                    ),
                  ),
                )
              : alert == null
                  ? const Center(child: Text('Workflow details unavailable.'))
                  : RefreshIndicator(
                      onRefresh: _loadDetails,
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Card 1: Main Overview & Risk Header
                            Card(
                              elevation: 2,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(16),
                              ),
                              child: Padding(
                                padding: const EdgeInsets.all(16.0),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.spaceBetween,
                                      children: [
                                        // Risk tag
                                        Container(
                                          padding: const EdgeInsets.symmetric(
                                              horizontal: 10, vertical: 4),
                                          decoration: BoxDecoration(
                                            color: alert.riskColor,
                                            borderRadius:
                                                BorderRadius.circular(8),
                                          ),
                                          child: Row(
                                            children: [
                                              Icon(alert.riskIcon,
                                                  color: Colors.white,
                                                  size: 14),
                                              const SizedBox(width: 4),
                                              Text(
                                                '${alert.riskLevel.toUpperCase()} RISK',
                                                style: const TextStyle(
                                                  color: Colors.white,
                                                  fontWeight: FontWeight.bold,
                                                  fontSize: 11,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                        StatusBadge(status: alert.status),
                                      ],
                                    ),
                                    const SizedBox(height: 14),
                                    Text(
                                      alert.objective,
                                      style: Theme.of(context)
                                          .textTheme
                                          .titleMedium
                                          ?.copyWith(
                                            fontWeight: FontWeight.bold,
                                            height: 1.3,
                                          ),
                                    ),
                                    const SizedBox(height: 12),
                                    const Divider(),
                                    const SizedBox(height: 8),

                                    // Key-value specs
                                    _buildDetailRow(
                                        'Workflow ID', '#${alert.id}'),
                                    _buildDetailRow('Correlation ID',
                                        alert.correlationId),
                                    _buildDetailRow('Contract Version',
                                        alert.contractVersion),
                                    _buildDetailRow('Created At',
                                        _formatTimestamp(alert.createdAt)),
                                    if (alert.appointmentId != null)
                                      _buildDetailRow('Appointment Ref',
                                          '#${alert.appointmentId}'),
                                  ],
                                ),
                              ),
                            ),
                            const SizedBox(height: 16),

                            // Required Action Banner
                            Container(
                              width: double.infinity,
                              padding: const EdgeInsets.all(14),
                              decoration: BoxDecoration(
                                color: alert.status ==
                                        WorkflowStatus.pausedForApproval
                                    ? Colors.amber.shade50
                                    : (alert.status == WorkflowStatus.approved ||
                                            alert.status ==
                                                WorkflowStatus.completed)
                                        ? Colors.green.shade50
                                        : Colors.red.shade50,
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(
                                  color: alert.status ==
                                          WorkflowStatus.pausedForApproval
                                      ? Colors.amber.shade400
                                      : (alert.status == WorkflowStatus.approved ||
                                              alert.status ==
                                                  WorkflowStatus.completed)
                                          ? Colors.green.shade400
                                          : Colors.red.shade300,
                                ),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      Icon(
                                        Icons.info_outline,
                                        color: alert.status ==
                                                WorkflowStatus.pausedForApproval
                                            ? Colors.amber.shade900
                                            : (alert.status ==
                                                        WorkflowStatus.approved ||
                                                    alert.status ==
                                                        WorkflowStatus.completed)
                                                ? Colors.green.shade900
                                                : Colors.red.shade900,
                                      ),
                                      const SizedBox(width: 8),
                                      const Text(
                                        'Operational Guidance',
                                        style: TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 14,
                                        ),
                                      ),
                                    ],
                                  ),
                                  const SizedBox(height: 6),
                                  Text(
                                    alert.requiredActionText,
                                    style: TextStyle(
                                      fontSize: 13,
                                      color: Colors.grey.shade900,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            const SizedBox(height: 16),

                            // Structured Plan Summary Card
                            if (alert.planSummary.isNotEmpty) ...[
                              _buildSectionTitle(context, 'Execution Plan'),
                              Card(
                                elevation: 1,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Padding(
                                  padding: const EdgeInsets.all(14.0),
                                  child: Text(
                                    alert.planSummary,
                                    style: const TextStyle(
                                        fontSize: 13, height: 1.4),
                                  ),
                                ),
                              ),
                              const SizedBox(height: 16),
                            ],

                            // Validation Summary Card
                            if (alert.validationSummary.isNotEmpty) ...[
                              _buildSectionTitle(
                                  context, 'Safety Auditor Validation'),
                              Card(
                                elevation: 1,
                                color: Colors.amber.shade50.withValues(alpha: 0.5),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                  side: BorderSide(
                                      color: Colors.amber.shade200),
                                ),
                                child: Padding(
                                  padding: const EdgeInsets.all(14.0),
                                  child: Text(
                                    alert.validationSummary,
                                    style: TextStyle(
                                      fontSize: 13,
                                      height: 1.4,
                                      color: Colors.brown.shade900,
                                    ),
                                  ),
                                ),
                              ),
                              const SizedBox(height: 16),
                            ],

                            // Final Outcome or Error Card
                            if (alert.finalOutcome.isNotEmpty ||
                                alert.errorMessage != null) ...[
                              _buildSectionTitle(context, 'Final Outcome'),
                              Card(
                                elevation: 1,
                                color: alert.errorMessage != null
                                    ? Colors.red.shade50
                                    : Colors.green.shade50,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                                child: Padding(
                                  padding: const EdgeInsets.all(14.0),
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      if (alert.finalOutcome.isNotEmpty)
                                        Text(
                                          alert.finalOutcome,
                                          style: const TextStyle(
                                              fontSize: 13, height: 1.4),
                                        ),
                                      if (alert.errorMessage != null) ...[
                                        if (alert.finalOutcome.isNotEmpty)
                                          const SizedBox(height: 8),
                                        Text(
                                          'Error [${alert.errorCode ?? "ERR"}]: ${alert.errorMessage}',
                                          style: const TextStyle(
                                            fontSize: 13,
                                            color: Colors.red,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ],
                                    ],
                                  ),
                                ),
                              ),
                              const SizedBox(height: 16),
                            ],

                            // Audit Step Timeline
                            _buildSectionTitle(
                                context, 'Agent Step Audit Log'),
                            const SizedBox(height: 8),
                            WorkflowTimelineWidget(auditLogs: alert.auditLogs),
                            const SizedBox(height: 24),
                          ],
                        ),
                      ),
                    ),
    );
  }

  Widget _buildSectionTitle(BuildContext context, String title) {
    return Padding(
      padding: const EdgeInsets.only(left: 4, bottom: 6),
      child: Text(
        title,
        style: Theme.of(context).textTheme.titleSmall?.copyWith(
              fontWeight: FontWeight.bold,
              color: Colors.indigo.shade900,
            ),
      ),
    );
  }

  Widget _buildDetailRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
          ),
          Flexible(
            child: Text(
              value,
              textAlign: TextAlign.end,
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
