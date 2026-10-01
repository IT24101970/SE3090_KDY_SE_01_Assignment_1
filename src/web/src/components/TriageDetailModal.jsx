import React from 'react';
import './ClinicalReviewView.css';
import './PatientManagement/PatientManagement.css';

export function TriageDetailModal({ assessment, referral, onClose, onUpdateReferralStatus }) {
  if (!assessment) return null;

  const getUrgencyClass = (level) => {
    switch (level?.toLowerCase()) {
      case 'emergency': return 'urgency-pill emergency';
      case 'high': return 'urgency-pill high';
      case 'medium': return 'urgency-pill medium';
      default: return 'urgency-pill low';
    }
  };

  return (
    <div className="pm-modal-overlay" onClick={onClose}>
      <div className="pm-modal" style={{ maxWidth: '680px' }} onClick={(e) => e.stopPropagation()}>
        <div className="pm-modal-header">
          <div>
            <h2 className="pm-modal-title">Triage Assessment #{assessment.id}</h2>
            <p className="pm-stat-hint" style={{ margin: 0 }}>
              Appointment ID: #{assessment.appointmentId} &bull; Submitted {new Date(assessment.createdAt).toLocaleString()}
            </p>
          </div>
          <button className="pm-modal-close" onClick={onClose}>&times;</button>
        </div>

        <div className="pm-modal-body" style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          <div>
            <h4 style={{ margin: '0 0 8px 0', fontSize: '13px', textTransform: 'uppercase', letterSpacing: '0.08em', color: 'var(--muted)' }}>
              Urgency & Recommended Specialty
            </h4>
            <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
              <span className={getUrgencyClass(assessment.urgencyLevel)}>
                {assessment.urgencyLevel} (Score: {assessment.urgencyScore}/100)
              </span>
              <span style={{ fontSize: '15px', fontWeight: 700, color: 'var(--blue)' }}>
                Specialty: {assessment.recommendedSpecialty || 'General Medicine'}
              </span>
            </div>
          </div>

          <div>
            <h4 style={{ margin: '0 0 8px 0', fontSize: '13px', textTransform: 'uppercase', letterSpacing: '0.08em', color: 'var(--muted)' }}>
              Patient Symptoms & Severity Breakdown
            </h4>
            <div style={{ background: 'var(--canvas)', border: '1px solid var(--line)', padding: '14px 16px', borderRadius: '10px' }}>
              <p style={{ margin: '0 0 8px 0', fontWeight: 600, color: 'var(--ink)' }}>
                Raw Symptoms: <span style={{ fontWeight: 400 }}>{assessment.rawSymptoms}</span>
              </p>
              {assessment.symptomLogs && assessment.symptomLogs.length > 0 && (
                <ul style={{ margin: 0, paddingLeft: '1.2rem', color: 'var(--ink)', fontSize: '13px' }}>
                  {assessment.symptomLogs.map((s, idx) => (
                    <li key={idx}>
                      <strong>{s.symptomKeyword}</strong> — Severity: {s.severityRating}/10, Duration: {s.durationInDays} days
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>

          <div>
            <h4 style={{ margin: '0 0 8px 0', fontSize: '13px', textTransform: 'uppercase', letterSpacing: '0.08em', color: 'var(--muted)' }}>
              AI Agent Reasoning Trace
            </h4>
            <div className="reasoning-box">
              {assessment.reasoningTrace || 'No reasoning trace available.'}
            </div>
          </div>

          <div>
            <h4 style={{ margin: '0 0 8px 0', fontSize: '13px', textTransform: 'uppercase', letterSpacing: '0.08em', color: 'var(--muted)' }}>
              Referral Management & Clinical Status
            </h4>
            <p style={{ margin: '0 0 10px 0', color: 'var(--muted)', fontSize: '13px' }}>
              Current Referral Status: <strong style={{ color: 'var(--ink)' }}>{referral?.status || 'Generated'}</strong>
            </p>
            <div style={{ display: 'flex', gap: '10px' }}>
              {referral?.status !== 'Assigned' && (
                <button
                  className="primary-button"
                  style={{ background: 'var(--teal)' }}
                  onClick={() => onUpdateReferralStatus(referral?.id || assessment.id, 'Assigned')}
                >
                  Assign Specialist Referral
                </button>
              )}
            </div>
          </div>
        </div>

        <div className="pm-modal-footer">
          <button className="outline-button" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
