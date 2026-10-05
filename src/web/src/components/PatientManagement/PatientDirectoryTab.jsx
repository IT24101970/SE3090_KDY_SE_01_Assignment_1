import { useState, useEffect, useCallback } from 'react';
import { patientApi } from '../../api/channelCenterApi';
import PatientFormModal from './PatientFormModal';
import PatientDetailModal from './PatientDetailModal';
import SlotBookingModal from './SlotBookingModal';
import './PatientManagement.css';

export default function PatientDirectoryTab({ onLaunchIntake }) {
  const [patients, setPatients] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [feedback, setFeedback] = useState({ message: '', type: '' });

  // Filters & Pagination
  const [searchTerm, setSearchTerm] = useState('');
  const [genderFilter, setGenderFilter] = useState('');
  const [bloodGroupFilter, setBloodGroupFilter] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  // Modals state
  const [formModalOpen, setFormModalOpen] = useState(false);
  const [editingPatient, setEditingPatient] = useState(null);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [activePatient, setActivePatient] = useState(null);
  const [slotBookingOpen, setSlotBookingOpen] = useState(false);
  const [bookingPatient, setBookingPatient] = useState(null);

  const fetchPatients = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await patientApi.list({
        searchTerm: searchTerm.trim(),
        gender: genderFilter,
        bloodGroup: bloodGroupFilter,
        page,
        pageSize,
      });
      setPatients(res.items || []);
      setTotalCount(res.totalCount || 0);
    } catch (err) {
      setError(err.message || 'Failed to load patients list.');
      setPatients([]);
    } finally {
      setLoading(false);
    }
  }, [searchTerm, genderFilter, bloodGroupFilter, page, pageSize]);

  useEffect(() => {
    fetchPatients();
  }, [fetchPatients]);

  const showNotification = (msg, type = 'success') => {
    setFeedback({ message: msg, type });
    setTimeout(() => {
      setFeedback({ message: '', type: '' });
    }, 4500);
  };

  const handleOpenCreate = () => {
    setEditingPatient(null);
    setFormModalOpen(true);
  };

  const handleOpenEdit = (pat) => {
    setEditingPatient(pat);
    setFormModalOpen(true);
  };

  const handleOpenDetail = (pat) => {
    setActivePatient(pat);
    setDetailModalOpen(true);
  };

  const handleOpenBooking = (pat) => {
    setBookingPatient(pat);
    setSlotBookingOpen(true);
  };

  const handleDelete = async (pat) => {
    if (!window.confirm(`Are you sure you want to delete patient "${pat.name}"? This action cannot be undone.`)) {
      return;
    }
    try {
      await patientApi.delete(pat.id);
      showNotification(`Patient "${pat.name}" deleted successfully.`);
      fetchPatients();
    } catch (err) {
      showNotification(err.message || 'Failed to delete patient. Ensure there are no active appointments.', 'error');
    }
  };

  const handlePatientSaved = (savedPatient, message) => {
    showNotification(message);
    fetchPatients();
  };

  const handleAppointmentBooked = (appointment, message) => {
    showNotification(message);
    fetchPatients();
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="pm-container">
      {/* Page Header */}
      <div className="pm-header">
        <div className="pm-header-titles">
          <p className="eyebrow">Component 01 · Patient Management</p>
          <h1>Patient Directory & Onboarding</h1>
          <p>Search, manage, and onboard patients into the ChannelCenter clinical workflow.</p>
        </div>
        <div className="pm-header-actions">
          <button className="primary-button" onClick={handleOpenCreate}>
            + Register New Patient
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
          <span className="pm-stat-label">Total Registered</span>
          <span className="pm-stat-value">{totalCount}</span>
          <span className="pm-stat-hint">Active records in database</span>
        </div>
        <div className="pm-stat-card">
          <span className="pm-stat-label">Current View</span>
          <span className="pm-stat-value">{patients.length}</span>
          <span className="pm-stat-hint">Patients on page {page} of {totalPages}</span>
        </div>
        <div className="pm-stat-card">
          <span className="pm-stat-label">System Status</span>
          <span className="pm-stat-value" style={{ fontSize: '18px', color: '#13a68a' }}>✓ Synchronized</span>
          <span className="pm-stat-hint">Connected to ASP.NET Core API</span>
        </div>
      </div>

      {/* Toolbar / Search / Filter */}
      <div className="pm-toolbar">
        <div className="pm-toolbar-left">
          <input
            type="text"
            className="pm-search-input"
            placeholder="Search by name, NIC, or email..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPage(1);
            }}
          />

          <select
            className="pm-select"
            value={genderFilter}
            onChange={(e) => {
              setGenderFilter(e.target.value);
              setPage(1);
            }}
          >
            <option value="">All Genders</option>
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>

          <select
            className="pm-select"
            value={bloodGroupFilter}
            onChange={(e) => {
              setBloodGroupFilter(e.target.value);
              setPage(1);
            }}
          >
            <option value="">All Blood Groups</option>
            <option value="O+">O+</option>
            <option value="O-">O-</option>
            <option value="A+">A+</option>
            <option value="A-">A-</option>
            <option value="B+">B+</option>
            <option value="B-">B-</option>
            <option value="AB+">AB+</option>
            <option value="AB-">AB-</option>
          </select>

          {(searchTerm || genderFilter || bloodGroupFilter) && (
            <button
              className="pm-btn-sm outline"
              onClick={() => {
                setSearchTerm('');
                setGenderFilter('');
                setBloodGroupFilter('');
                setPage(1);
              }}
            >
              Reset Filters
            </button>
          )}
        </div>

        <div className="pm-toolbar-right">
          <button
            className="pm-btn-sm outline"
            onClick={fetchPatients}
            disabled={loading}
            title="Refresh patient list"
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
            <span>Loading patient records...</span>
          </div>
        )}

        {error && !loading && (
          <div className="pm-alert error" style={{ margin: '20px' }}>
            <span>⚠️</span> <span>{error}</span>
            <button className="pm-btn-sm outline" style={{ marginLeft: 'auto' }} onClick={fetchPatients}>
              Try Again
            </button>
          </div>
        )}

        {!loading && !error && patients.length === 0 && (
          <div className="pm-empty-state">
            <div className="icon">👥</div>
            <h3>No Patients Found</h3>
            <p>
              {searchTerm || genderFilter || bloodGroupFilter
                ? 'No patient records match the applied search and filter criteria.'
                : 'No patients are currently registered in the hospital system.'}
            </p>
            <button className="primary-button" onClick={handleOpenCreate}>
              + Register First Patient
            </button>
          </div>
        )}

        {!loading && !error && patients.length > 0 && (
          <div className="pm-table-wrapper">
            <table className="pm-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Patient Name & Age</th>
                  <th>National ID (NIC)</th>
                  <th>Contact Info</th>
                  <th>Blood & Gender</th>
                  <th>Emergency Contact</th>
                  <th>Bookings</th>
                  <th style={{ textAlign: 'right' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {patients.map((pat) => (
                  <tr key={pat.id}>
                    <td style={{ fontWeight: 600 }}>#{pat.id}</td>
                    <td>
                      <strong style={{ fontSize: '13px', color: 'var(--ink)' }}>{pat.name}</strong>
                      <div style={{ fontSize: '11px', color: 'var(--muted)' }}>
                        {pat.age} years old
                      </div>
                    </td>
                    <td>
                      <code style={{ background: '#f1f5f9', padding: '2px 6px', borderRadius: '4px' }}>
                        {pat.nic}
                      </code>
                    </td>
                    <td>
                      <div>{pat.phoneNumber}</div>
                      <div style={{ fontSize: '11px', color: 'var(--muted)' }}>{pat.email}</div>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: '4px', alignItems: 'center' }}>
                        <span className="pm-badge low">{pat.bloodGroup || '—'}</span>
                        <span style={{ fontSize: '12px', color: 'var(--muted)' }}>{pat.gender}</span>
                      </div>
                    </td>
                    <td>
                      <span style={{ fontSize: '12px' }}>{pat.emergencyContact}</span>
                    </td>
                    <td>
                      <span className="pm-badge completed">
                        {pat.totalAppointments || 0} visits
                      </span>
                    </td>
                    <td>
                      <div className="pm-actions-group" style={{ justifyContent: 'flex-end' }}>
                        <button
                          className="pm-btn-sm outline"
                          onClick={() => handleOpenDetail(pat)}
                          title="View complete profile and history"
                        >
                          View
                        </button>
                        <button
                          className="pm-btn-sm outline"
                          onClick={() => handleOpenEdit(pat)}
                          title="Edit demographics"
                        >
                          Edit
                        </button>
                        <button
                          className="pm-btn-sm primary"
                          onClick={() => handleOpenBooking(pat)}
                          title="Book an appointment for this patient"
                        >
                          📅 Book
                        </button>
                        {onLaunchIntake && (
                          <button
                            className="pm-btn-sm success"
                            onClick={() => onLaunchIntake(pat)}
                            title="Open AI Intake Agent for this patient"
                          >
                            ⚡ Intake
                          </button>
                        )}
                        <button
                          className="pm-btn-sm danger"
                          onClick={() => handleDelete(pat)}
                          title="Delete patient record"
                        >
                          ✕
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
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
      <PatientFormModal
        isOpen={formModalOpen}
        patient={editingPatient}
        onClose={() => setFormModalOpen(false)}
        onSaved={handlePatientSaved}
      />

      <PatientDetailModal
        isOpen={detailModalOpen}
        patient={activePatient}
        onClose={() => setDetailModalOpen(false)}
        onEdit={(pat) => {
          setDetailModalOpen(false);
          handleOpenEdit(pat);
        }}
        onBookAppointment={(pat) => {
          setDetailModalOpen(false);
          handleOpenBooking(pat);
        }}
        onLaunchIntake={(pat) => {
          setDetailModalOpen(false);
          if (onLaunchIntake) onLaunchIntake(pat);
        }}
      />

      <SlotBookingModal
        isOpen={slotBookingOpen}
        preselectedPatient={bookingPatient}
        onClose={() => setSlotBookingOpen(false)}
        onBooked={handleAppointmentBooked}
      />
    </div>
  );
}
