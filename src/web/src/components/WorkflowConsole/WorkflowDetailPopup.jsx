import {formatDate, statusClass, statusLabels} from './workflowConsoleUtils';

export default function WorkflowDetailPopup({workflow, onClose, onDecision}) {
    const logs = workflow.auditLogs || [];

    return (
        <div className="drawer-backdrop">
            <aside className="detail-drawer" aria-label="Workflow details">
                <div className="drawer-top">
                    <button className="back-button" onClick={onClose}>← Queue</button>
                    <button className="modal-close" onClick={onClose}>×</button>
                </div>
                <div className="drawer-title"><span
                    className={`status-badge ${statusClass(workflow.status)}`}><i/>{statusLabels[workflow.status] || workflow.status}</span>
                    <h2>WF-{workflow.id}</h2><p>{workflow.objective}</p></div>
                {workflow.status === 'PausedForApproval' && <div className="review-alert"><strong>Human review
                    required</strong><span>{workflow.pauseReason || 'The Safety Auditor paused this workflow for an accountable admin decision.'}</span>
                </div>}
                <div className="detail-section"><h3>Execution summary</h3>
                    <div className="detail-grid">
                        <div><small>Risk level</small><strong
                            className={`risk-text ${(workflow.risk || '').toLowerCase()}`}>{workflow.risk || 'Not classified'}</strong>
                        </div>
                        <div><small>Created</small><strong>{formatDate(workflow.createdAt)}</strong></div>
                        <div><small>Approval
                            gate</small><strong>{workflow.requiresHumanApproval ? 'Required' : 'Not required'}</strong>
                        </div>
                        <div><small>Source</small><strong>Structured evidence</strong></div>
                    </div>
                </div>
                <div className="detail-section"><h3>Agent timeline <span>{logs.length} events</span>
                </h3>{logs.length === 0 ?
                    <div className="empty-state"><span>No audit evidence has been recorded.</span></div> :
                    <div className="timeline">{logs.map((log) => <div className="timeline-item" key={log.id}><span
                        className="timeline-dot"/>
                        <div>
                            <div className="timeline-meta"><strong>{log.agentName}</strong>
                                <time>{formatDate(log.createdAt)}</time>
                            </div>
                            <strong className="tool-name">{log.toolCalled}</strong><p>{log.toolOutput}</p></div>
                    </div>)}</div>}</div>
                {workflow.status === 'PausedForApproval' && <div className="drawer-actions">
                    <button className="outline-button revise" onClick={() => onDecision('Revised')}>Request revision
                    </button>
                    <button className="danger-button" onClick={() => onDecision('Rejected')}>Reject</button>
                    <button className="primary-button" onClick={() => onDecision('Approved')}>Approve workflow</button>
                </div>}
            </aside>
        </div>
    );
}
