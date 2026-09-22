import {useEffect, useState} from 'react';
import {workflowApi} from '../../api/channelCenterApi';

function Metric({label, value, detail, tone}) {
    return <div className="metric-card"><span
        className={`metric-icon ${tone}`}>{tone === 'danger' ? '!' : tone === 'success' ? '✓' : tone === 'blue' ? '◈' : '↗'}</span>
        <div><span className="metric-label">{label}</span><strong>{value}</strong><small>{detail}</small></div>
    </div>;
}

export default function SafetyAnalyticsTab() {
    const [overview, setOverview] = useState(null);
    const [metrics, setMetrics] = useState(null);
    const [error, setError] = useState('');
    useEffect(() => {
        Promise.all([workflowApi.overview(), workflowApi.aiMetrics()]).then(([overviewData, metricsData]) => {
            setOverview(overviewData);
            setMetrics(metricsData);
        }).catch((requestError) => setError(requestError.message));
    }, []);
    if (error) return <div className="workflow-page">
        <div className="empty-state error-state"><strong>Unable to load analytics</strong><span>{error}</span></div>
    </div>;
    if (!overview || !metrics) return <div className="workflow-page">
        <div className="empty-state"><strong>Loading analytics…</strong><span>Fetching current API metrics.</span></div>
    </div>;
    return <div className="workflow-page">
        <header className="page-header">
            <div><p className="eyebrow">Governance / Performance</p><h1>Safety analytics</h1><p>Understand where human
                oversight is protecting the workflow.</p></div>
        </header>
        <div className="metric-grid"><Metric label="Intervention rate" value={`${metrics.humanInterventionRate}%`}
                                             detail="Human review activity" tone="blue"/><Metric label="Approval rate"
                                                                                                 value={`${overview.systemApprovalRate}%`}
                                                                                                 detail="Across reviewed workflows"
                                                                                                 tone="success"/><Metric
            label="Revisions" value={metrics.revisedCount} detail="Returned to agents" tone="amber"/><Metric
            label="Safe failures" value={overview.emergencyCasesCount} detail="Emergency cases" tone="danger"/></div>
        <section className="panel chart-panel">
            <div className="panel-heading">
                <div><h2>Workflow totals</h2><p>Current persisted workflow counts</p></div>
            </div>
            <div className="detail-grid analytics-grid">
                <div><small>Active</small><strong>{overview.activeWorkflowsCount}</strong></div>
                <div><small>Paused</small><strong>{overview.pausedWorkflowsCount}</strong></div>
                <div><small>Completed</small><strong>{overview.completedWorkflowsCount}</strong></div>
                <div><small>Total reviewed</small><strong>{metrics.totalWorkflows}</strong></div>
            </div>
        </section>
    </div>;
}
