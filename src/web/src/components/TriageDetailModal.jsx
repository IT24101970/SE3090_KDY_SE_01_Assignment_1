import React from 'react';

export function TriageDetailModal({ assessment, referral, onClose, onUpdateReferralStatus }) {
  if (!assessment) return null;

  const getUrgencyClass = (level) => {
    switch (level?.toLowerCase()) {
      case 'emergency': return 'emergency';
      case 'high': return 'high';
      case 'medium': return 'medium';
      default: return 'low';
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div>
            <h2>Triage Assessment #{assessment.id}</h2>
            <p className="text-secondary" style={{ margin: 0, fontSize: '0.88rem' }}>
              Appointment ID: #{assessment.appointmentId} &bull; Submitted {new Date(assessment.createdAt).toLocaleString()}
            </p>
          </div>
          <button className="close-btn" onClick={onClose}>&times;</button>
        </div>

        <div className="modal-section">
          <h4>Urgency & Recommended Specialty</h4>
          <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <span className={`urgency-badge ${getUrgencyClass(assessment.urgencyLevel)}`}>
              {assessment.urgencyLevel} (Score: {assessment.urgencyScore}/100)
            </span>
            <span style={{ fontSize: '1.05rem', fontWeight: 600, color: '#38bdf8' }}>
              Specialty: {assessment.recommendedSpecialty || 'General Medicine'}
            </span>
          </div>
        </div>

        <div className="modal-section">
          <h4>Patient Symptoms & Severity Breakdown</h4>
          <div style={{ background: 'rgba(15, 23, 42, 0.6)', padding: '1rem', borderRadius: '10px', marginBottom: '0.8rem' }}>
            <p style={{ margin: '0 0 0.5rem 0', fontWeight: 500 }}>
              <strong>Raw Symptoms:</strong> {assessment.rawSymptoms}
            </p>
            {assessment.symptomLogs && assessment.symptomLogs.length > 0 && (
              <ul style={{ margin: 0, paddingLeft: '1.2rem', color: '#cbd5e1' }}>
                {assessment.symptomLogs.map((s, idx) => (
                  <li key={idx}>
                    <strong>{s.symptomKeyword}</strong> — Severity: {s.severityRating}/10, Duration: {s.durationInDays} days
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="modal-section">
          <h4>AI Agent Reasoning Trace</h4>
          <div className="reasoning-box">
            {assessment.reasoningTrace || 'No reasoning trace available.'}
          </div>
        </div>

        <div className="modal-section">
          <h4>Referral Management & Clinical Status</h4>
          <p style={{ margin: '0 0 0.8rem 0', color: '#94a3b8' }}>
            Current Referral Status: <strong style={{ color: '#f8fafc' }}>{referral?.status || 'Generated'}</strong>
          </p>
          <div className="referral-actions">
            {referral?.status !== 'Reviewed' && referral?.status !== 'Assigned' && (
              <button
                className="btn-update review"
                onClick={() => onUpdateReferralStatus(referral?.id || assessment.id, 'Reviewed')}
              >
                Mark as Reviewed
              </button>
            )}
            {referral?.status !== 'Assigned' && (
              <button
                className="btn-update assign"
                onClick={() => onUpdateReferralStatus(referral?.id || assessment.id, 'Assigned')}
              >
                Assign Specialist Referral
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
