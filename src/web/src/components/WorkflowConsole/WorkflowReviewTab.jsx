import { useEffect, useMemo, useState } from 'react';
import { workflowApi } from '../../api/channelCenterApi';
import WorkflowDetailPopup from './WorkflowDetailPopup';
import ApprovalConfirmationPopup from './ApprovalConfirmationPopup';
import { formatDate, statusClass, statusLabels } from './workflowConsoleUtils';
import './WorkflowConsole.css';

function Metric({ label, value, detail, tone }) {
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

export default function WorkflowReviewTab({ hideHeader = false }) {
    const [workflows, setWorkflows] = useState([]);
    const [selected, setSelected] = useState(null);
    const [filter, setFilter] = useState('All');
    const [search, setSearch] = useState('');
    const [decision, setDecision] = useState(null);
    const [notice, setNotice] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const loadWorkflows = () => {
        setLoading(true);
        setError('');
        workflowApi.list().then((data) => setWorkflows(data.map((item) => ({
            ...item,
            risk: item.risk || (item.requiresHumanApproval ? 'High' : 'Standard')
        })))).catch((requestError) => setError(requestError.message)).finally(() => setLoading(false));
    };

    useEffect(() => {
        Promise.resolve().then(loadWorkflows);
    }, []);

    // Group workflows by process key (e.g., appointmentId or root correlationId)
    const processes = useMemo(() => {
        const groups = {};

        workflows.forEach((item) => {
            let processKey = `wf-${item.id}`;
            if (item.appointmentId && item.appointmentId > 0) {
                processKey = `appt-${item.appointmentId}`;
            } else if (item.correlationId && item.correlationId.startsWith('appointment-')) {
                const parts = item.correlationId.split('-');
                if (parts[1]) processKey = `appt-${parts[1]}`;
            } else if (item.correlationId) {
                const rootCorr = item.correlationId.split('-')[0];
                if (rootCorr) processKey = `corr-${rootCorr}`;
            }

            if (!groups[processKey]) {
                groups[processKey] = {
                    processKey,
                    id: item.id,
                    objective: item.objective,
                    appointmentId: item.appointmentId,
                    items: [item],
                    status: item.status,
                    risk: item.risk || 'Standard',
                    requiresHumanApproval: item.requiresHumanApproval || false,
                    agentsSet: new Set([item.agent || 'Workflow Agent']),
                    createdAt: item.createdAt,
                };
            } else {
                const g = groups[processKey];
                g.items.push(item);
                if (item.agent) g.agentsSet.add(item.agent);

                const isReviewable = (s) => s === 'PausedForApproval' || s === 'SafeFailed' || s === 1 || s === 4;
                if (isReviewable(item.status) && !isReviewable(g.status)) {
                    g.id = item.id;
                    g.status = item.status;
                }

                const riskHierarchy = { emergency: 4, high: 3, standard: 2, low: 1 };
                const currentRiskVal = riskHierarchy[(g.risk || '').toLowerCase()] || 2;
                const itemRiskVal = riskHierarchy[(item.risk || '').toLowerCase()] || 2;
                if (itemRiskVal > currentRiskVal) {
                    g.risk = item.risk;
                }

                if (item.requiresHumanApproval) {
                    g.requiresHumanApproval = true;
                }

                if (new Date(item.createdAt) < new Date(g.createdAt)) {
                    g.createdAt = item.createdAt;
                }
            }
        });

        return Object.values(groups).map((g) => ({
            ...g,
            agentPipeline: Array.from(g.agentsSet).join(' → '),
            subCount: g.items.length
        }));
    }, [workflows]);

    const isReviewable = (s) => s === 'PausedForApproval' || s === 'SafeFailed' || s === 1 || s === 4;

    const filtered = useMemo(() => processes.filter((proc) => (filter === 'All' || statusLabels[proc.status] === filter) && `${proc.id} ${proc.objective} ${proc.agentPipeline}`.toLowerCase().includes(search.toLowerCase())), [filter, search, processes]);

    const counts = {
        paused: processes.filter((x) => isReviewable(x.status)).length,
        running: processes.filter((x) => x.status === 'Running' || x.status === 0).length,
        completed: processes.filter((x) => x.status === 'Completed' || x.status === 2).length,
        terminated: processes.filter((x) => x.status === 'Terminated' || x.status === 3).length,
        emergency: processes.filter((x) => (x.risk || '').toLowerCase() === 'emergency' || (x.risk || '').toLowerCase() === 'high' || x.requiresHumanApproval).length
    };

    const applyDecision = async () => {
        try {
            const targetIds = selected.subItemIds || [selected.id];
            const response = await workflowApi.decide(selected.id, decision);
            const updatedStatus = response.updatedWorkflowStatus;
            
            setWorkflows((items) => items.map((item) => targetIds.includes(item.id) || item.id === selected.id ? {
                ...item,
                status: updatedStatus,
                requiresHumanApproval: updatedStatus === 'PausedForApproval'
            } : item));
            
            setSelected((item) => ({ ...item, status: updatedStatus }));
            setNotice(`Decision recorded: ${decision.toLowerCase()} applied to whole process.`);
            setDecision(null);
        } catch (requestError) {
            setNotice(requestError.message);
        }
    };

    return (
        <div className={hideHeader ? 'wc-embedded' : 'wc-container'}>
            {!hideHeader && (
                <header className="wc-header">
                    <div className="wc-header-titles">
                        <p className="wc-eyebrow">Admin workspace / Connected API</p>
                        <h1>Workflow review</h1>
                        <p>Monitor Safety Auditor decisions before they reach the care team.</p>
                    </div>
                    <div className="wc-header-actions">
                        <button className="wc-btn outline" onClick={loadWorkflows}>↻ Refresh</button>
                    </div>
                </header>
            )}

            {notice && (
                <div className="wc-alert success" role="status">
                    <span>{notice}</span>
                    <button onClick={() => setNotice('')}>×</button>
                </div>
            )}

            <div className="wc-stats-grid">
                <Metric label="Needs review" value={counts.paused} detail="Awaiting admin decision" tone="danger" />
                <Metric label="Running" value={counts.running} detail="Active agent workflows" tone="blue" />
                <Metric label="Completed" value={counts.completed} detail="Successfully resolved" tone="success" />
                <Metric label="Emergency risk" value={counts.emergency} detail="Requires close attention" tone="amber" />
            </div>

            <section className="wc-card">
                <div className="wc-card-header">
                    <div>
                        <h2>Approval queue</h2>
                        <p>Structured agent work, consolidated by clinical process.</p>
                    </div>
                    <span className="wc-live-pill"><span className="wc-live-dot" /> LIVE</span>
                </div>

                <div className="wc-toolbar">
                    <div className="wc-toolbar-left">
                        <div className="wc-search-box">
                            <span className="wc-search-icon">⌕</span>
                            <input
                                type="text"
                                className="wc-search-input"
                                value={search}
                                onChange={(event) => setSearch(event.target.value)}
                                placeholder="Search process, objective or agent"
                            />
                        </div>
                        <select
                            className="wc-select"
                            value={filter}
                            onChange={(event) => setFilter(event.target.value)}
                            aria-label="Filter workflow status"
                        >
                            <option>All</option>
                            <option>Paused for review</option>
                            <option>Running</option>
                            <option>Completed</option>
                            <option>Safe-failed</option>
                        </select>
                    </div>
                </div>

                {loading ? (
                    <div className="wc-loading-box">
                        <div className="wc-spinner"></div>
                        <h3>Loading workflows…</h3>
                        <p>Fetching current workflow state from the API.</p>
                    </div>
                ) : error ? (
                    <div className="wc-empty-state">
                        <div className="icon">⚠️</div>
                        <h3>Unable to load workflows</h3>
                        <p>{error}</p>
                        <button className="wc-btn outline" onClick={loadWorkflows}>Retry</button>
                    </div>
                ) : filtered.length === 0 ? (
                    <div className="wc-empty-state">
                        <div className="icon">📋</div>
                        <h3>No processes found</h3>
                        <p>There are no processes matching the current filters.</p>
                    </div>
                ) : (
                    <div className="wc-table-wrapper">
                        <table className="wc-table">
                            <thead>
                                <tr>
                                    <th>Process / Workflow</th>
                                    <th>Status</th>
                                    <th>Risk</th>
                                    <th>Agent Pipeline</th>
                                    <th>Created</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filtered.map((proc) => (
                                    <tr key={proc.processKey} onClick={async () => {
                                        try {
                                            const detail = await workflowApi.detail(proc.id);
                                            setSelected({
                                                ...detail,
                                                subItemIds: proc.items.map((i) => i.id)
                                            });
                                        } catch (requestError) {
                                            setNotice(requestError.message);
                                        }
                                    }}>
                                        <td>
                                            <strong className="wc-table-id">PROCESS-{proc.id}</strong>
                                            <span className="wc-table-subtext">{proc.objective}</span>
                                        </td>
                                        <td>
                                            <span className={`wc-badge ${statusClass(proc.status)}`}>
                                                <i />{statusLabels[proc.status] || proc.status}
                                            </span>
                                        </td>
                                        <td>
                                            <span className={`wc-risk-text ${(proc.risk || '').toLowerCase()}`}>
                                                {proc.risk || 'Standard'}
                                            </span>
                                        </td>
                                        <td>{proc.agentPipeline || 'Workflow Agent'}</td>
                                        <td>{formatDate(proc.createdAt)}</td>
                                        <td>
                                            <button className="wc-btn-sm outline" aria-label={`Open process ${proc.id}`}>→</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>

            {selected && (
                <WorkflowDetailPopup workflow={selected} onClose={() => setSelected(null)} onDecision={setDecision} />
            )}
            {decision && (
                <ApprovalConfirmationPopup decision={decision} onCancel={() => setDecision(null)} onConfirm={applyDecision} />
            )}
        </div>
    );
}
