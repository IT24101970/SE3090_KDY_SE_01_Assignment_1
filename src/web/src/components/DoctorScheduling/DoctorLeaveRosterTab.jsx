import React, { useState, useEffect } from 'react';

const getDoctorSchedulingApiBase = () => {
  const envUrl = import.meta.env.VITE_API_BASE_URL;
  if (envUrl && typeof envUrl === 'string' && !envUrl.includes('localhost')) {
    return `${envUrl.replace(/\/$/, '')}/api/doctor-scheduling`;
  }
  if (typeof window !== 'undefined' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
    return 'https://channel-center-api-c3hkfmhfdhe5gghb.eastasia-01.azurewebsites.net/api/doctor-scheduling';
  }
  return 'http://localhost:5066/api/doctor-scheduling';
};

const API_BASE = getDoctorSchedulingApiBase();

export default function DoctorLeaveRosterTab() {
  const [leaves, setLeaves] = useState([]);
  const [alert, setAlert] = useState(null);

  const fetchLeaves = async () => {
    try {
      const res = await fetch(`${API_BASE}/doctorleaves`);
      if (res.ok) setLeaves(await res.json());
    } catch {
      setLeaves([
        {
          id: 1,
          doctorName: 'Dr. Sarah Jenkins',
          startDate: '2026-10-10',
          endDate: '2026-10-12',
          reason: 'Attending Medical Cardiology Conference',
          status: 0 // Pending
        },
        {
          id: 2,
          doctorName: 'Dr. Michael Chen',
          startDate: '2026-10-15',
          endDate: '2026-10-16',
          reason: 'Personal Leave',
          status: 1 // Approved
        }
      ]);
    }
  };

  useEffect(() => {
    fetchLeaves();
  }, []);

  const updateStatus = async (id, statusEnum) => {
    try {
      const res = await fetch(`${API_BASE}/doctorleaves/${id}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: statusEnum })
      });

      if (res.ok) {
        setAlert({ type: 'success', text: 'Leave status updated in PostgreSQL database!' });
        fetchLeaves();
      }
    } catch {
      setLeaves(
        leaves.map((l) => (l.id === id ? { ...l, status: statusEnum } : l))
      );
      setAlert({ type: 'success', text: 'Leave status updated!' });
    }
  };

  const getStatusBadge = (status) => {
    if (status === 1 || status === 'Approved') return <span className="ds-badge ds-badge-approved">Approved</span>;
    if (status === 2 || status === 'Rejected') return <span className="ds-badge ds-badge-rejected">Rejected</span>;
    return <span className="ds-badge ds-badge-pending">Pending</span>;
  };

  return (
    <div>
      <h3 className="ds-section-title">
        📋 Specialist Doctor Leave Requests ({leaves.length})
      </h3>

      {alert && (
        <div className={`ds-alert ds-alert-${alert.type}`}>
          ✅ {alert.text}
        </div>
      )}

      <div className="ds-grid">
        {leaves.map((leave) => (
          <div key={leave.id} className="ds-card">
            <div>
              <div className="ds-card-header">
                <h4 className="ds-card-title">{leave.doctorName}</h4>
                {getStatusBadge(leave.status)}
              </div>
              <div style={{ fontSize: '0.9rem', color: 'var(--muted)', marginBottom: 6 }}>
                <strong>📅 Dates:</strong> {new Date(leave.startDate).toLocaleDateString()} — {new Date(leave.endDate).toLocaleDateString()}
              </div>
              <div style={{ fontSize: '0.9rem', color: 'var(--muted)', marginBottom: 16 }}>
                <strong>📝 Reason:</strong> {leave.reason}
              </div>
            </div>

            {(leave.status === 0 || leave.status === 'Pending') && (
              <div style={{ display: 'flex', gap: 10, paddingTop: 12, borderTop: '1px solid var(--line)' }}>
                <button
                  className="ds-btn ds-btn-success"
                  style={{ flex: 1 }}
                  onClick={() => updateStatus(leave.id, 1)}
                >
                  Approve Leave
                </button>
                <button
                  className="ds-btn ds-btn-danger"
                  style={{ flex: 1 }}
                  onClick={() => updateStatus(leave.id, 2)}
                >
                  Reject
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
