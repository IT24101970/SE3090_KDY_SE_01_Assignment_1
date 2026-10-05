import { useState, useEffect } from 'react';

const AI_API_BASE = 'http://localhost:8000/api/agent/doctor-scheduling';
const BACKEND_API_BASE = 'http://localhost:5066/api/doctor-scheduling';

export default function AiScheduleOptimizerModal({ isOpen, onClose, doctors, rooms, onScheduleCreated }) {
  const [pendingAppointments, setPendingAppointments] = useState([]);
  const [selectedApptId, setSelectedApptId] = useState('');
  const [windowStart, setWindowStart] = useState('');
  const [durationMinutes, setDurationMinutes] = useState(120);

  const [loading, setLoading] = useState(false);
  const [fetchingAppts, setFetchingAppts] = useState(false);
  const [workflowState, setWorkflowState] = useState(null);
  const [errorAlert, setErrorAlert] = useState(null);
  const [successAlert, setSuccessAlert] = useState(null);

  useEffect(() => {
    if (isOpen) {
      setFetchingAppts(true);
      fetch(`${BACKEND_API_BASE}/doctorschedules/pending-appointments`)
        .then((res) => (res.ok ? res.json() : []))
        .then((data) => {
          if (Array.isArray(data) && data.length > 0) {
            setPendingAppointments(data);
            setSelectedApptId(data[0].id);
          } else {
            setPendingAppointments([]);
            setSelectedApptId('');
          }
        })
        .catch(() => {
          setPendingAppointments([]);
          setSelectedApptId('');
        })
        .finally(() => setFetchingAppts(false));
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const handleRunAiOptimization = async (e) => {
    e.preventDefault();
    setLoading(true);
    setErrorAlert(null);
    setSuccessAlert(null);
    setWorkflowState(null);

    const startDt = windowStart ? new Date(windowStart) : new Date(Date.now() + 24 * 60 * 60 * 1000);
    startDt.setHours(9, 0, 0, 0); // Default 09:00 AM slot
    const endDt = new Date(startDt.getTime() + durationMinutes * 60 * 1000);

    const payload = {
      appointment_id: Number(selectedApptId || 1001),
      desired_window_start: startDt.toISOString(),
      desired_window_end: endDt.toISOString(),
      estimated_duration_minutes: Number(durationMinutes),
      max_patients: 15
    };

    try {
      let res;
      try {
        res = await fetch(`${AI_API_BASE}/optimize`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
      } catch {
        res = await fetch(`${BACKEND_API_BASE}/doctorsschedules/ai-optimize`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
      }

      if (!res.ok) {
        throw new Error(`AI Agent Workflow execution failed (HTTP ${res.status})`);
      }

      const state = await res.json();
      setWorkflowState(state);
    } catch (err) {
      setErrorAlert(`Optimization Error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleApproveWorkflow = async () => {
    if (!workflowState) return;
    setLoading(true);
    setErrorAlert(null);

    try {
      const res = await fetch(`${BACKEND_API_BASE}/doctorsschedules/ai-approve/${workflowState.workflow_id}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
      });

      if (!res.ok) {
        const errJson = await res.json();
        throw new Error(errJson.message || 'Approval execution failed');
      }

      const createdSchedule = await res.json();
      setSuccessAlert('Recommendation Approved! Doctor Schedule committed to database.');
      if (onScheduleCreated) onScheduleCreated(createdSchedule);
      
      setTimeout(() => {
        onClose();
      }, 1800);
    } catch (err) {
      setErrorAlert(`Approval execution error: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleRejectWorkflow = async () => {
    if (!workflowState) return;
    setLoading(true);

    try {
      await fetch(`${AI_API_BASE}/workflows/${workflowState.workflow_id}/reject`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason: 'Rejected by User' })
      });
      setErrorAlert('Recommendation rejected.');
      setWorkflowState((prev) => ({ ...prev, approval_status: 'Rejected' }));
    } catch (err) {
      setErrorAlert(`Rejection failed: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="ds-modal-overlay">
      <div className="ds-modal-content" style={{ maxWidth: 750 }}>
        <div className="ds-modal-header" style={{ background: '#1e293b', color: 'white', padding: '16px 24px', borderRadius: '12px 12px 0 0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h3 style={{ margin: 0, fontSize: '1.2rem', fontWeight: '600' }}>Schedule Optimization</h3>
          <button onClick={onClose} style={{ background: 'transparent', border: 'none', color: 'white', fontSize: '1.4rem', cursor: 'pointer' }}>×</button>
        </div>

        <div style={{ padding: 24, maxHeight: '80vh', overflowY: 'auto' }}>
          {errorAlert && <div className="ds-alert ds-alert-error" style={{ marginBottom: 16 }}>{errorAlert}</div>}
          {successAlert && <div className="ds-alert ds-alert-success" style={{ marginBottom: 16 }}>{successAlert}</div>}

          <form onSubmit={handleRunAiOptimization} style={{ background: '#f8fafc', padding: 18, borderRadius: 10, border: '1px solid #e2e8f0', marginBottom: 20 }}>
            <div className="ds-form-row">
              <div className="ds-form-group" style={{ gridColumn: 'span 2' }}>
                <label className="ds-label" style={{ fontWeight: '600', color: '#1e293b', marginBottom: 6 }}>
                  Select Pending Patient Appointment
                </label>
                {fetchingAppts ? (
                  <div style={{ fontSize: '0.85rem', color: '#64748b', padding: 8 }}>Loading pending appointments from database...</div>
                ) : pendingAppointments.length === 0 ? (
                  <div style={{ fontSize: '0.9rem', color: '#64748b', background: '#ffffff', padding: '10px 14px', borderRadius: 6, border: '1px solid #cbd5e1' }}>
                    No pending unassigned appointments found in database.
                  </div>
                ) : (
                  <select className="ds-select" value={selectedApptId} onChange={(e) => setSelectedApptId(e.target.value)}>
                    {pendingAppointments.map((a) => (
                      <option key={a.id} value={a.id}>
                        Appt #{a.id} — {a.patientName} | Symptoms: "{a.reasonForVisit}" ({a.recommendedSpecialty} | Urgency: {a.urgencyScore} {a.urgencyLevel})
                      </option>
                    ))}
                  </select>
                )}
              </div>
            </div>

            <button type="submit" className="ds-btn ds-btn-coral" disabled={loading || pendingAppointments.length === 0} style={{ marginTop: 12, padding: '12px 24px', width: '100%', fontSize: '0.95rem', fontWeight: '600', opacity: pendingAppointments.length === 0 ? 0.6 : 1 }}>
              {loading ? 'Running Schedule Optimizer...' : 'Run Schedule Optimizer'}
            </button>
          </form>

          {/* WORKFLOW RESULTS */}
          {workflowState && (
            <div style={{ border: '1px solid #cbd5e1', borderRadius: 10, padding: 20, background: '#ffffff' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <div>
                  <h4 style={{ margin: 0, color: '#0f172a', fontSize: '1.1rem' }}>Optimization Results</h4>
                  <span style={{ fontSize: '0.8rem', color: '#64748b' }}>Workflow ID: {workflowState.workflow_id}</span>
                </div>
                <span className={`ds-badge ${workflowState.approval_status === 'PendingApproval' ? 'ds-badge-pending' : workflowState.approval_status === 'Approved' ? 'ds-badge-approved' : 'ds-badge-rejected'}`}>
                  {workflowState.approval_status}
                </span>
              </div>

              {/* RECOMMENDED OPTION SUMMARY */}
              {!workflowState.recommended_option ? (
                <div style={{ background: '#fef2f2', border: '1px solid #fecaca', padding: 18, borderRadius: 10, marginBottom: 18, color: '#991b1b' }}>
                  <h5 style={{ margin: '0 0 6px 0', fontSize: '1rem', fontWeight: '600' }}>No Active Doctor Schedule Available</h5>
                  <p style={{ margin: 0, fontSize: '0.88rem', color: '#7f1d1d' }}>
                    No active doctor schedule slot or available specialist was found for this required specialty on the target date. Please add a Doctor Schedule for this specialty or check doctor leave status.
                  </p>
                </div>
              ) : (() => {
                const rec = workflowState.recommended_option;
                const matchedAppt = pendingAppointments.find(a => Number(a.id) === Number(rec.appointment_id || selectedApptId));
                return (
                  <div style={{ background: '#f0f9ff', border: '1px solid #bae6fd', padding: 18, borderRadius: 10, marginBottom: 18 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                      <h5 style={{ margin: 0, color: '#0369a1', fontSize: '1.05rem', fontWeight: '600' }}>
                        Recommended Doctor Schedule Placement
                      </h5>
                      <div style={{ background: '#0284c7', color: 'white', padding: '4px 12px', borderRadius: 20, fontWeight: '600', fontSize: '0.82rem' }}>
                        Score: {rec.conflict_free_score}%
                      </div>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, fontSize: '0.9rem', color: '#1e293b' }}>
                      <div style={{ background: '#ffffff', padding: 10, borderRadius: 6, border: '1px solid #e2e8f0' }}>
                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: '#64748b', fontWeight: 'bold', display: 'block' }}>Patient & Appointment</span>
                        <strong style={{ fontSize: '0.95rem', color: '#0f172a' }}>
                          Appt #{rec.appointment_id || selectedApptId} {matchedAppt ? `— ${matchedAppt.patientName}` : ''}
                        </strong>
                        {matchedAppt?.reasonForVisit && (
                          <div style={{ fontSize: '0.82rem', color: '#475569', marginTop: 2 }}>
                            Symptoms: "{matchedAppt.reasonForVisit}"
                          </div>
                        )}
                      </div>

                      <div style={{ background: '#ffffff', padding: 10, borderRadius: 6, border: '1px solid #e2e8f0' }}>
                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: '#64748b', fontWeight: 'bold', display: 'block' }}>Assigned Specialist</span>
                        <strong style={{ fontSize: '0.95rem', color: '#0f172a' }}>{rec.doctor_name}</strong>
                        <div style={{ fontSize: '0.82rem', color: '#0369a1' }}>Specialty: {rec.specialty_name}</div>
                      </div>

                      <div style={{ background: '#ffffff', padding: 10, borderRadius: 6, border: '1px solid #e2e8f0' }}>
                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: '#64748b', fontWeight: 'bold', display: 'block' }}>Priority Placement</span>
                        <strong style={{ fontSize: '0.95rem', color: '#15803d' }}>{rec.priority_rank}</strong>
                        <div style={{ fontSize: '0.82rem', color: '#475569' }}>Triage Urgency: {rec.urgency_level}</div>
                      </div>

                      <div style={{ background: '#ffffff', padding: 10, borderRadius: 6, border: '1px solid #e2e8f0' }}>
                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: '#64748b', fontWeight: 'bold', display: 'block' }}>Allocated Room</span>
                        <strong style={{ fontSize: '0.95rem', color: '#0f172a' }}>{rec.allocated_room_name}</strong>
                        <div style={{ fontSize: '0.82rem', color: '#475569' }}>Floor / Wing: {rec.floor}</div>
                      </div>

                      <div style={{ gridColumn: 'span 2', background: '#ffffff', padding: 12, borderRadius: 6, border: '1px solid #cbd5e1' }}>
                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: '#64748b', fontWeight: 'bold', display: 'block' }}>Recommended Session Date & Time</span>
                        <div style={{ fontSize: '0.95rem', fontWeight: 'bold', color: '#0f172a', marginTop: 2 }}>
                          {new Date(rec.recommended_start_time).toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' })}
                        </div>
                        <div style={{ fontSize: '0.9rem', color: '#0284c7', fontWeight: '600', marginTop: 2 }}>
                          {new Date(rec.recommended_start_time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} — {new Date(rec.recommended_end_time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })()}

              {/* APPROVAL ACTIONS */}
              {workflowState.approval_status === 'PendingApproval' && (
                <div style={{ display: 'flex', gap: 12 }}>
                  <button onClick={handleApproveWorkflow} className="ds-btn ds-btn-coral" style={{ flex: 1, padding: '12px 20px', fontSize: '0.95rem', fontWeight: '600' }}>
                    Approve & Commit Schedule
                  </button>
                  <button onClick={handleRejectWorkflow} className="ds-btn ds-btn-danger" style={{ padding: '12px 20px', fontSize: '0.9rem' }}>
                    Reject
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

