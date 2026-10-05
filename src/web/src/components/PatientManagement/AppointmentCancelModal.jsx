import { useState, useEffect } from 'react';
import { appointmentApi } from '../../api/channelCenterApi';

export default function AppointmentCancelModal({
  appointment,
  isOpen,
  onClose,
  onCancelled,
}) {
  const [cancelReason, setCancelReason] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [prevIsOpen, setPrevIsOpen] = useState(isOpen);

  if (isOpen !== prevIsOpen) {
    setPrevIsOpen(isOpen);
    if (isOpen) {
      setCancelReason('');
      setError('');
    }
  }

  if (!isOpen || !appointment) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!cancelReason.trim()) {
      setError('A cancellation reason must be provided.');
      return;
    }

    setSubmitting(true);
    try {
      const updated = await appointmentApi.cancel(appointment.id, cancelReason.trim());
      onCancelled(updated, `Appointment #${appointment.id} was successfully cancelled.`);
      onClose();
    } catch (err) {
      setError(err.message || 'Failed to cancel appointment.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="pm-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="pm-modal-card" style={{ maxWidth: '480px' }} onClick={(e) => e.stopPropagation()}>
        <div className="pm-modal-header">
          <div>
            <span className="eyebrow" style={{ color: 'var(--red)' }}>Booking Cancellation</span>
            <h2>Cancel Appointment #{appointment.id}</h2>
          </div>
          <button className="pm-modal-close" onClick={onClose} aria-label="Close dialog">×</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="pm-modal-body">
            <p style={{ margin: '0 0 14px', fontSize: '13px', color: 'var(--muted)', lineHeight: '1.5' }}>
              Are you sure you want to cancel the appointment for <strong>{appointment.patientName}</strong> with{' '}
              <strong>{appointment.doctorName}</strong> on {new Date(appointment.appointmentDate).toLocaleString()}?
            </p>

            {error && (
              <div className="pm-alert error" role="alert">
                <span>⚠️</span> <span>{error}</span>
              </div>
            )}

            <div className="pm-form-group">
              <label className="pm-label" htmlFor="cancel-reason-input">
                Cancellation Reason <span className="required">*</span>
              </label>
              <textarea
                id="cancel-reason-input"
                className={`pm-textarea ${error ? 'error' : ''}`}
                rows={3}
                placeholder="e.g. Patient called to reschedule, Doctor emergency unavailable, Symptom resolved"
                value={cancelReason}
                onChange={(e) => {
                  setCancelReason(e.target.value);
                  if (error) setError('');
                }}
                disabled={submitting}
                required
              />
              <span style={{ fontSize: '11px', color: 'var(--muted)' }}>
                This reason will be recorded in the hospital audit trail.
              </span>
            </div>
          </div>

          <div className="pm-modal-footer">
            <button
              type="button"
              className="outline-button"
              onClick={onClose}
              disabled={submitting}
            >
              Keep Appointment
            </button>
            <button
              type="submit"
              className="danger-button"
              disabled={submitting}
            >
              {submitting ? 'Cancelling...' : 'Confirm Cancellation'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
