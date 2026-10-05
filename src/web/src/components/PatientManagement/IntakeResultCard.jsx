export default function IntakeResultCard({ result, onProceedToBooking }) {
  if (!result) return null;

  const {
    patientMetadata = {},
    symptoms = [],
    severityFlags = {},
    preferredTimeWindows = [],
    doctorPreferences = {},
    executionPlan = [],
    summaryText = '',
    processedAt,
  } = result;

  const isEmergency = severityFlags.isEmergency || severityFlags.urgencyLevel === 'Emergency' || severityFlags.urgencyLevel === 3;
  const redFlags = severityFlags.redFlagsDetected || [];

  return (
    <div className="pm-intake-pane" style={{ background: '#fff' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <span className="eyebrow">Structured Execution Contract</span>
          <h2 style={{ font: "700 20px 'Space Grotesk', sans-serif", margin: '4px 0 0', color: 'var(--ink)' }}>
            AI Intake Assessment Summary
          </h2>
        </div>
        <span className={`pm-badge ${isEmergency ? 'emergency' : 'confirmed'}`}>
          {isEmergency ? '🚨 Immediate Attention' : '✓ Verified & Structured'}
        </span>
      </div>

      {/* Emergency Red Flag Callout if detected */}
      {isEmergency && (
        <div className="pm-alert error" style={{ margin: '4px 0 0' }}>
          <span style={{ fontSize: '18px' }}>⚠️</span>
          <div>
            <strong>Emergency Clinical Red Flags Detected:</strong>{' '}
            {redFlags.length > 0 ? redFlags.join(', ') : 'Urgent critical symptoms flagged'}
            <div style={{ fontSize: '11px', marginTop: '2px' }}>
              Execution plan recommends escalation to emergency care and medical supervisor oversight.
            </div>
          </div>
        </div>
      )}

      {/* Patient Eligibility Header */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
        gap: '10px',
        background: '#f8fafc',
        border: '1px solid var(--line)',
        borderRadius: '10px',
        padding: '12px 14px',
      }}>
        <div>
          <div style={{ fontSize: '11px', color: 'var(--muted)', fontWeight: 600 }}>PATIENT</div>
          <strong style={{ fontSize: '13px' }}>{patientMetadata.name}</strong>
        </div>
        <div>
          <div style={{ fontSize: '11px', color: 'var(--muted)', fontWeight: 600 }}>AGE / GENDER</div>
          <span style={{ fontSize: '13px' }}>{patientMetadata.age} yrs · {patientMetadata.gender}</span>
        </div>
        <div>
          <div style={{ fontSize: '11px', color: 'var(--muted)', fontWeight: 600 }}>ELIGIBILITY</div>
          <span className="pm-badge confirmed" style={{ fontSize: '10px', padding: '2px 8px' }}>
            {patientMetadata.eligibilityStatus || 'Eligible'}
          </span>
        </div>
        <div>
          <div style={{ fontSize: '11px', color: 'var(--muted)', fontWeight: 600 }}>URGENCY LEVEL</div>
          <span className={`pm-badge ${isEmergency ? 'emergency' : 'medium'}`} style={{ fontSize: '10px', padding: '2px 8px' }}>
            {severityFlags.urgencyLevel || 'Medium'}
          </span>
        </div>
      </div>

      {/* Structured Symptoms Table */}
      <div>
        <div style={{ fontSize: '12px', fontWeight: 700, color: 'var(--ink)', marginBottom: '8px' }}>
          Extracted Symptoms ({symptoms.length})
        </div>
        {symptoms.length === 0 ? (
          <p style={{ fontSize: '12px', color: 'var(--muted)', margin: 0 }}>No discrete symptoms identified.</p>
        ) : (
          <div className="pm-table-wrapper" style={{ border: '1px solid var(--line)', borderRadius: '8px' }}>
            <table className="pm-table" style={{ fontSize: '12px' }}>
              <thead>
                <tr>
                  <th style={{ padding: '8px 12px' }}>Symptom</th>
                  <th style={{ padding: '8px 12px' }}>Severity</th>
                  <th style={{ padding: '8px 12px' }}>Duration</th>
                  <th style={{ padding: '8px 12px' }}>Clinical Context</th>
                </tr>
              </thead>
              <tbody>
                {symptoms.map((s, idx) => (
                  <tr key={idx}>
                    <td style={{ fontWeight: 600, padding: '8px 12px' }}>{s.keyword}</td>
                    <td style={{ padding: '8px 12px' }}>
                      <span className={`pm-badge ${s.severity === 'Severe' || s.severity === 'Critical' ? 'emergency' : 'low'}`} style={{ fontSize: '10px' }}>
                        {s.severity}
                      </span>
                    </td>
                    <td style={{ padding: '8px 12px', color: 'var(--muted)' }}>{s.duration || 'Not specified'}</td>
                    <td style={{ padding: '8px 12px', color: 'var(--ink)' }}>{s.notes || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Preferences & Windows */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        <div style={{ background: '#f8fafc', border: '1px solid var(--line)', borderRadius: '8px', padding: '10px 12px' }}>
          <div style={{ fontSize: '11px', fontWeight: 700, color: 'var(--muted)', marginBottom: '4px' }}>
            PREFERRED SPECIALTY / DOCTOR
          </div>
          <div style={{ fontSize: '13px', fontWeight: 600, color: 'var(--ink)' }}>
            {doctorPreferences.preferredSpecialty || 'General Practice'}
          </div>
          {doctorPreferences.preferredDoctorName && (
            <div style={{ fontSize: '11px', color: 'var(--blue)' }}>
              Requested: {doctorPreferences.preferredDoctorName}
            </div>
          )}
        </div>

        <div style={{ background: '#f8fafc', border: '1px solid var(--line)', borderRadius: '8px', padding: '10px 12px' }}>
          <div style={{ fontSize: '11px', fontWeight: 700, color: 'var(--muted)', marginBottom: '4px' }}>
            PREFERRED TIME WINDOWS
          </div>
          <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
            {preferredTimeWindows.length > 0 ? (
              preferredTimeWindows.map((tw, idx) => (
                <span key={idx} className="pm-chip" style={{ fontSize: '10px', padding: '2px 8px' }}>
                  🕒 {tw}
                </span>
              ))
            ) : (
              <span style={{ fontSize: '12px', color: 'var(--muted)' }}>Any available time slot</span>
            )}
          </div>
        </div>
      </div>

      {/* Formatted Clinical Summary */}
      {summaryText && (
        <div>
          <div style={{ fontSize: '11px', fontWeight: 700, color: 'var(--muted)', marginBottom: '4px' }}>
            FORMATTED CLINICAL SUMMARY
          </div>
          <p style={{
            fontSize: '12px',
            lineHeight: '1.5',
            color: 'var(--ink)',
            background: '#fff',
            border: '1px solid var(--line)',
            borderRadius: '8px',
            padding: '10px 12px',
            margin: 0,
          }}>
            {summaryText}
          </p>
        </div>
      )}

      {/* Multi-Step Execution Plan */}
      <div>
        <div style={{ fontSize: '12px', fontWeight: 700, color: 'var(--ink)', marginBottom: '6px' }}>
          Deterministic Agent Execution Plan
        </div>
        <ul className="pm-plan-steps">
          {executionPlan.map((step, idx) => (
            <li key={idx} className="pm-plan-step">
              <span className="pm-step-icon">{idx + 1}</span>
              <span style={{ fontSize: '12px' }}>{step}</span>
            </li>
          ))}
        </ul>
      </div>

      {/* Bottom CTA to Book Slot */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        paddingTop: '12px',
        borderTop: '1px solid var(--line)',
        marginTop: '6px',
      }}>
        <span style={{ fontSize: '11px', color: 'var(--muted)' }}>
          Processed at {processedAt ? new Date(processedAt).toLocaleTimeString() : 'now'}
        </span>
        <button
          type="button"
          className="primary-button"
          onClick={() => onProceedToBooking(result)}
        >
          📅 Book Recommended Slot →
        </button>
      </div>
    </div>
  );
}
