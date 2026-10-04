import { useState, useEffect } from 'react';
import WorkflowReviewTab from './WorkflowReviewTab';
import AuditTrailTab from './AuditTrailTab';
import { workflowApi } from '../../api/channelCenterApi';
import './WorkflowConsole.css';

function Metric({ label, value, detail, tone, icon }) {
  return (
    <div className="wc-stat-card">
      <div className="wc-stat-header">
        <span className="wc-stat-label">{label}</span>
        <span className={`wc-stat-icon ${tone}`}>
          {icon || (tone === 'danger' ? '!' : tone === 'success' ? '✓' : tone === 'blue' ? '◈' : '↗')}
        </span>
      </div>
      <div className="wc-stat-value">{value}</div>
      <div className="wc-stat-hint">{detail}</div>
    </div>
  );
}

export default function WorkflowGovernanceConsole() {
  const [activeSubTab, setActiveSubTab] = useState('review');
  const [overview, setOverview] = useState(null);
  const [metrics, setMetrics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchSafetyData = () => {
    setLoading(true);
    setError('');
    Promise.all([workflowApi.overview(), workflowApi.aiMetrics()])
      .then(([overviewData, metricsData]) => {
        setOverview(overviewData);
        setMetrics(metricsData);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchSafetyData();
  }, []);

  return (
    <div className="wc-container">
      {/* Safety Analytics Section (Top Header & Metrics) */}
      <header className="wc-header">
        <div className="wc-header-titles">
          <p className="wc-eyebrow">Governance & Safety Console</p>
          <h1>Workflow & Safety Governance</h1>
          <p>Real-time monitoring of AI Safety Auditor interventions, risk telemetry, and audit logs.</p>
        </div>
        <div className="wc-header-actions">
          <span className="wc-live-pill"><span className="wc-live-dot" /> SAFETY AUDITOR ACTIVE</span>
          <button className="wc-btn outline" onClick={fetchSafetyData} disabled={loading}>
            {loading ? 'Refreshing…' : '↻ Refresh Metrics'}
          </button>
        </div>
      </header>

      {/* Safety Analytics Metrics Grid */}
      {error ? (
        <div className="wc-alert error" role="alert">
          <span>⚠️ Safety Analytics offline: {error}</span>
          <button onClick={fetchSafetyData}>Retry</button>
        </div>
      ) : loading ? (
        <div className="wc-loading-box" style={{ padding: '24px 0' }}>
          <div className="wc-spinner"></div>
          <h3>Loading Safety Analytics…</h3>
        </div>
      ) : (
        <div className="wc-stats-grid">
          <Metric
            label="Intervention Rate"
            value={`${metrics?.humanInterventionRate ?? 0}%`}
            detail="Human review trigger frequency"
            tone="blue"
            icon="🛡️"
          />
          <Metric
            label="System Approval Rate"
            value={`${overview?.systemApprovalRate ?? 0}%`}
            detail="Across reviewed workflows"
            tone="success"
            icon="✓"
          />
          <Metric
            label="Revisions Requested"
            value={metrics?.revisedCount ?? 0}
            detail="Returned for clarification"
            tone="amber"
            icon="🔄"
          />
          <Metric
            label="Safe Failures"
            value={overview?.emergencyCasesCount ?? 0}
            detail="Emergency fallback triggers"
            tone="danger"
            icon="🚨"
          />
          <Metric
            label="Active Workflows"
            value={overview?.activeWorkflowsCount ?? 0}
            detail="Currently executing agents"
            tone="blue"
            icon="⚡"
          />
          <Metric
            label="Paused Workflows"
            value={overview?.pausedWorkflowsCount ?? 0}
            detail="Awaiting admin approval"
            tone="amber"
            icon="⏸️"
          />
          <Metric
            label="Completed Workflows"
            value={overview?.completedWorkflowsCount ?? 0}
            detail="Successfully executed"
            tone="success"
            icon="✔️"
          />
          <Metric
            label="Total Reviewed"
            value={metrics?.totalWorkflows ?? 0}
            detail="Persisted workflow count"
            tone="blue"
            icon="📋"
          />
        </div>
      )}

      {/* Sub-Navigation Bar Below Safety Analytics */}
      <div className="wc-nav-tabs" role="tablist" aria-label="Governance sub-tabs">
        <button
          role="tab"
          aria-selected={activeSubTab === 'review'}
          className={`wc-tab-btn ${activeSubTab === 'review' ? 'active' : ''}`}
          onClick={() => setActiveSubTab('review')}
        >
          <span>🛡️</span> Workflow Review Queue
        </button>
        <button
          role="tab"
          aria-selected={activeSubTab === 'audit'}
          className={`wc-tab-btn ${activeSubTab === 'audit' ? 'active' : ''}`}
          onClick={() => setActiveSubTab('audit')}
        >
          <span>📋</span> System Audit Trail
        </button>
      </div>

      {/* Sub-Tab Panel */}
      <div className="wc-subtab-panel">
        {activeSubTab === 'review' && <WorkflowReviewTab hideHeader={true} />}
        {activeSubTab === 'audit' && <AuditTrailTab hideHeader={true} />}
      </div>
    </div>
  );
}
