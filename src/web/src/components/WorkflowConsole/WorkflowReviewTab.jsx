import {useEffect, useMemo, useState} from 'react';
import {workflowApi} from '../../api/channelCenterApi';
import WorkflowDetailPopup from './WorkflowDetailPopup';
import ApprovalConfirmationPopup from './ApprovalConfirmationPopup';
import {formatDate, statusClass, statusLabels} from './workflowConsoleUtils';
import './WorkflowConsole.css';

function Metric({label, value, detail, tone}) {
    return <div className="metric-card"><span
        className={`metric-icon ${tone}`}>{tone === 'danger' ? '!' : tone === 'success' ? '✓' : tone === 'blue' ? '◈' : '↗'}</span>
        <div><span className="metric-label">{label}</span><strong>{value}</strong><small>{detail}</small></div>
    </div>;
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
        emergency: workflows.filter((x) => x.risk === 'Emergency').length
    };

    const applyDecision = async () => {
        try {
            const response = await workflowApi.decide(selected.id, decision);
            const updatedStatus = response.updatedWorkflowStatus;
            setWorkflows((items) => items.map((item) => item.id === selected.id ? {
                ...item,
                status: updatedStatus
            } : item));
            setSelected((item) => ({...item, status: updatedStatus}));
            setNotice(`Decision recorded: ${decision.toLowerCase()}.`);
            setDecision(null);
        } catch (requestError) {
            setNotice(requestError.message);
        }
    };

    return <div className="workflow-page">
        <header className="page-header">
            <div><p className="eyebrow">Admin workspace / Connected API</p><h1>Workflow review</h1><p>Monitor Safety
                Auditor decisions before they reach the care team.</p></div>
            <button className="outline-button" onClick={loadWorkflows}>↻ Refresh</button>
        </header>
        {notice && <div className="toast" role="status">{notice}
            <button onClick={() => setNotice('')}>×</button>
        </div>}
        <div className="metric-grid"><Metric label="Needs review" value={counts.paused} detail="Awaiting admin decision"
                                             tone="danger"/><Metric label="Running" value={counts.running}
                                                                    detail="Active agent workflows" tone="blue"/><Metric
            label="Completed" value={counts.completed} detail="Successfully resolved" tone="success"/><Metric
            label="Emergency risk" value={counts.emergency} detail="Requires close attention" tone="amber"/></div>
        <section className="panel queue-panel">
            <div className="panel-heading">
                <div><h2>Approval queue</h2><p>Structured agent work, never raw model reasoning.</p></div>
                <span className="live-pill"><span/> LIVE</span></div>
            <div className="queue-toolbar"><label className="search-box"><span>⌕</span><input value={search}
                                                                                              onChange={(event) => setSearch(event.target.value)}
                                                                                              placeholder="Search workflow, objective or agent"/></label><select
                value={filter} onChange={(event) => setFilter(event.target.value)} aria-label="Filter workflow status">
                <option>All</option>
                <option>Paused for review</option>
                <option>Running</option>
                <option>Completed</option>
                <option>Safe-failed</option>
            </select></div>
            {loading ?
                <div className="empty-state"><strong>Loading workflows…</strong><span>Fetching current workflow state from the API.</span>
                </div> : error ? <div className="empty-state error-state"><strong>Unable to load
                    workflows</strong><span>{error}</span>
                    <button className="outline-button" onClick={loadWorkflows}>Retry</button>
                </div> : filtered.length === 0 ?
                    <div className="empty-state"><strong>No workflows found</strong><span>There are no workflows matching the current filters.</span>
                    </div> : <div className="workflow-table-wrap">
                        <table className="workflow-table">
                            <thead>
                            <tr>
                                <th>Workflow</th>
                                <th>Status</th>
                                <th>Risk</th>
                                <th>Agent</th>
                                <th>Created</th>
                                <th/>
                            </tr>
                            </thead>
                            <tbody>{filtered.map((item) => <tr key={item.id} onClick={async () => {
                                try {
                                    setSelected(await workflowApi.detail(item.id));
                                } catch (requestError) {
                                    setNotice(requestError.message);
                                }
                            }}>
                                <td><strong>WF-{item.id}</strong><span>{item.objective}</span></td>
                                <td><span
                                    className={`status-badge ${statusClass(item.status)}`}><i/>{statusLabels[item.status] || item.status}</span>
                                </td>
                                <td><span
                                    className={`risk-text ${(item.risk || '').toLowerCase()}`}>{item.risk || 'Standard'}</span>
                                </td>
                                <td>{item.agent || 'Workflow Agent'}</td>
                                <td>{formatDate(item.createdAt)}</td>
                                <td>
                                    <button className="row-action" aria-label={`Open workflow ${item.id}`}>→</button>
                                </td>
                            </tr>)}</tbody>
                        </table>
                    </div>}
        </section>
        {selected &&
            <WorkflowDetailPopup workflow={selected} onClose={() => setSelected(null)} onDecision={setDecision}/>}
        {decision && <ApprovalConfirmationPopup decision={decision} onCancel={() => setDecision(null)}
                                                onConfirm={applyDecision}/>}
    </div>;
}
