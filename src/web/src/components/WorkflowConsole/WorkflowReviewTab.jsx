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

export default function WorkflowReviewTab() {
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

    const filtered = useMemo(() => workflows.filter((item) => (filter === 'All' || statusLabels[item.status] === filter) && `${item.id} ${item.objective} ${item.agent}`.toLowerCase().includes(search.toLowerCase())), [filter, search, workflows]);

    const counts = {
        paused: workflows.filter((x) => x.status === 'PausedForApproval').length,
        running: workflows.filter((x) => x.status === 'Running').length,
        completed: workflows.filter((x) => x.status === 'Completed').length,
        terminated: workflows.filter((x) => x.risk === 'Terminated').length
    };

    const applyDecision = async () => {
        try {
            const response = await workflowApi.decide(selected.id, decision);
            const updatedStatus = response.updatedWorkflowStatus;
            setWorkflows((items) => items.map((item) => item.id === selected.id ? {
                ...item,
                status: updatedStatus
            } : item));
            setSelected((item) => ({ ...item, status: updatedStatus }));
            setNotice(`Decision recorded: ${decision.toLowerCase()}.`);
            setDecision(null);
        } catch (requestError) {
            setNotice(requestError.message);
        }
    };

    return (
        <div className="wc-container">
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
                        <p>Structured agent work, never raw model reasoning.</p>
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
                                placeholder="Search workflow, objective or agent"
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
                        <h3>No workflows found</h3>
                        <p>There are no workflows matching the current filters.</p>
                    </div>
                ) : (
                    <div className="wc-table-wrapper">
                        <table className="wc-table">
                            <thead>
                                <tr>
                                    <th>Workflow</th>
                                    <th>Status</th>
                                    <th>Risk</th>
                                    <th>Agent</th>
                                    <th>Created</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filtered.map((item) => (
                                    <tr key={item.id} onClick={async () => {
                                        try {
                                            setSelected(await workflowApi.detail(item.id));
                                        } catch (requestError) {
                                            setNotice(requestError.message);
                                        }
                                    }}>
                                        <td>
                                            <strong className="wc-table-id">WF-{item.id}</strong>
                                            <span className="wc-table-subtext">{item.objective}</span>
                                        </td>
                                        <td>
                                            <span className={`wc-badge ${statusClass(item.status)}`}>
                                                <i />{statusLabels[item.status] || item.status}
                                            </span>
                                        </td>
                                        <td>
                                            <span className={`wc-risk-text ${(item.risk || '').toLowerCase()}`}>
                                                {item.risk || 'Standard'}
                                            </span>
                                        </td>
                                        <td>{item.agent || 'Workflow Agent'}</td>
                                        <td>{formatDate(item.createdAt)}</td>
                                        <td>
                                            <button className="wc-btn-sm outline" aria-label={`Open workflow ${item.id}`}>→</button>
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
