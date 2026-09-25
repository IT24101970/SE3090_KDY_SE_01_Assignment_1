import {useEffect, useState} from 'react';
import {workflowApi} from '../../api/channelCenterApi';
import './WorkflowConsole.css';

function Metric({label, value, detail, tone}) {
    return (
        <div className="wc-stat-card">
            <div className="wc-stat-header">
                <span className="wc-stat-label">{label}</span>
                <span className={`wc-stat-icon ${tone}`}>
                    {tone === 'danger' ? '!' : tone === 'success' ? '✓' : tone === 'blue' ? '◈' : '↗'}
                </span>
            </div>
            <div className="wc-stat-value">{value}</div>
            <div className="wc-stat-hint">{detail}</div>
        </div>
    );
}

export default function SafetyAnalyticsTab() {
    const [overview, setOverview] = useState(null);
    const [metrics, setMetrics] = useState(null);
    const [error, setError] = useState('');

    useEffect(() => {
        Promise.all([workflowApi.overview(), workflowApi.aiMetrics()])
            .then(([overviewData, metricsData]) => {
                setOverview(overviewData);
                setMetrics(metricsData);
            })
            .catch((requestError) => setError(requestError.message));
    }, []);

    if (error) {
        return (
            <div className="wc-container">
                <div className="wc-empty-state">
                    <div className="icon">⚠️</div>
                    <h3>Unable to load analytics</h3>
                    <p>{error}</p>
                </div>
            </div>
        );
    }

    if (!overview || !metrics) {
        return (
            <div className="wc-container">
                <div className="wc-loading-box">
                    <div className="wc-spinner"></div>
                    <h3>Loading analytics…</h3>
                    <p>Fetching current API metrics.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="wc-container">
            <header className="wc-header">
                <div className="wc-header-titles">
                    <p className="wc-eyebrow">Governance / Performance</p>
                    <h1>Safety analytics</h1>
                    <p>Understand where human oversight is protecting the workflow.</p>
                </div>
            </header>

            <div className="wc-stats-grid">
                <Metric
                    label="Intervention rate"
                    value={`${metrics.humanInterventionRate}%`}
                    detail="Human review activity"
                    tone="blue"
                />
                <Metric
                    label="Approval rate"
                    value={`${overview.systemApprovalRate}%`}
                    detail="Across reviewed workflows"
                    tone="success"
                />
                <Metric
                    label="Revisions"
                    value={metrics.revisedCount}
                    detail="Returned to agents"
                    tone="amber"
                />
                <Metric
                    label="Safe failures"
                    value={overview.emergencyCasesCount}
                    detail="Emergency cases"
                    tone="danger"
                />
            </div>

            <section className="wc-card">
                <div className="wc-card-header">
                    <div>
                        <h2>Workflow totals</h2>
                        <p>Current persisted workflow counts</p>
                    </div>
                </div>
                <div className="wc-modal-body">
                    <div className="wc-form-grid">
                        <div className="wc-stat-card">
                            <span className="wc-stat-label">Active Workflows</span>
                            <div className="wc-stat-value">{overview.activeWorkflowsCount}</div>
                        </div>
                        <div className="wc-stat-card">
                            <span className="wc-stat-label">Paused Workflows</span>
                            <div className="wc-stat-value">{overview.pausedWorkflowsCount}</div>
                        </div>
                        <div className="wc-stat-card">
                            <span className="wc-stat-label">Completed Workflows</span>
                            <div className="wc-stat-value">{overview.completedWorkflowsCount}</div>
                        </div>
                        <div className="wc-stat-card">
                            <span className="wc-stat-label">Total Reviewed</span>
                            <div className="wc-stat-value">{metrics.totalWorkflows}</div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    );
}
