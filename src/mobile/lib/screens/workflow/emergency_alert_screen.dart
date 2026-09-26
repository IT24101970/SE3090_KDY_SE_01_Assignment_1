import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../providers/auth_provider.dart';
import '../../providers/workflow_provider.dart';
import '../../models/workflow_models.dart';
import '../../widgets/emergency_alert_card.dart';
import 'workflow_status_detail_screen.dart';

class EmergencyAlertScreen extends StatefulWidget {
  const EmergencyAlertScreen({Key? key}) : super(key: key);

  @override
  State<EmergencyAlertScreen> createState() => _EmergencyAlertScreenState();
}

class _EmergencyAlertScreenState extends State<EmergencyAlertScreen> {
  String _selectedFilter = 'All';

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final auth = Provider.of<AuthProvider>(context, listen: false);
      final wf = Provider.of<WorkflowProvider>(context, listen: false);
      wf.fetchAlerts(auth.token);
      wf.startAutoPolling(auth.token);
    });
  }

  @override
  void dispose() {
    // Note: Provider cleanup handled by provider dispose
    super.dispose();
  }

  Future<void> _handleRefresh() async {
    final auth = Provider.of<AuthProvider>(context, listen: false);
    final wf = Provider.of<WorkflowProvider>(context, listen: false);
    await wf.refreshAlerts(auth.token);
  }

  List<EmergencyAlert> _filterAlerts(List<EmergencyAlert> alerts) {
    if (_selectedFilter == 'Emergency') {
      return alerts.where((a) => a.isEmergency).toList();
    } else if (_selectedFilter == 'High/Emergency') {
      return alerts.where((a) => a.isHighRisk).toList();
    } else if (_selectedFilter == 'Paused') {
      return alerts.where((a) => a.status == WorkflowStatus.pausedForApproval).toList();
    } else if (_selectedFilter == 'Approved') {
      return alerts.where((a) => a.status == WorkflowStatus.approved || a.status == WorkflowStatus.completed).toList();
    }
    return alerts;
  }

  @override
  Widget build(BuildContext context) {
    final wf = Provider.of<WorkflowProvider>(context);
    final filteredAlerts = _filterAlerts(wf.alerts);
    final activeEmergencies = wf.activeEmergencyAlerts;

    return Scaffold(
      appBar: AppBar(
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.amber),
            SizedBox(width: 8),
            Text('Safety Alerts & Workflows'),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh',
            onPressed: wf.isLoading ? null : _handleRefresh,
          ),
        ],
      ),
      body: Column(
        children: [
          // Live status polling indicator header bar
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            color: Colors.indigo.shade50,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: const BoxDecoration(
                        color: Colors.green,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      wf.isPollingActive
                          ? 'Live Polling Active (Auto-refresh 10s)'
                          : 'Live Monitoring Off',
                      style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        color: Colors.indigo.shade900,
                      ),
                    ),
                  ],
                ),
                if (wf.lastRefreshedAt != null)
                  Text(
                    'Refreshed: ${DateFormat('HH:mm:ss').format(wf.lastRefreshedAt!)}',
                    style: TextStyle(fontSize: 11, color: Colors.grey.shade700),
                  ),
              ],
            ),
          ),

          // In-App Alert Notification Toast Banner (if status changed)
          if (wf.notificationMessage != null) ...[
            MaterialBanner(
              backgroundColor: Colors.amber.shade100,
              leading: const Icon(Icons.notification_important, color: Colors.deepOrange),
              content: Text(
                wf.notificationMessage!,
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
              ),
              actions: [
                TextButton(
                  onPressed: () => wf.clearNotification(),
                  child: const Text('DISMISS'),
                ),
              ],
            ),
          ],

          // Top Emergency Banner if active high/emergency risks exist
          if (activeEmergencies.isNotEmpty) ...[
            Container(
              width: double.infinity,
              margin: const EdgeInsets.all(12),
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFFB71C1C), Color(0xFFD32F2F)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.red.withValues(alpha: 0.3),
                    blurRadius: 8,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: Row(
                children: [
                  const Icon(Icons.emergency, color: Colors.white, size: 36),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${activeEmergencies.length} ACTIVE SAFETY ALERT(S)',
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 14,
                            letterSpacing: 0.5,
                          ),
                        ),
                        const SizedBox(height: 2),
                        const Text(
                          'Operational users must monitor safety workflow outcomes.',
                          style: TextStyle(color: Colors.white70, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],

          // Category Filter Chips
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
            child: Row(
              children: [
                _buildFilterChip('All'),
                const SizedBox(width: 8),
                _buildFilterChip('High/Emergency'),
                const SizedBox(width: 8),
                _buildFilterChip('Paused'),
                const SizedBox(width: 8),
                _buildFilterChip('Approved'),
              ],
            ),
          ),
          const Divider(height: 1),

          // Alert List Body
          Expanded(
            child: wf.isLoading && wf.alerts.isEmpty
                ? const Center(child: CircularProgressIndicator())
                : wf.errorMessage != null && wf.alerts.isEmpty
                    ? Center(
                        child: Padding(
                          padding: const EdgeInsets.all(24.0),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              const Icon(Icons.cloud_off, size: 48, color: Colors.grey),
                              const SizedBox(height: 12),
                              Text(
                                wf.errorMessage!,
                                textAlign: TextAlign.center,
                                style: const TextStyle(color: Colors.grey),
                              ),
                              const SizedBox(height: 16),
                              ElevatedButton.icon(
                                onPressed: _handleRefresh,
                                icon: const Icon(Icons.refresh),
                                label: const Text('Try Again'),
                              ),
                            ],
                          ),
                        ),
                      )
                    : filteredAlerts.isEmpty
                        ? Center(
                            child: Padding(
                              padding: const EdgeInsets.all(24.0),
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Icon(Icons.shield_outlined,
                                      size: 56, color: Colors.teal.shade300),
                                  const SizedBox(height: 12),
                                  const Text(
                                    'No Safety Alerts Found',
                                    style: TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 16,
                                    ),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    _selectedFilter == 'All'
                                        ? 'There are currently no active safety alerts or emergency workflows.'
                                        : 'No workflows match the "$_selectedFilter" filter.',
                                    textAlign: TextAlign.center,
                                    style: const TextStyle(color: Colors.grey, fontSize: 13),
                                  ),
                                ],
                              ),
                            ),
                          )
                        : RefreshIndicator(
                            onRefresh: _handleRefresh,
                            child: ListView.builder(
                              itemCount: filteredAlerts.length,
                              itemBuilder: (context, index) {
                                final alert = filteredAlerts[index];
                                return EmergencyAlertCard(
                                  alert: alert,
                                  onTap: () {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) => WorkflowStatusDetailScreen(
                                          workflowId: alert.id,
                                        ),
                                      ),
                                    );
                                  },
                                );
                              },
                            ),
                          ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChip(String label) {
    final isSelected = _selectedFilter == label;
    return FilterChip(
      selected: isSelected,
      label: Text(label),
      selectedColor: Colors.indigo.shade100,
      checkmarkColor: Colors.indigo,
      labelStyle: TextStyle(
        color: isSelected ? Colors.indigo.shade900 : Colors.black87,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
        fontSize: 12,
      ),
      onSelected: (selected) {
        if (selected) {
          setState(() {
            _selectedFilter = label;
          });
        }
      },
    );
  }
}
