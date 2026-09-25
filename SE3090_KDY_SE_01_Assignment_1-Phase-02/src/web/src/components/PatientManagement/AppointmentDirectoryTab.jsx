import { useState, useEffect, useCallback } from 'react';
import { appointmentApi } from '../../api/channelCenterApi';
import SlotBookingModal from './SlotBookingModal';
import AppointmentCancelModal from './AppointmentCancelModal';
import './PatientManagement.css';

export default function AppointmentDirectoryTab() {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [feedback, setFeedback] = useState({ message: '', type: '' });

  // Filters
  const [statusFilter, setStatusFilter] = useState('');
  const [startDateFilter, setStartDateFilter] = useState('');
  const [upcomingOnly, setUpcomingOnly] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  // Modals
  const [slotBookingOpen, setSlotBookingOpen] = useState(false);
  const [cancelModalOpen, setCancelModalOpen] = useState(false);
  const [targetAppointment, setTargetAppointment] = useState(null);

  const fetchAppointments = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const filter = {
        page,
        pageSize,
      };
      if (statusFilter) filter.status = statusFilter;
      if (startDateFilter) filter.startDate = startDateFilter;
      if (upcomingOnly) filter.upcomingOnly = true;

      const res = await appointmentApi.list(filter);
      setAppointments(res.items || []);
      setTotalCount(res.totalCount || 0);
    } catch (err) {
      setError(err.message || 'Failed to load appointments.');
      setAppointments([]);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, statusFilter, startDateFilter, upcomingOnly]);

  useEffect(() => {
    fetchAppointments();
  }, [fetchAppointments]);

  const showNotification = (msg, type = 'success') => {
    setFeedback({ message: msg, type });
    setTimeout(() => {
      setFeedback({ message: '', type: '' });
    }, 4500);
  };

  const handleUpdateStatus = async (appt, newStatus) => {
    try {
      await appointmentApi.updateStatus(appt.id, { status: newStatus });
      showNotification(`Appointment #${appt.id} status transitioned to "${newStatus}".`);
      fetchAppointments();
    } catch (err) {
      showNotification(err.message || `Failed to transition appointment to ${newStatus}.`, 'error');
    }
  };

  const handleOpenCancel = (appt) => {
    setTargetAppointment(appt);
    setCancelModalOpen(true);
  };

  const handleAppointmentCancelled = (updated, message) => {
    showNotification(message);
    fetchAppointments();
  };

  const handleAppointmentBooked = (created, message) => {
    showNotification(message);
    fetchAppointments();
  };

  const formatDate = (isoString) => {
    if (!isoString) return '—';
    const d = new Date(isoString);
    return d.toLocaleDateString(undefined, {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  // Compute status counts for KPI overview
  const confirmedCount = appointments.filter((a) => a.status === 'Confirmed').length;
  const pendingCount = appointments.filter((a) => a.status === 'Pending').length;
  const cancelledCount = appointments.filter((a) => a.status === 'Cancelled').length;

  return (
    <div className="pm-container">
      {/* Header */}
      <div className="pm-header">
        <div className="pm-header-titles">
          <p className="eyebrow">Component 01 · Lifecycle Management</p>
          <h1>Appointment Management & Slot Roster</h1>
          <p>Supervise booking states, enforce transition rules, search available channel slots, and handle cancellations.</p>
        </div>
        <div className="pm-header-actions">
          <button className="primary-button" onClick={() => setSlotBookingOpen(true)}>
            + Book New Appointment
          </button>
        </div>
      </div>

      {/* Notification Toast */}
      {feedback.message && (
        <div className={`pm-alert ${feedback.type === 'error' ? 'error' : 'success'}`} role="status">
          <span>{feedback.type === 'error' ? '⚠️' : '✓'}</span>
          <span>{feedback.message}</span>
        </div>
      )}

      {/* KPI Stats */}
      <div className="pm-stats-grid">
        <div className="pm-stat-card">
          <span className="pm-stat-label">Total Bookings</span>
          <span className="pm-stat-value">{totalCount}</span>
          <span className="pm-stat-hint">Across all medical specialties</span>
        </div>
        <div className="pm-stat-card">
          <span className="pm-stat-label">Confirmed on View</span>
          <span className="pm-stat-value" style={{ color: '#13a68a' }}>{confirmedCount}</span>
          <span className="pm-stat-hint">Ready for doctor consultation</span>
        </div>
        <div className="pm-stat-card">
          <span className="pm-stat-label">Pending Approval</span>
          <span className="pm-stat-value" style={{ color: '#e5a82f' }}>{pendingCount}</span>
          <span className="pm-stat-hint">Awaiting confirmation</span>
        </div>
        <div className="pm-stat-card">
          <span className="pm-stat-label">Cancelled Visits</span>
          <span className="pm-stat-value" style={{ color: '#d9535d' }}>{cancelledCount}</span>
          <span className="pm-stat-hint">With audited cancellation reasons</span>
        </div>
      </div>

      {/* Filter Toolbar */}
      <div className="pm-toolbar">
        <div className="pm-toolbar-left">
          {/* Status Tab Filter */}
          <div className="pm-tab-pills">
            {['', 'Pending', 'Confirmed', 'Completed', 'Cancelled'].map((st) => (
              <button
                key={st || 'all'}
                className={`pm-tab-pill ${statusFilter === st ? 'active' : ''}`}
                onClick={() => {
                  setStatusFilter(st);
                  setPage(1);
                }}
              >
                {st || 'All Bookings'}
              </button>
            ))}
          </div>

          {/* Date Picker */}
          <input
            type="date"
            className="pm-input"
            style={{ padding: '7px 10px', fontSize: '12px' }}
            value={startDateFilter}
            onChange={(e) => {
              setStartDateFilter(e.target.value);
              setPage(1);
            }}
            title="Filter by appointment date"
          />

          {/* Upcoming Only Toggle */}
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', cursor: 'pointer', color: 'var(--ink)' }}>
            <input
              type="checkbox"
              checked={upcomingOnly}
              onChange={(e) => {
                setUpcomingOnly(e.target.checked);
                setPage(1);
              }}
            />
            <span>Upcoming Only</span>
          </label>

          {(statusFilter || startDateFilter || upcomingOnly) && (
            <button
              className="pm-btn-sm outline"
              onClick={() => {
                setStatusFilter('');
                setStartDateFilter('');
                setUpcomingOnly(false);
                setPage(1);
              }}
            >
              Clear Filters
            </button>
          )}
        </div>

        <div className="pm-toolbar-right">
          <button
            className="pm-btn-sm outline"
            onClick={fetchAppointments}
            disabled={loading}
          >
            {loading ? 'Refreshing...' : '↻ Refresh'}
          </button>
        </div>
      </div>

      {/* Main Table Card */}
      <div className="pm-card">
        {loading && (
          <div className="pm-loading-box">
            <div className="pm-spinner" />
            <span>Retrieving appointment records...</span>
          </div>
        )}

        {error && !loading && (
          <div className="pm-alert error" style={{ margin: '20px' }}>
            <span>⚠️</span> <span>{error}</span>
            <button className="pm-btn-sm outline" style={{ marginLeft: 'auto' }} onClick={fetchAppointments}>
              Try Again
            </button>
          </div>
        )}

        {!loading && !error && appointments.length === 0 && (
          <div className="pm-empty-state">
            <div className="icon">📅</div>
            <h3>No Appointments Found</h3>
            <p>
              {statusFilter || startDateFilter || upcomingOnly
                ? 'No appointments match the applied status or date filters.'
                : 'No appointments are currently scheduled.'}
            </p>
            <button className="primary-button" onClick={() => setSlotBookingOpen(true)}>
              + Book an Appointment
            </button>
          </div>
        )}

        {!loading && !error && appointments.length > 0 && (
          <div className="pm-table-wrapper">
            <table className="pm-table">
              <thead>
                <tr>
                  <th>Booking ID</th>
                  <th>Appointment Time</th>
                  <th>Patient Details</th>
                  <th>Assigned Doctor & Specialty</th>
                  <th>Location</th>
                  <th>Reason for Visit</th>
                  <th>Status</th>
                  <th style={{ textAlign: 'right' }}>Lifecycle Actions</th>
                </tr>
              </thead>
              <tbody>
                {appointments.map((appt) => {
                  const statusStr = String(appt.status || '');
                  return (
                    <tr key={appt.id}>
                      <td style={{ fontWeight: 600 }}>#{appt.id}</td>
                      <td style={{ whiteSpace: 'nowrap' }}>
                        <strong style={{ fontSize: '13px', color: 'var(--ink)' }}>{formatDate(appt.appointmentDate)}</strong>
                      </td>
                      <td>
                        <strong>{appt.patientName}</strong>
                        <div style={{ fontSize: '11px', color: 'var(--muted)' }}>
                          NIC: {appt.patientNIC} · {appt.patientPhone}
                        </div>
                      </td>
                      <td>
                        <strong style={{ color: 'var(--ink)' }}>{appt.doctorName || 'Dr. Assigned'}</strong>
                        <div style={{ fontSize: '11px', color: 'var(--blue)' }}>{appt.doctorSpecialty || 'General'}</div>
                      </td>
                      <td>
                        <div>{appt.roomName || 'Room 101'}</div>
                        <div style={{ fontSize: '11px', color: 'var(--muted)' }}>{appt.roomFloor || 'Floor 1'}</div>
                      </td>
                      <td style={{ maxWidth: '200px' }}>
                        <span style={{ fontSize: '12px' }}>{appt.reasonForVisit}</span>
                        {appt.cancelReason && (
                          <div style={{ fontSize: '11px', color: 'var(--red)', marginTop: '3px' }}>
                            <em>Cancellation: "{appt.cancelReason}"</em>
                          </div>
                        )}
                      </td>
                      <td>
                        <span className={`pm-badge ${statusStr.toLowerCase()}`}>
                          {statusStr}
                        </span>
                      </td>
                      <td>
                        <div className="pm-actions-group" style={{ justifyContent: 'flex-end' }}>
                          {/* Valid Transitions based on backend rules:
                              Pending -> Confirmed
                              Confirmed -> Completed
                              Pending or Confirmed -> Cancelled
                          */}
                          {statusStr === 'Pending' && (
                            <button
                              className="pm-btn-sm success"
                              onClick={() => handleUpdateStatus(appt, 'Confirmed')}
                              title="Transition state from Pending to Confirmed"
                            >
                              ✓ Confirm
                            </button>
                          )}

                          {statusStr === 'Confirmed' && (
                            <button
                              className="pm-btn-sm primary"
                              onClick={() => handleUpdateStatus(appt, 'Completed')}
                              title="Mark consultation as completed"
                            >
                              ✓ Complete
                            </button>
                          )}

                          {(statusStr === 'Pending' || statusStr === 'Confirmed') && (
                            <button
                              className="pm-btn-sm danger"
                              onClick={() => handleOpenCancel(appt)}
                              title="Cancel booking with required reason"
                            >
                              Cancel
                            </button>
                          )}

                          {statusStr === 'Completed' && (
                            <span style={{ fontSize: '11px', color: 'var(--muted)', fontWeight: 600 }}>
                              Archived
                            </span>
                          )}

                          {statusStr === 'Cancelled' && (
                            <span style={{ fontSize: '11px', color: 'var(--red)', fontWeight: 600 }}>
                              Cancelled
                            </span>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination Footer */}
        {!loading && totalCount > 0 && (
          <div className="pm-pagination">
            <span>
              Showing {((page - 1) * pageSize) + 1}–{Math.min(page * pageSize, totalCount)} of {totalCount} records
            </span>
            <div className="pm-pagination-nav">
              <button
                className="pm-btn-sm outline"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                ← Previous
              </button>
              <span style={{ margin: '0 8px', fontWeight: 600 }}>
                Page {page} of {totalPages}
              </span>
              <button
                className="pm-btn-sm outline"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              >
                Next →
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Modals */}
      <SlotBookingModal
        isOpen={slotBookingOpen}
        onClose={() => setSlotBookingOpen(false)}
        onBooked={handleAppointmentBooked}
      />

      <AppointmentCancelModal
        isOpen={cancelModalOpen}
        appointment={targetAppointment}
        onClose={() => setCancelModalOpen(false)}
        onCancelled={handleAppointmentCancelled}
      />
    </div>
  );
}
