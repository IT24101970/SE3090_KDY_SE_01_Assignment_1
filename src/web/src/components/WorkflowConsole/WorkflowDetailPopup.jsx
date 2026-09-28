import {formatDate, statusClass, statusLabels} from './workflowConsoleUtils';
import StructuredPayloadViewer from './StructuredPayloadViewer';
import './WorkflowConsole.css';

export default function WorkflowDetailPopup({workflow, onClose, onDecision}) {
    const logs = workflow.auditLogs || [];
    const canReview = workflow.status === 'PausedForApproval' || workflow.status === 'SafeFailed' || workflow.status === 1 || workflow.status === 4;

    return (
        <div className="wc-modal-backdrop" onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
            <div className="wc-modal-card wc-detail-modal" role="dialog" aria-modal="true" aria-label="Workflow details">
                <div className="wc-modal-header">
                    <div>
                        <span className={`wc-badge ${statusClass(workflow.status)}`}>
                            <i />{statusLabels[workflow.status] || workflow.status}
                        </span>
                        <h2>WF-{workflow.id}</h2>
                        <p>{workflow.objective}</p>
                    </div>
                    <button className="wc-modal-close" onClick={onClose}>×</button>
                </div>

                <div className="wc-modal-body">
                    {canReview && (
                        <div className="wc-alert warning">
                            <div>
                                <strong>Human review required</strong>
                                <div style={{ marginTop: '4px' }}>
                                    {workflow.pauseReason || workflow.errorMessage || 'This workflow requires an accountable admin decision to confirm or override.'}
                                </div>
                            </div>
                        </div>
                    )}

                    <div className="wc-detail-section">
                        <h3>Execution summary</h3>
                        <div className="wc-form-grid">
                            <div>
                                <span className="wc-label">Risk level</span>
                                <div>
                                    <strong className={`wc-risk-text ${(workflow.risk || '').toLowerCase()}`}>
                                        {workflow.risk || 'Not classified'}
                                    </strong>
                                </div>
                            </div>
                            <div>
                                <span className="wc-label">Created</span>
                                <div><strong>{formatDate(workflow.createdAt)}</strong></div>
                            </div>
                            <div>
                                <span className="wc-label">Approval gate</span>
                                <div><strong>{workflow.requiresHumanApproval || canReview ? 'Required' : 'Not required'}</strong></div>
                            </div>
                            <div>
                                <span className="wc-label">Source</span>
                                <div><strong>Structured evidence</strong></div>
                            </div>
                        </div>
                    </div>

                    <div className="wc-detail-section" style={{ marginTop: '20px' }}>
                        <h3>
                            Agent timeline{' '}
                            <span className="wc-badge low" style={{ textTransform: 'none', marginLeft: '6px' }}>
                                {logs.length} events
                            </span>
                        </h3>
                        {logs.length === 0 ? (
                            <div className="wc-empty-state">
                                <p>No audit evidence has been recorded.</p>
                            </div>
                        ) : (
                            <div className="wc-timeline">
                                {logs.map((log) => (
                                    <div className="wc-timeline-item" key={log.id}>
                                        <span className="wc-timeline-dot" />
                                        <div className="wc-timeline-content">
                                            <div className="wc-timeline-meta">
                                                <strong>{log.agentName}</strong>
                                                <time>{formatDate(log.createdAt)}</time>
                                            </div>
                                            <strong className="wc-tool-name">{log.toolCalled}</strong>
                                            <StructuredPayloadViewer payload={log.toolOutput} title={log.toolCalled} />
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                {canReview && (
                    <div className="wc-modal-footer">
                        <button className="wc-btn warning-border" onClick={() => onDecision('Revised')}>
                            Request revision
                        </button>
                        <button className="wc-btn danger-filled" onClick={() => onDecision('Rejected')}>
                            Reject
                        </button>
                        <button className="wc-btn primary" onClick={() => onDecision('Approved')}>
                            Approve workflow
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}

