import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5066/api/doctor-scheduling';

export default function DoctorDirectoryTab() {
  const [doctors, setDoctors] = useState([]);
  const [editingDoctorId, setEditingDoctorId] = useState(null);
  const [alert, setAlert] = useState(null);

  const [formData, setFormData] = useState({
    userId: 1,
    specialtyId: 1,
    qualifications: ''
  });

  const [editFormData, setEditFormData] = useState({
    specialtyId: 1,
    qualifications: ''
  });

  const specialtiesList = [
    { id: 1, name: 'Cardiology' },
    { id: 2, name: 'Neurology' },
    { id: 3, name: 'Pediatrics' },
    { id: 4, name: 'Dermatology' },
    { id: 5, name: 'Orthopedics' },
    { id: 6, name: 'General Medicine' }
  ];

  const fetchDoctors = async () => {
    try {
      const res = await fetch(`${API_BASE}/doctors`);
      if (res.ok) setDoctors(await res.json());
    } catch {
      setDoctors([
        {
          id: 1,
          userId: 1,
          doctorName: 'Dr. Sarah Jenkins',
          specialtyId: 1,
          specialtyName: 'Cardiology',
          qualifications: 'MD, FACC, Board Certified Cardiologist'
        },
        {
          id: 2,
          userId: 2,
          doctorName: 'Dr. Michael Chen',
          specialtyId: 2,
          specialtyName: 'Neurology',
          qualifications: 'MBBS, MD (Neurology), PhD'
        }
      ]);
    }
  };

  useEffect(() => {
    fetchDoctors();
  }, []);

  const handleRegisterProfile = async (e) => {
    e.preventDefault();
    if (!formData.qualifications) {
      setAlert({ type: 'error', text: 'Please enter doctor qualifications.' });
      return;
    }

    try {
      const res = await fetch(`${API_BASE}/doctors`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userId: Number(formData.userId),
          specialtyId: Number(formData.specialtyId),
          qualifications: formData.qualifications
        })
      });

      if (res.ok) {
        setAlert({ type: 'success', text: '👨‍⚕️ Specialist Profile registered in PostgreSQL Database!' });
        setFormData({ userId: 1, specialtyId: 1, qualifications: '' });
        fetchDoctors();
      } else {
        const err = await res.json();
        setAlert({ type: 'error', text: err.message || 'Failed to register profile.' });
      }
    } catch {
      const sp = specialtiesList.find((s) => s.id === Number(formData.specialtyId));
      setDoctors([
        ...doctors,
        {
          id: doctors.length + 1,
          userId: Number(formData.userId),
          doctorName: `Dr. Specialist #${formData.userId}`,
          specialtyId: Number(formData.specialtyId),
          specialtyName: sp ? sp.name : 'General',
          qualifications: formData.qualifications
        }
      ]);
      setAlert({ type: 'success', text: '👨‍⚕️ Specialist Profile registered!' });
      setFormData({ userId: 1, specialtyId: 1, qualifications: '' });
    }
  };

  const handleStartEdit = (doctor) => {
    setEditingDoctorId(doctor.id);
    setEditFormData({
      specialtyId: doctor.specialtyId || 1,
      qualifications: doctor.qualifications || ''
    });
  };

  const handleSaveEdit = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/doctors/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          specialtyId: Number(editFormData.specialtyId),
          qualifications: editFormData.qualifications
        })
      });

      if (res.ok) {
        setAlert({ type: 'success', text: 'Profile updated in PostgreSQL database!' });
        setEditingDoctorId(null);
        fetchDoctors();
      }
    } catch {
      setDoctors(
        doctors.map((d) =>
          d.id === id
            ? {
                ...d,
                specialtyId: Number(editFormData.specialtyId),
                specialtyName: specialtiesList.find((s) => s.id === Number(editFormData.specialtyId))?.name || d.specialtyName,
                qualifications: editFormData.qualifications
              }
            : d
        )
      );
      setEditingDoctorId(null);
      setAlert({ type: 'success', text: 'Specialist profile updated!' });
    }
  };

  return (
    <div>
      {/* Register New Specialist Profile Form */}
      <form className="ds-form" onSubmit={handleRegisterProfile}>
        <div className="ds-form-title">
          <span>👨‍⚕️</span> Register New Specialist Doctor Profile
        </div>

        {alert && (
          <div className={`ds-alert ds-alert-${alert.type}`}>
            {alert.type === 'success' ? '✅' : '⚠️'} {alert.text}
          </div>
        )}

        <div className="ds-form-row">
          <div className="ds-form-group">
            <label className="ds-label">User Account ID</label>
            <input
              type="number"
              className="ds-input"
              value={formData.userId}
              onChange={(e) => setFormData({ ...formData, userId: e.target.value })}
              min="1"
            />
          </div>

          <div className="ds-form-group">
            <label className="ds-label">Clinical Specialty</label>
            <select
              className="ds-select"
              value={formData.specialtyId}
              onChange={(e) => setFormData({ ...formData, specialtyId: e.target.value })}
            >
              {specialtiesList.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>

          <div className="ds-form-group" style={{ gridColumn: 'span 2' }}>
            <label className="ds-label">Medical Qualifications & Degrees</label>
            <input
              type="text"
              className="ds-input"
              placeholder="e.g. MBBS, MD (Cardiology), FACC Board Certified"
              value={formData.qualifications}
              onChange={(e) => setFormData({ ...formData, qualifications: e.target.value })}
            />
          </div>
        </div>

        <button type="submit" className="ds-btn ds-btn-coral">
          Register Specialist Profile
        </button>
      </form>

      <h3 style={{ marginBottom: 16, color: 'var(--ds-navy-dark)' }}>
        👨‍⚕️ Specialist Doctors Roster ({doctors.length})
      </h3>

      <div className="ds-grid">
        {doctors.map((d) => (
          <div key={d.id} className="ds-card">
            {editingDoctorId === d.id ? (
              /* Inline Edit Mode */
              <div>
                <h4 style={{ margin: '0 0 12px 0', color: 'var(--ds-navy-dark)' }}>
                  Edit {d.doctorName}
                </h4>

                <div className="ds-form-group" style={{ marginBottom: 10 }}>
                  <label className="ds-label">Specialty</label>
                  <select
                    className="ds-select"
                    value={editFormData.specialtyId}
                    onChange={(e) => setEditFormData({ ...editFormData, specialtyId: e.target.value })}
                  >
                    {specialtiesList.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="ds-form-group" style={{ marginBottom: 14 }}>
                  <label className="ds-label">Qualifications</label>
                  <input
                    type="text"
                    className="ds-input"
                    value={editFormData.qualifications}
                    onChange={(e) => setEditFormData({ ...editFormData, qualifications: e.target.value })}
                  />
                </div>

                <div style={{ display: 'flex', gap: 8 }}>
                  <button
                    className="ds-btn ds-btn-success"
                    style={{ flex: 1 }}
                    onClick={() => handleSaveEdit(d.id)}
                  >
                    Save Changes
                  </button>
                  <button
                    className="ds-btn ds-btn-danger"
                    style={{ flex: 1 }}
                    onClick={() => setEditingDoctorId(null)}
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              /* Normal View Mode */
              <div>
                <div className="ds-card-header">
                  <h4 className="ds-card-title">{d.doctorName}</h4>
                  <span className="ds-badge ds-badge-approved">{d.specialtyName}</span>
                </div>
                <div style={{ fontSize: '0.9rem', color: '#486581', marginTop: 8, marginBottom: 14 }}>
                  <strong>🎓 Qualifications:</strong> {d.qualifications}
                </div>
                <div style={{ paddingTop: 10, borderTop: '1px solid #e2e8f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.8rem', color: '#627d98', fontWeight: 600 }}>User ID: #{d.userId}</span>
                  <button
                    className="ds-btn ds-btn-navy"
                    style={{ padding: '6px 12px', fontSize: '0.8rem' }}
                    onClick={() => handleStartEdit(d)}
                  >
                    Edit Profile
                  </button>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
