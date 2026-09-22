export default function ApprovalConfirmationPopup({decision, onCancel, onConfirm}) {
  return (
      <div className="modal-backdrop">
        <div className="confirm-modal" role="dialog" aria-modal="true" aria-labelledby="decision-title">
          <button className="modal-close" onClick={onCancel}>×</button>
          
          <span
              className={`decision-symbol ${decision === 'Rejected' ? 'reject' : ''}`}>{decision === 'Approved' ? '✓' : '!'}
          </span>
          
          <h2 id="decision-title">{decision} workflow?</h2>
          <p>This decision will be sent to the ASP.NET Core API and added to the audit history.</p>
          
          <div className="modal-actions">
            <button className="outline-button" onClick={onCancel}>Cancel</button>
            <button className={decision === 'Rejected' ? 'danger-button' : 'primary-button'}
                    onClick={onConfirm}>Confirm {decision.toLowerCase()}
            </button>
          </div>
        </div>
      </div>
  );
}
