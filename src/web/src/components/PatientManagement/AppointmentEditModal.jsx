import { useState, useEffect, useCallback } from 'react';
import { appointmentApi, doctorApi, specialtiesApi } from '../../api/channelCenterApi';
import './PatientManagement.css';

export default function AppointmentEditModal({
  isOpen,
  appointment,
  onClose,
  onUpdated,
}) {
  const [doctors, setDoctors] = useState([]);
  const [specialties, setSpecialties] = useState([]);
  const [schedules, setSchedules] = useState([]);
  const [loadingInitial, setLoadingInitial] = useState(false);

  // Editable state fields
  const [selectedDoctorId, setSelectedDoctorId] = useState('');
  const [selectedScheduleId, setSelectedScheduleId] = useState('');
  const [appointmentDate, setAppointmentDate] = useState('');
  const [status, setStatus] = useState('Pending');
  const [reasonForVisit, setReasonForVisit] = useState('');
  const [cancelReason, setCancelReason] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  // Searchable Doctor State
  const [doctorSearch, setDoctorSearch] = useState('');
  const [isDoctorMenuOpen, setIsDoctorMenuOpen] = useState(false);

  // Sync doctorSearch label when selectedDoctorId or doctors list changes
  useEffect(() => {
    if (selectedDoctorId && doctors.length > 0) {
      const doc = doctors.find((d) => String(d.id) === String(selectedDoctorId));
      if (doc) {
        const name = doc.doctorName || doc.fullName || doc.name || doc.user?.fullName || `Doctor #${doc.id}`;
        const spec = doc.specialtyName || doc.specialty?.name || 'Specialist';
        setDoctorSearch(`${name} (${spec})`);
      }
    }
  }, [selectedDoctorId, doctors]);

  const filteredDoctors = doctors.filter((d) => {
    if (!doctorSearch) return true;
    const q = doctorSearch.toLowerCase();
    const name = (d.doctorName || d.fullName || d.name || d.user?.fullName || '').toLowerCase();
    const spec = (d.specialtyName || d.specialty?.name || '').toLowerCase();
    return name.includes(q) || spec.includes(q);
  });

  // Pre-fill fields when modal opens with appointment
  useEffect(() => {
    if (isOpen && appointment) {
      setSelectedDoctorId(appointment.doctorId ? String(appointment.doctorId) : '');
      setSelectedScheduleId(appointment.scheduleId ? String(appointment.scheduleId) : '');
      
      if (appointment.appointmentDate) {
        const d = new Date(appointment.appointmentDate);
        // Format to YYYY-MM-DDTHH:mm for datetime-local input
        const localIso = new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
        setAppointmentDate(localIso);
      } else {
        setAppointmentDate('');
      }

      setStatus(appointment.status || 'Pending');
      setReasonForVisit(appointment.reasonForVisit || '');
      setCancelReason(appointment.cancelReason || '');
      setError('');
      setIsDoctorMenuOpen(false);
    }
  }, [isOpen, appointment]);

  const loadDoctorsAndSlots = useCallback(async () => {
    setLoadingInitial(true);
    try {
      const [docsRes, specsRes, slotsRes] = await Promise.allSettled([
        doctorApi.list(),
        specialtiesApi.list(),
        appointmentApi.getSlots(),
      ]);

      if (docsRes.status === 'fulfilled' && Array.isArray(docsRes.value)) {
        setDoctors(docsRes.value);
      }
      if (specsRes.status === 'fulfilled' && Array.isArray(specsRes.value)) {
        setSpecialties(specsRes.value);
      }
      if (slotsRes.status === 'fulfilled' && Array.isArray(slotsRes.value)) {
        setSchedules(slotsRes.value);
      }
    } catch {
      // Ignore fallback failures
    } finally {
      setLoadingInitial(false);
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      loadDoctorsAndSlots();
    }
  }, [isOpen, loadDoctorsAndSlots]);

  if (!isOpen || !appointment) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!reasonForVisit.trim()) {
      setError('Reason for visit cannot be blank.');
      return;
    }

    if (status === 'Cancelled' && !cancelReason.trim()) {
      setError('Please provide a cancellation reason when setting status to Cancelled.');
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        doctorId: selectedDoctorId ? Number(selectedDoctorId) : appointment.doctorId,
        scheduleId: selectedScheduleId ? Number(selectedScheduleId) : appointment.scheduleId,
        appointmentDate: appointmentDate ? new Date(appointmentDate).toISOString() : appointment.appointmentDate,
        reasonForVisit: reasonForVisit.trim(),
        status,
        cancelReason: status === 'Cancelled' ? cancelReason.trim() : null,
      };

      const updated = await appointmentApi.update(appointment.id, payload);
      if (onUpdated) {
        onUpdated(updated, `Appointment #${appointment.id} was updated successfully.`);
      }
      onClose();
    } catch (err) {
      setError(err.message || 'Failed to update appointment.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="pm-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="pm-modal-card" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '640px' }}>
        <form onSubmit={handleSubmit} className="pm-modal-form">
          <div className="pm-modal-header">
            <div>
              <span className="eyebrow">Appointment Management</span>
              <h2>Edit Appointment Details</h2>
            </div>
            <button type="button" className="pm-modal-close" onClick={onClose} aria-label="Close dialog">×</button>
          </div>

          <div className="pm-modal-body">
            {error && (
              <div className="pm-alert error" role="alert" style={{ marginBottom: '16px' }}>
                <span>⚠️</span> <span>{error}</span>
              </div>
            )}

            {/* Non-editable Patient Info Header */}
            <div style={{
              background: '#f8fafc',
              border: '1px solid var(--line)',
              borderRadius: '10px',
              padding: '12px 16px',
              marginBottom: '20px',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}>
              <div>
                <span className="pm-label" style={{ fontSize: '11px' }}>Patient Details (Read-only)</span>
                <strong style={{ fontSize: '14px', color: 'var(--ink)', display: 'block' }}>
                  {appointment.patientName || 'Walk-in / Unregistered Patient'}
                </strong>
                <span style={{ fontSize: '12px', color: 'var(--muted)' }}>
                  NIC: {appointment.patientNIC || 'N/A'} · Phone: {appointment.patientPhone || 'N/A'}
                </span>
              </div>
              <span className="pm-badge low">Fixed Patient</span>
            </div>

            {/* Reassign Doctor Search Bar */}
            <div style={{ marginBottom: '16px', position: 'relative' }}>
              <label className="pm-label" htmlFor="edit-doctor-search">
                Assigned Doctor & Specialty
              </label>
              <div style={{ position: 'relative', marginTop: '4px' }}>
                <input
                  id="edit-doctor-search"
                  type="text"
                  className="pm-input"
                  style={{ width: '100%', paddingRight: doctorSearch ? '32px' : '12px' }}
                  placeholder="🔍 Type doctor name or specialty (e.g. Sarah, Cardiology)..."
                  value={doctorSearch}
                  onFocus={() => setIsDoctorMenuOpen(true)}
                  onChange={(e) => {
                    setDoctorSearch(e.target.value);
                    setIsDoctorMenuOpen(true);
                  }}
                  disabled={submitting || loadingInitial}
                />
                {doctorSearch && (
                  <button
                    type="button"
                    style={{
                      position: 'absolute', right: '10px', top: '50%', transform: 'translateY(-50%)',
                      background: 'none', border: 'none', color: 'var(--muted)', cursor: 'pointer', fontSize: '14px'
                    }}
                    onClick={() => {
                      setSelectedDoctorId('');
                      setDoctorSearch('');
                      setIsDoctorMenuOpen(true);
                    }}
                  >
                    ✕
                  </button>
                )}
              </div>

              {isDoctorMenuOpen && (
                <div style={{
                  position: 'absolute', top: '100%', left: 0, right: 0, zIndex: 100,
                  background: 'var(--surface)', border: '1px solid var(--line)', borderRadius: '8px',
                  boxShadow: '0 8px 24px rgba(0,0,0,0.12)', maxHeight: '200px', overflowY: 'auto', marginTop: '4px'
                }}>
                  <div
                    style={{ padding: '8px 12px', cursor: 'pointer', borderBottom: '1px solid var(--line)', fontSize: '13px', color: 'var(--muted)' }}
                    onClick={() => {
                      setSelectedDoctorId('');
                      setDoctorSearch('');
                      setIsDoctorMenuOpen(false);
                    }}
                  >
                    -- Keep Current Doctor ({appointment.doctorName || 'Assigned'}) --
                  </div>
                  {filteredDoctors.length === 0 ? (
                    <div style={{ padding: '10px 12px', fontSize: '13px', color: 'var(--muted)' }}>
                      No matching doctors found for "{doctorSearch}"
                    </div>
                  ) : (
                    filteredDoctors.map((d) => {
                      const docName = d.doctorName || d.fullName || d.name || d.user?.fullName || `Doctor #${d.id}`;
                      const specName = d.specialtyName || d.specialty?.name || 'Specialist';
                      return (
                        <div
                          key={d.id}
                          style={{
                            padding: '9px 14px', cursor: 'pointer', borderBottom: '1px solid var(--line)',
                            display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '13px',
                            background: String(selectedDoctorId) === String(d.id) ? 'var(--blue-soft)' : 'transparent'
                          }}
                          onClick={() => {
                            setSelectedDoctorId(String(d.id));
                            setDoctorSearch(`${docName} (${specName})`);
                            setIsDoctorMenuOpen(false);
                            const matchSched = schedules.find((s) => String(s.doctorId) === String(d.id));
                            if (matchSched) setSelectedScheduleId(String(matchSched.scheduleId));
                          }}
                        >
                          <strong>👨‍⚕️ {docName}</strong>
                          <span className="pm-badge low" style={{ background: '#e6f4ff', color: 'var(--blue)' }}>{specName}</span>
                        </div>
                      );
                    })
                  )}
                </div>
              )}
            </div>

            {/* Reassign Doctor Schedule / Room Session */}
            <div style={{ marginBottom: '16px' }}>
              <label className="pm-label" htmlFor="edit-schedule-select">
                Channel Session Schedule & Consultation Room
              </label>
              <select
                id="edit-schedule-select"
                className="pm-select-field"
                style={{ width: '100%', marginTop: '4px' }}
                value={selectedScheduleId}
                onChange={(e) => setSelectedScheduleId(e.target.value)}
                disabled={submitting || loadingInitial}
              >
                <option value="">-- Keep Current Schedule ({appointment.roomName || 'Room 101'}) --</option>
                {schedules.map((s) => (
                  <option key={s.scheduleId} value={s.scheduleId}>
                    Session #{s.scheduleId}: {s.doctorName} ({s.specialtyName}) · {s.roomName} ({s.roomFloor})
                  </option>
                ))}
              </select>
            </div>

            {/* Appointment Date & Time Picker */}
            <div style={{ marginBottom: '16px' }}>
              <label className="pm-label" htmlFor="edit-appointment-date">
                Appointment Date & Time
              </label>
              <input
                id="edit-appointment-date"
                type="datetime-local"
                className="pm-input"
                style={{ width: '100%', marginTop: '4px' }}
                value={appointmentDate}
                onChange={(e) => setAppointmentDate(e.target.value)}
                disabled={submitting}
              />
            </div>

            {/* Status Dropdown */}
            <div style={{ marginBottom: '16px' }}>
              <label className="pm-label" htmlFor="edit-appointment-status">
                Appointment Status
              </label>
              <select
                id="edit-appointment-status"
                className="pm-select-field"
                style={{ width: '100%', marginTop: '4px' }}
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                disabled={submitting}
              >
                <option value="Pending">Pending (Awaiting Approval)</option>
                <option value="Confirmed">Confirmed (Ready for Visit)</option>
                <option value="Completed">Completed (Consultation Finished)</option>
                <option value="Cancelled">Cancelled (Booking Void)</option>
              </select>
            </div>

            {/* Cancel Reason (if Status is Cancelled) */}
            {status === 'Cancelled' && (
              <div style={{ marginBottom: '16px' }}>
                <label className="pm-label" htmlFor="edit-cancel-reason">
                  Cancellation Reason <span className="required">*</span>
                </label>
                <input
                  id="edit-cancel-reason"
                  type="text"
                  className="pm-input"
                  style={{ width: '100%', marginTop: '4px' }}
                  placeholder="Enter reason for cancelling this appointment..."
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                  disabled={submitting}
                />
              </div>
            )}

            {/* Reason for Visit */}
            <div style={{ marginBottom: '10px' }}>
              <label className="pm-label" htmlFor="edit-reason-visit">
                Reason for Visit / Symptoms
              </label>
              <textarea
                id="edit-reason-visit"
                className="pm-textarea"
                style={{ width: '100%', marginTop: '4px' }}
                rows={3}
                value={reasonForVisit}
                onChange={(e) => setReasonForVisit(e.target.value)}
                disabled={submitting}
              />
            </div>
          </div>

          <div className="pm-modal-footer">
            <button type="button" className="outline-button" onClick={onClose} disabled={submitting}>
              Cancel
            </button>
            <button type="submit" className="primary-button" disabled={submitting}>
              {submitting ? 'Saving Changes...' : 'Save Appointment Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
