import { useState, useEffect, useCallback } from 'react';
import { appointmentApi, patientApi, specialtiesApi, doctorApi } from '../../api/channelCenterApi';

export default function SlotBookingModal({
  preselectedPatient,
  initialSpecialty,
  isOpen,
  onClose,
  onBooked,
}) {
  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState(preselectedPatient ? preselectedPatient.id : '');
  const [specialties, setSpecialties] = useState([]);
  const [selectedSpecialtyId, setSelectedSpecialtyId] = useState('');
  const [doctors, setDoctors] = useState([]);
  const [selectedDoctorId, setSelectedDoctorId] = useState('');
  const [selectedDate, setSelectedDate] = useState(() => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  });

  const [slots, setSlots] = useState([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(null);

  const [patientError, setPatientError] = useState('');
  const [reasonForVisit, setReasonForVisit] = useState('');
  const [reasonError, setReasonError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState('');

  const [prevIsOpen, setPrevIsOpen] = useState(isOpen);
  const [prevPatient, setPrevPatient] = useState(preselectedPatient);

  if (isOpen !== prevIsOpen) {
    setPrevIsOpen(isOpen);
    if (isOpen) {
      setSelectedPatientId(preselectedPatient ? preselectedPatient.id : '');
      setPatientError('');
      setSelectedSlot(null);
      setReasonForVisit('');
      setReasonError('');
      setApiError('');
    }
  }

  if (preselectedPatient !== prevPatient) {
    setPrevPatient(preselectedPatient);
    if (preselectedPatient) {
      setSelectedPatientId(preselectedPatient.id);
    }
  }

  const loadInitialData = useCallback(async () => {
    try {
      const [patientsRes, specsRes, docsRes] = await Promise.allSettled([
        patientApi.list({ pageSize: 50 }),
        specialtiesApi.list(),
        doctorApi.list(),
      ]);

      if (patientsRes.status === 'fulfilled' && patientsRes.value?.items) {
        setPatients(patientsRes.value.items);
      }
      if (specsRes.status === 'fulfilled' && Array.isArray(specsRes.value)) {
        setSpecialties(specsRes.value);
        if (initialSpecialty) {
          const match = specsRes.value.find((s) => s.name?.toLowerCase() === initialSpecialty.toLowerCase());
          if (match) setSelectedSpecialtyId(match.id);
        }
      }
      if (docsRes.status === 'fulfilled' && Array.isArray(docsRes.value)) {
        setDoctors(docsRes.value);
      }
    } catch {
      // Ignore fallback failures; slots can be queried directly
    }
  }, [initialSpecialty]);

  useEffect(() => {
    if (isOpen) {
      loadInitialData();
    }
  }, [isOpen, loadInitialData]);

  const searchSlots = useCallback(async () => {
    setLoadingSlots(true);
    setApiError('');
    try {
      const filter = {};
      if (selectedDoctorId) filter.doctorId = Number(selectedDoctorId);
      if (selectedSpecialtyId) filter.specialtyId = Number(selectedSpecialtyId);
      if (selectedDate) filter.date = selectedDate;

      const availableSlots = await appointmentApi.getSlots(filter);
      const list = availableSlots || [];
      setSlots(list);
      const openSlots = list.filter((s) => s.isAvailable);
      setSelectedSlot((prev) => {
        if (prev && openSlots.some((s) => s.scheduleId === prev.scheduleId)) {
          return prev;
        }
        if (openSlots.length === 1) {
          return openSlots[0];
        }
        return null;
      });
    } catch (err) {
      setApiError(err.message || 'Failed to search channel slots.');
      setSlots([]);
    } finally {
      setLoadingSlots(false);
    }
  }, [selectedDoctorId, selectedSpecialtyId, selectedDate]);

  useEffect(() => {
    if (isOpen) {
      searchSlots();
    }
  }, [isOpen, searchSlots]);

  if (!isOpen) return null;

  const formatSlotTime = (isoString) => {
    if (!isoString) return '';
    const date = new Date(isoString);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  const handleBooking = async (e) => {
    e.preventDefault();
    setApiError('');
    setReasonError('');
    setPatientError('');

    if (!selectedPatientId) {
      setPatientError('Please select a target patient for this appointment.');
      setApiError('Please select a target patient from the dropdown above.');
      return;
    }
    if (!selectedSlot) {
      setApiError('Please click on an available session slot from the list above to select it.');
      return;
    }
    const cleanReason = reasonForVisit.trim();
    if (!cleanReason || cleanReason.length < 3) {
      setReasonError('Reason for visit must be between 3 and 500 characters.');
      return;
    }
    if (cleanReason.length > 500) {
      setReasonError('Reason for visit cannot exceed 500 characters.');
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        patientId: Number(selectedPatientId),
        doctorId: selectedSlot.doctorId,
        scheduleId: selectedSlot.scheduleId,
        appointmentDate: selectedSlot.startTime,
        reasonForVisit: cleanReason,
      };

      const result = await appointmentApi.create(payload);
      const successMsg = result?.normalizedRawSymptoms
        ? `Appointment booked. Intake symptoms: "${result.normalizedRawSymptoms}"`
        : 'Appointment successfully booked.';
      if (onBooked) {
        onBooked(result, successMsg);
      }
      onClose();
    } catch (err) {
      setApiError(err.message || 'Booking rejected by server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="pm-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="pm-modal-card large" onClick={(e) => e.stopPropagation()}>
        <form onSubmit={handleBooking} className="pm-modal-form" style={{ display: 'flex', flexDirection: 'column', height: '100%', minHeight: 0, overflow: 'hidden' }}>
          <div className="pm-modal-header" style={{ flexShrink: 0 }}>
            <div>
              <span className="eyebrow">Channel Slot Booking Lifecycle</span>
              <h2>Book Patient Appointment</h2>
            </div>
            <button type="button" className="pm-modal-close" onClick={onClose} aria-label="Close dialog">×</button>
          </div>

          <div className="pm-modal-body" style={{ overflowY: 'auto', flex: 1, minHeight: 0 }}>
            {apiError && (
              <div className="pm-alert error" role="alert">
                <span>⚠️</span> <span>{apiError}</span>
              </div>
            )}

            {/* Step 1: Patient Picker */}
            <div style={{ marginBottom: '18px' }}>
              <label className="pm-label" htmlFor="booking-patient-select">
                Target Patient <span className="required">*</span>
              </label>
              {preselectedPatient ? (
                <div style={{
                  padding: '10px 14px',
                  background: '#f1f5f9',
                  borderRadius: '8px',
                  border: '1px solid var(--line)',
                  fontSize: '13px',
                  marginTop: '4px',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}>
                  <span>
                    <strong>{preselectedPatient.name}</strong> (NIC: {preselectedPatient.nic} · {preselectedPatient.phoneNumber})
                  </span>
                  <span className="pm-badge low">Locked</span>
                </div>
              ) : (
                <>
                  <select
                    id="booking-patient-select"
                    className={`pm-select-field ${patientError ? 'error' : ''}`}
                    style={{ width: '100%', marginTop: '4px' }}
                    value={selectedPatientId}
                    onChange={(e) => {
                      setSelectedPatientId(e.target.value);
                      if (patientError) setPatientError('');
                      if (apiError) setApiError('');
                    }}
                    disabled={submitting}
                  >
                    <option value="">-- Choose registered patient --</option>
                    {patients.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name} (NIC: {p.nic}, Phone: {p.phoneNumber})
                      </option>
                    ))}
                  </select>
                  {patientError && <span className="pm-field-error">{patientError}</span>}
                </>
              )}
            </div>

            {/* Step 2: Slot Filters */}
            <div style={{
              background: '#f8fafc',
              border: '1px solid var(--line)',
              borderRadius: '12px',
              padding: '14px 16px',
              marginBottom: '18px',
            }}>
              <div style={{ fontSize: '12px', fontWeight: 700, color: 'var(--ink)', marginBottom: '10px' }}>
                🔍 Filter Available Channel Sessions
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '10px' }}>
                <div>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <label className="pm-label" style={{ fontSize: '11px' }}>Date</label>
                    {selectedDate && (
                      <button
                        type="button"
                        style={{ background: 'none', border: 'none', color: 'var(--blue)', fontSize: '10px', cursor: 'pointer', padding: 0 }}
                        onClick={() => setSelectedDate('')}
                      >
                        All Dates
                      </button>
                    )}
                  </div>
                  <input
                    type="date"
                    className="pm-input"
                    style={{ width: '100%', padding: '7px 10px' }}
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                  />
                </div>

                <div>
                  <label className="pm-label" style={{ fontSize: '11px' }}>Doctor</label>
                  <select
                    className="pm-select-field"
                    style={{ width: '100%', padding: '7px 10px' }}
                    value={selectedDoctorId}
                    onChange={(e) => setSelectedDoctorId(e.target.value)}
                  >
                    <option value="">All Doctors</option>
                    {doctors.map((d) => (
                      <option key={d.id} value={d.id}>{d.fullName || d.user?.fullName || `Doctor #${d.id}`}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="pm-label" style={{ fontSize: '11px' }}>Specialty</label>
                  <select
                    className="pm-select-field"
                    style={{ width: '100%', padding: '7px 10px' }}
                    value={selectedSpecialtyId}
                    onChange={(e) => setSelectedSpecialtyId(e.target.value)}
                  >
                    <option value="">All Specialties</option>
                    {specialties.map((s, idx) => {
                      const specName = typeof s === 'string' ? s : s.name;
                      const specId = typeof s === 'string' ? idx + 1 : s.id;
                      return <option key={specId} value={specId}>{specName}</option>;
                    })}
                  </select>
                </div>
              </div>
            </div>

            {/* Step 3: Slots Grid */}
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                <label className="pm-label">
                  Available Slots ({slots.filter((s) => s.isAvailable).length} slots open)
                </label>
                {loadingSlots && <span style={{ fontSize: '11px', color: 'var(--muted)' }}>Searching slots...</span>}
              </div>

              {loadingSlots && (
                <div className="pm-loading-box" style={{ padding: '24px' }}>
                  <div className="pm-spinner" />
                  <span>Checking room schedules and capacity...</span>
                </div>
              )}

              {!loadingSlots && slots.length === 0 && (
                <div className="pm-empty-state" style={{ padding: '28px 12px' }}>
                  <div className="icon">⏱️</div>
                  <h4>No Doctor Slots Scheduled</h4>
                  <p>There are no active doctor sessions matching the chosen date or criteria.</p>
                </div>
              )}

              {!loadingSlots && slots.length > 0 && (
                <div className="pm-slots-grid" style={{ maxHeight: '220px', overflowY: 'auto' }}>
                  {slots.map((slot) => {
                    const isSelected = selectedSlot?.scheduleId === slot.scheduleId;
                    const canBook = slot.isAvailable;
                    return (
                      <div
                        key={slot.scheduleId}
                        className={`pm-slot-card ${isSelected ? 'selected' : ''} ${!canBook ? 'disabled' : ''}`}
                        onClick={() => {
                          if (canBook) {
                            setSelectedSlot(slot);
                            setApiError('');
                          }
                        }}
                        role="button"
                        tabIndex={canBook ? 0 : -1}
                        onKeyDown={(e) => {
                          if (canBook && (e.key === 'Enter' || e.key === ' ')) {
                            e.preventDefault();
                            setSelectedSlot(slot);
                            setApiError('');
                          }
                        }}
                      >
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                          <span className="pm-slot-doctor">{slot.doctorName}</span>
                          <span className={`pm-badge ${canBook ? 'confirmed' : 'cancelled'}`} style={{ fontSize: '10px', padding: '2px 7px' }}>
                            {canBook ? `${slot.availableSlots} Left` : 'Full'}
                          </span>
                        </div>
                        <span className="pm-slot-specialty">{slot.specialtyName}</span>
                        <div style={{ fontSize: '12px', color: 'var(--ink)' }}>
                          🕒 {formatSlotTime(slot.startTime)} – {formatSlotTime(slot.endTime)}
                        </div>
                        <div className="pm-slot-meta">
                          <span>📍 {slot.roomName} ({slot.roomFloor})</span>
                          <span>Max: {slot.maxPatients}</span>
                        </div>

                        {canBook && (
                          <div className={`pm-slot-select-action ${isSelected ? 'selected' : ''}`}>
                            <span className="pm-radio-circle">{isSelected ? '●' : '○'}</span>
                            <span>{isSelected ? '✓ Selected Slot' : 'Click to Select Slot'}</span>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}

              {selectedSlot && (
                <div className="pm-selected-slot-banner">
                  <span className="icon">✓</span>
                  <div>
                    <strong>Selected Session:</strong> {selectedSlot.doctorName} ({selectedSlot.specialtyName}) · {formatSlotTime(selectedSlot.startTime)}–{formatSlotTime(selectedSlot.endTime)} · {selectedSlot.roomName}
                  </div>
                </div>
              )}
            </div>

            {/* Step 4: Reason for Visit */}
            <div style={{ marginTop: '18px' }}>
              <label className="pm-label" htmlFor="booking-reason">
                Reason for Visit <span className="required">*</span>
              </label>
              <p style={{ fontSize: '12px', color: 'var(--muted)', marginTop: '2px', marginBottom: '6px' }}>
                Describe your reason for visiting in your own words.
              </p>
              <textarea
                id="booking-reason"
                className={`pm-textarea ${reasonError ? 'error' : ''}`}
                style={{ width: '100%', marginTop: '4px' }}
                rows={3}
                placeholder="Describe your reason for visiting in your own words (e.g. I have a bad headache since yesterday and feel dizzy)"
                value={reasonForVisit}
                maxLength={500}
                onChange={(e) => {
                  setReasonForVisit(e.target.value);
                  if (reasonError) setReasonError('');
                }}
                disabled={submitting}
              />
              {reasonError && <span className="pm-field-error">{reasonError}</span>}
            </div>
          </div>

          {apiError && (
            <div style={{ padding: '6px 28px 0', background: '#fafbfd', flexShrink: 0 }}>
              <div className="pm-alert error" style={{ margin: 0, padding: '7px 12px', fontSize: '12px' }} role="alert">
                <span>⚠️</span> <span>{apiError}</span>
              </div>
            </div>
          )}

          <div className="pm-modal-footer" style={{ flexShrink: 0 }}>
            <button
              type="button"
              className="outline-button"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                onClose();
              }}
              disabled={submitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="primary-button"
              disabled={submitting}
            >
              {submitting ? 'Confirming Booking...' : 'Confirm Appointment'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
