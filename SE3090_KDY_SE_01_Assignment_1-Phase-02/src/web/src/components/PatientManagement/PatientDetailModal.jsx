import { useState, useEffect } from 'react';
import { appointmentApi } from '../../api/channelCenterApi';

export default function PatientDetailModal({
  patient,
  isOpen,
  onClose,
  onEdit,
  onBookAppointment,
  onLaunchIntake,
}) {
  const [history, setHistory] = useState([]);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [historyError, setHistoryError] = useState('');

  const loadHistory = async (patientId) => {
    setLoadingHistory(true);
    setHistoryError('');
    try {
      const res = await appointmentApi.getPatientHistory(patientId, { pageSize: 10 });
      setHistory(res.items || []);
    } catch (err) {
      setHistoryError(err.message || 'Failed to load appointment history.');
    } finally {
      setLoadingHistory(false);
    }
  };

  useEffect(() => {
    if (patient && isOpen) {
      loadHistory(patient.id);
    }
  }, [patient, isOpen]);

  if (!isOpen || !patient) return null;

  const formatDate = (isoString) => {
    if (!isoString) return '—';
    const date = new Date(isoString);
    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="pm-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="pm-modal-card large" onClick={(e) => e.stopPropagation()}>
        <div className="pm-modal-header">
          <div>
            <span className="eyebrow">Medical Profile & Booking Record</span>
            <h2>{patient.name}</h2>
          </div>
          <button className="pm-modal-close" onClick={onClose} aria-label="Close dialog">×</button>
        </div>

        <div className="pm-modal-body">
          {/* Demographics Card */}
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
            gap: '14px',
            background: '#f8fafc',
            border: '1px solid var(--line)',
            borderRadius: '12px',
            padding: '18px 20px',
            marginBottom: '20px',
          }}>
            <div>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>NATIONAL ID (NIC)</div>
              <strong style={{ fontSize: '14px', color: 'var(--ink)' }}>{patient.nic}</strong>
            </div>
            <div>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>AGE / GENDER</div>
              <strong style={{ fontSize: '14px', color: 'var(--ink)' }}>{patient.age} yrs · {patient.gender}</strong>
            </div>
            <div>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>BLOOD GROUP</div>
              <span className="pm-badge low" style={{ fontWeight: 700 }}>{patient.bloodGroup || 'Not Recorded'}</span>
            </div>
            <div>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>PHONE</div>
              <strong style={{ fontSize: '14px', color: 'var(--ink)' }}>{patient.phoneNumber}</strong>
            </div>
            <div style={{ gridColumn: 'span 2' }}>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>EMAIL</div>
              <span style={{ fontSize: '13px', color: 'var(--ink)' }}>{patient.email}</span>
            </div>
            <div style={{ gridColumn: 'span 2' }}>
              <div className="pm-label" style={{ color: 'var(--muted)', fontSize: '11px' }}>EMERGENCY CONTACT</div>
              <strong style={{ fontSize: '13px', color: 'var(--ink)' }}>{patient.emergencyContact}</strong>
            </div>
          </div>

          {/* Clinical Alerts / Notes */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '24px' }}>
            <div style={{
              background: patient.allergies ? '#fff5f5' : '#fff',
              border: `1px solid ${patient.allergies ? '#fed7d7' : 'var(--line)'}`,
              borderRadius: '10px',
              padding: '14px 16px',
            }}>
              <div style={{
                fontSize: '11px',
                fontWeight: 700,
                textTransform: 'uppercase',
                color: patient.allergies ? '#c53030' : 'var(--muted)',
                marginBottom: '6px',
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
              }}>
                <span>⚠️ Known Allergies</span>
              </div>
              <p style={{ margin: 0, fontSize: '13px', color: patient.allergies ? '#742a2a' : 'var(--muted)' }}>
                {patient.allergies || 'No allergies recorded.'}
              </p>
            </div>

            <div style={{
              background: '#fff',
              border: '1px solid var(--line)',
              borderRadius: '10px',
              padding: '14px 16px',
            }}>
              <div style={{
                fontSize: '11px',
                fontWeight: 700,
                textTransform: 'uppercase',
                color: 'var(--muted)',
                marginBottom: '6px',
              }}>
                📋 Medical History & Conditions
              </div>
              <p style={{ margin: 0, fontSize: '13px', color: 'var(--ink)' }}>
                {patient.medicalHistory || 'No historical medical conditions noted.'}
              </p>
            </div>
          </div>

          {/* Appointment History Section */}
          <div style={{ marginTop: '10px' }}>
            <div style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '12px',
            }}>
              <h3 style={{ font: "700 16px 'Space Grotesk', sans-serif", margin: 0 }}>
                Appointment History ({history.length})
              </h3>
              <button
                className="pm-btn-sm outline"
                onClick={() => loadHistory(patient.id)}
                disabled={loadingHistory}
              >
                {loadingHistory ? 'Refreshing...' : '↻ Refresh'}
              </button>
            </div>

            {loadingHistory && (
              <div className="pm-loading-box">
                <div className="pm-spinner" />
                <span>Loading appointment history...</span>
              </div>
            )}

            {historyError && (
              <div className="pm-alert error">
                <span>⚠️ {historyError}</span>
              </div>
            )}

            {!loadingHistory && !historyError && history.length === 0 && (
              <div className="pm-empty-state" style={{ padding: '24px 12px' }}>
                <div className="icon">📅</div>
                <h4 style={{ margin: '0 0 4px', fontSize: '14px' }}>No Appointments Booked Yet</h4>
                <p style={{ margin: 0, fontSize: '12px' }}>This patient does not have any prior or upcoming appointments.</p>
              </div>
            )}

            {!loadingHistory && history.length > 0 && (
              <div className="pm-table-wrapper" style={{ maxHeight: '240px', overflowY: 'auto' }}>
                <table className="pm-table">
                  <thead>
                    <tr>
                      <th>Ref</th>
                      <th>Date & Time</th>
                      <th>Doctor / Specialty</th>
                      <th>Room</th>
                      <th>Reason</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {history.map((appt) => (
                      <tr key={appt.id}>
                        <td style={{ fontWeight: 600 }}>#{appt.id}</td>
                        <td style={{ whiteSpace: 'nowrap' }}>{formatDate(appt.appointmentDate)}</td>
                        <td>
                          <strong>{appt.doctorName || 'Dr. Assigned'}</strong>
                          <div style={{ fontSize: '11px', color: 'var(--muted)' }}>{appt.doctorSpecialty}</div>
                        </td>
                        <td>{appt.roomName || 'Room 101'}</td>
                        <td style={{ maxWidth: '180px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {appt.reasonForVisit}
                        </td>
                        <td>
                          <span className={`pm-badge ${String(appt.status).toLowerCase()}`}>
                            {appt.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>

        <div className="pm-modal-footer" style={{ justifyContent: 'space-between' }}>
          <div>
            <button
              type="button"
              className="outline-button"
              onClick={() => onEdit(patient)}
            >
              ✎ Edit Profile
            </button>
          </div>
          <div style={{ display: 'flex', gap: '8px' }}>
            <button
              type="button"
              className="outline-button"
              onClick={() => onLaunchIntake(patient)}
            >
              ⚡ AI Intake
            </button>
            <button
              type="button"
              className="primary-button"
              onClick={() => onBookAppointment(patient)}
            >
              📅 Book Appointment
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
