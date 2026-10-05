import './WorkflowConsole.css';

export default function ApprovalConfirmationPopup({decision, onCancel, onConfirm}) {
    return (
        <div className="wc-modal-backdrop" onClick={(e) => { if (e.target === e.currentTarget) onCancel(); }}>
            <div className="wc-modal-card wc-confirm-modal" role="dialog" aria-modal="true" aria-labelledby="decision-title">
                <div className="wc-modal-header" style={{ borderBottom: 'none', background: 'transparent' }}>
                    <h2 id="decision-title" style={{ display: 'none' }}>{decision} workflow?</h2>
                    <button className="wc-modal-close" onClick={onCancel} style={{ marginLeft: 'auto' }}>×</button>
                </div>
                <div className="wc-modal-body" style={{ textAlign: 'center', paddingTop: 0 }}>
                    <span className={`wc-decision-symbol ${decision === 'Rejected' ? 'reject' : ''}`}>
                        {decision === 'Approved' ? '✓' : '!'}
                    </span>
                    <h2 style={{ font: "700 20px 'Space Grotesk', sans-serif", color: 'var(--ink)', margin: '14px 0 6px' }}>
                        {decision} workflow?
                    </h2>
                    <p style={{ color: 'var(--muted)', fontSize: '13px', margin: 0, lineHeight: 1.5 }}>
                        This decision will be sent to the ASP.NET Core API and added to the audit history.
                    </p>
                </div>
                <div className="wc-modal-footer" style={{ justifyContent: 'center', background: '#fafbfd' }}>
                    <button className="wc-btn outline" onClick={onCancel}>
                        Cancel
                    </button>
                    <button
                        className={`wc-btn ${decision === 'Rejected' ? 'danger-filled' : 'primary'}`}
                        onClick={onConfirm}
                    >
                        Confirm {decision.toLowerCase()}
                    </button>
                </div>
            </div>
        </div>
    );
}
