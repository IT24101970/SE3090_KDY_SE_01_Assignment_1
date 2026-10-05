import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../providers/auth_provider.dart';
import '../../providers/workflow_provider.dart';
import '../../widgets/notification_bell_widget.dart';

class AdminSystemOverviewScreen extends StatefulWidget {
  const AdminSystemOverviewScreen({super.key});

  @override
  State<AdminSystemOverviewScreen> createState() => _AdminSystemOverviewScreenState();
}

class _AdminSystemOverviewScreenState extends State<AdminSystemOverviewScreen> {
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _refreshData();
  }

  Future<void> _refreshData() async {
    setState(() => _isLoading = true);
    final workflowProvider = Provider.of<WorkflowProvider>(context, listen: false);
    await workflowProvider.fetchWorkflows();
    if (mounted) {
      setState(() => _isLoading = false);
    }
  }

  void _confirmLogout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Admin Portal Logout'),
        content: const Text('Are you sure you want to log out of the Admin panel?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              Provider.of<AuthProvider>(context, listen: false).logout();
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Log Out', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthProvider>(context);
    final user = auth.currentUser;
    final workflowProvider = Provider.of<WorkflowProvider>(context);

    final workflows = workflowProvider.workflows;
    final pausedWorkflows = workflows.where((w) {
      final st = w.status.name.toLowerCase();
      return st == 'pausedforapproval' || st == 'safefailed' || st == '1' || st == '6';
    }).toList();

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      appBar: AppBar(
        backgroundColor: const Color(0xFF1E293B),
        foregroundColor: Colors.white,
        elevation: 0,
        title: Row(
          children: const [
            Icon(Icons.shield_outlined, color: Colors.cyanAccent),
            SizedBox(width: 8),
            Text('System Overview', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Metrics',
            onPressed: _refreshData,
          ),
          const NotificationBellWidget(),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.redAccent),
            tooltip: 'Logout',
            onPressed: _confirmLogout,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _refreshData,
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Admin Header Card
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18.0),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [Color(0xFF1E293B), Color(0xFF0F172A)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: Colors.cyan.withValues(alpha: 0.3)),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      radius: 26,
                      backgroundColor: Colors.cyan.withValues(alpha: 0.2),
                      child: const Icon(Icons.admin_panel_settings, color: Colors.cyanAccent, size: 30),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            user?.fullName ?? 'Administrator',
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white),
                          ),
                          const SizedBox(height: 2),
                          const Text(
                            'System Administrator • Read-Only Overview',
                            style: TextStyle(fontSize: 12, color: Colors.white70),
                          ),
                          const SizedBox(height: 6),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                            decoration: BoxDecoration(
                              color: const Color(0xFF10B981).withValues(alpha: 0.2),
                              borderRadius: BorderRadius.circular(8),
                              border: Border.all(color: const Color(0xFF10B981)),
                            ),
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: const [
                                Icon(Icons.circle, color: Color(0xFF34D399), size: 8),
                                SizedBox(width: 6),
                                Text('System Status: Operational', style: TextStyle(color: Color(0xFF34D399), fontSize: 11, fontWeight: FontWeight.bold)),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 20),

              // Overview Metrics Grid
              Row(
                children: [
                  Expanded(
                    child: _buildMetricCard(
                      title: 'Active Workflows',
                      value: '${workflows.length}',
                      icon: Icons.alt_route,
                      color: Colors.blueAccent,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: _buildMetricCard(
                      title: 'Safety Alerts',
                      value: '${pausedWorkflows.length}',
                      icon: Icons.warning_amber_rounded,
                      color: pausedWorkflows.isNotEmpty ? Colors.amberAccent : Colors.greenAccent,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: _buildMetricCard(
                      title: 'AI Agents',
                      value: '4 Online',
                      icon: Icons.memory,
                      color: Colors.cyanAccent,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: _buildMetricCard(
                      title: 'System Health',
                      value: '100%',
                      icon: Icons.check_circle_outline,
                      color: const Color(0xFF34D399),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 24),

              // AI Agent Network Overview
              const Text(
                'AI Agent Network Status',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
              ),
              const SizedBox(height: 12),
              _buildAgentStatusTile(
                name: 'Intake AI Agent',
                role: 'Symptom extraction & initial triage scoring',
                status: 'Active',
                icon: Icons.psychology,
              ),
              const SizedBox(height: 8),
              _buildAgentStatusTile(
                name: 'Doctor Scheduling AI Agent',
                role: 'Specialty matching & slot allocation',
                status: 'Active',
                icon: Icons.calendar_month,
              ),
              const SizedBox(height: 8),
              _buildAgentStatusTile(
                name: 'Safety Auditor Agent',
                role: 'High-risk interaction screening & pause triggers',
                status: 'Monitoring',
                icon: Icons.security,
              ),
              const SizedBox(height: 8),
              _buildAgentStatusTile(
                name: 'Notification Agent',
                role: 'Role-targeted event broadcasting',
                status: 'Active',
                icon: Icons.notifications_active,
              ),

              const SizedBox(height: 24),

              // High-Risk Safety Alerts Monitor
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'High-Risk Safety Flagged Workflows',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(
                      color: Colors.amber.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      '${pausedWorkflows.length} Paused',
                      style: const TextStyle(color: Colors.amberAccent, fontSize: 12, fontWeight: FontWeight.bold),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),

              if (_isLoading)
                const Center(child: CircularProgressIndicator(color: Colors.cyan))
              else if (pausedWorkflows.isEmpty)
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: const Color(0xFF1E293B),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.white12),
                  ),
                  child: Row(
                    children: const [
                      Icon(Icons.check_circle, color: Color(0xFF34D399)),
                      SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          'No high-risk appointments currently paused by Safety Auditor.',
                          style: TextStyle(color: Colors.white70, fontSize: 13),
                        ),
                      ),
                    ],
                  ),
                )
              else
                ListView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: pausedWorkflows.length,
                  itemBuilder: (context, index) {
                    final item = pausedWorkflows[index];
                    return Card(
                      color: const Color(0xFF1E293B),
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        leading: const CircleAvatar(
                          backgroundColor: Colors.amber,
                          child: Icon(Icons.warning, color: Colors.black, size: 20),
                        ),
                        title: Text(
                          'Workflow ID: ${item.id} - ${item.patientName}',
                          style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 14),
                        ),
                        subtitle: Text(
                          'Status: ${item.status} • Risk Score: ${item.riskScore}',
                          style: const TextStyle(color: Colors.white70, fontSize: 12),
                        ),
                        trailing: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                          decoration: BoxDecoration(
                            color: Colors.red.withValues(alpha: 0.2),
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.redAccent),
                          ),
                          child: const Text('Paused', style: TextStyle(color: Colors.redAccent, fontSize: 11, fontWeight: FontWeight.bold)),
                        ),
                      ),
                    );
                  },
                ),

              const SizedBox(height: 24),

              // Web Portal Info Card
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.blue.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.blue.withValues(alpha: 0.3)),
                ),
                child: Row(
                  children: const [
                    Icon(Icons.info_outline, color: Colors.blueAccent),
                    SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        'Full administrative operations (approvals, schedule creation, audit logs) are performed on the Web Interface.',
                        style: TextStyle(color: Colors.white70, fontSize: 12),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildMetricCard({
    required String title,
    required String value,
    required IconData icon,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFF1E293B),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(title, style: const TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.w600)),
              Icon(icon, color: color, size: 20),
            ],
          ),
          const SizedBox(height: 10),
          Text(
            value,
            style: TextStyle(color: color, fontSize: 22, fontWeight: FontWeight.bold),
          ),
        ],
      ),
    );
  }

  Widget _buildAgentStatusTile({
    required String name,
    required String role,
    required String status,
    required IconData icon,
  }) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: const Color(0xFF1E293B),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.white12),
      ),
      child: Row(
        children: [
          Icon(icon, color: Colors.cyan, size: 24),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(name, style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 14)),
                const SizedBox(height: 2),
                Text(role, style: const TextStyle(color: Colors.white60, fontSize: 11)),
              ],
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
            decoration: BoxDecoration(
              color: Colors.cyan.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              status,
              style: const TextStyle(color: Colors.cyanAccent, fontSize: 11, fontWeight: FontWeight.bold),
            ),
          ),
        ],
      ),
    );
  }
}
