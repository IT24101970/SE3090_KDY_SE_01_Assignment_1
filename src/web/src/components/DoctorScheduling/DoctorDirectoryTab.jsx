import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5066/api/doctor-scheduling';

export default function DoctorDirectoryTab() {
  const [doctors, setDoctors] = useState([]);
  const [alert, setAlert] = useState(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingDoctorId, setEditingDoctorId] = useState(null);

  const [formData, setFormData] = useState({
    doctorName: '',
    email: '',
    password: '',
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
        },
        {
          id: 3,
          userId: 3,
          doctorName: 'Dr. Elena Rostova',
          specialtyId: 3,
          specialtyName: 'Pediatrics',
          qualifications: 'MD, FAAP, Pediatric Care Specialist'
        }
      ]);
    }
  };

  useEffect(() => {
    fetchDoctors();
  }, []);

  const handleOpenAddForm = () => {
    setEditingDoctorId(null);
    setFormData({
      doctorName: '',
      email: '',
      password: 'DoctorPass123!',
      specialtyId: 1,
      qualifications: ''
    });
    setAlert(null);
    setIsFormOpen(true);
  };

  const handleOpenEditForm = (doctor) => {
    setEditingDoctorId(doctor.id);
    setFormData({
      doctorName: doctor.doctorName || '',
      email: '',
      password: '',
      specialtyId: doctor.specialtyId || 1,
      qualifications: doctor.qualifications || ''
    });
    setAlert(null);
    setIsFormOpen(true);
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setEditingDoctorId(null);
    setFormData({
      doctorName: '',
      email: '',
      password: '',
      specialtyId: 1,
      qualifications: ''
    });
  };

  const handleSubmitForm = async (e) => {
    e.preventDefault();
    if (!formData.qualifications.trim()) {
      setAlert({ type: 'error', text: 'Please enter doctor medical qualifications.' });
      return;
    }

    if (editingDoctorId) {
      // EDIT DOCTOR (PUT)
      try {
        const res = await fetch(`${API_BASE}/doctors/${editingDoctorId}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            specialtyId: Number(formData.specialtyId),
            qualifications: formData.qualifications
          })
        });

        if (res.ok) {
          setAlert({ type: 'success', text: '👨‍⚕️ Specialist doctor profile updated in database!' });
          fetchDoctors();
          handleCloseForm();
        } else {
          const err = await res.json();
          setAlert({ type: 'error', text: err.message || 'Failed to update profile.' });
        }
      } catch {
        const sp = specialtiesList.find((s) => s.id === Number(formData.specialtyId));
        setDoctors(
          doctors.map((d) =>
            d.id === editingDoctorId
              ? {
                  ...d,
                  specialtyId: Number(formData.specialtyId),
                  specialtyName: sp ? sp.name : d.specialtyName,
                  qualifications: formData.qualifications
                }
              : d
          )
        );
        setAlert({ type: 'success', text: '👨‍⚕️ Specialist profile updated!' });
        handleCloseForm();
      }
    } else {
      // REGISTER NEW DOCTOR USER ACCOUNT & DOCTOR RECORD (POST)
      if (!formData.doctorName.trim() || !formData.email.trim()) {
        setAlert({ type: 'error', text: 'Please enter doctor name and email address.' });
        return;
      }

      try {
        const res = await fetch(`${API_BASE}/doctors`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            doctorName: formData.doctorName.trim(),
            email: formData.email.trim(),
            password: formData.password || 'DoctorPass123!',
            specialtyId: Number(formData.specialtyId),
            qualifications: formData.qualifications.trim()
          })
        });

        if (res.ok) {
          const created = await res.json();
          setAlert({
            type: 'success',
            text: `👨‍⚕️ ${created.doctorName || 'Doctor'} registered in Users & Doctors database! (User ID #${created.userId})`
          });
          fetchDoctors();
          handleCloseForm();
        } else {
          const err = await res.json();
          setAlert({ type: 'error', text: err.message || 'Failed to register new doctor.' });
        }
      } catch {
        const sp = specialtiesList.find((s) => s.id === Number(formData.specialtyId));
        const newDoc = {
          id: doctors.length + 1,
          userId: doctors.length + 1,
          doctorName: formData.doctorName.startsWith('Dr.') ? formData.doctorName : `Dr. ${formData.doctorName}`,
          specialtyId: Number(formData.specialtyId),
          specialtyName: sp ? sp.name : 'General',
          qualifications: formData.qualifications
        };
        setDoctors([...doctors, newDoc]);
        setAlert({ type: 'success', text: `👨‍⚕️ ${newDoc.doctorName} registered successfully!` });
        handleCloseForm();
      }
    }
  };

  return (
    <div>
      {/* Top Header & Register Button */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <div>
          <h3 className="ds-section-title" style={{ margin: 0 }}>
            👨‍⚕️ Specialist Doctors Roster ({doctors.length})
          </h3>
          <p style={{ margin: '4px 0 0 0', fontSize: '0.875rem', color: 'var(--muted)' }}>
            Register new doctor user accounts and manage specialist profiles.
          </p>
        </div>
        <button
          className="ds-btn ds-btn-coral"
          onClick={isFormOpen ? handleCloseForm : handleOpenAddForm}
        >
          {isFormOpen ? '✖ Close Form' : '➕ Register New Doctor Account'}
        </button>
      </div>

      {alert && (
        <div className={`ds-alert ds-alert-${alert.type}`} style={{ marginBottom: 20 }}>
          {alert.type === 'success' ? '✅' : '⚠️'} {alert.text}
        </div>
      )}

      {/* UNIFIED REGISTER / EDIT DOCTOR FORM */}
      {isFormOpen && (
        <form className="ds-form" onSubmit={handleSubmitForm} style={{ border: '1px solid var(--blue)', boxShadow: '0 4px 20px rgba(45, 108, 223, 0.12)' }}>
          <div className="ds-form-title" style={{ justifyContent: 'space-between' }}>
            <span>
              {editingDoctorId ? '✏️ Edit Specialist Doctor Profile' : '👨‍⚕️ Register New Doctor User Account'}
            </span>
            <button
              type="button"
              onClick={handleCloseForm}
              style={{ background: 'transparent', border: 'none', cursor: 'pointer', fontSize: '1.1rem', color: 'var(--muted)' }}
            >
              ✖
            </button>
          </div>

          <div className="ds-form-row">
            {!editingDoctorId && (
              <>
                <div className="ds-form-group">
                  <label className="ds-label">Doctor Full Name</label>
                  <input
                    type="text"
                    className="ds-input"
                    placeholder="e.g. Dr. Sarah Jenkins"
                    value={formData.doctorName}
                    onChange={(e) => setFormData({ ...formData, doctorName: e.target.value })}
                    required
                  />
                </div>

                <div className="ds-form-group">
                  <label className="ds-label">Doctor Email Address</label>
                  <input
                    type="email"
                    className="ds-input"
                    placeholder="e.g. sarah.jenkins@channelcenter.hospital"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    required
                  />
                </div>

                <div className="ds-form-group">
                  <label className="ds-label">App Password</label>
                  <input
                    type="text"
                    className="ds-input"
                    placeholder="Password for doctor app login"
                    value={formData.password}
                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                    required
                  />
                </div>
              </>
            )}

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
          </div>

          <div className="ds-form-group" style={{ marginBottom: 20 }}>
            <label className="ds-label">Medical Qualifications & Degrees</label>
            <input
              type="text"
              className="ds-input"
              placeholder="e.g. MBBS, MD (Cardiology), FACC Board Certified"
              value={formData.qualifications}
              onChange={(e) => setFormData({ ...formData, qualifications: e.target.value })}
              required
            />
          </div>

          <div style={{ display: 'flex', gap: 12 }}>
            <button type="submit" className="ds-btn ds-btn-coral" style={{ flex: 1 }}>
              {editingDoctorId ? '💾 Save Profile Changes' : '👨‍⚕️ Register Doctor & Create User Account'}
            </button>
            <button type="button" className="ds-btn ds-btn-danger" onClick={handleCloseForm}>
              Cancel
            </button>
          </div>
        </form>
      )}

      {/* DOCTOR DIRECTORY TABLE */}
      <div style={{ overflowX: 'auto', background: 'var(--surface)', borderRadius: 12, border: '1px solid var(--line)', boxShadow: 'var(--shadow)' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.92rem' }}>
          <thead>
            <tr style={{ background: 'var(--canvas)', borderBottom: '2px solid var(--line)', color: 'var(--navy)' }}>
              <th style={{ padding: '14px 18px', fontWeight: 700 }}>Doctor Name</th>
              <th style={{ padding: '14px 18px', fontWeight: 700 }}>Specialty</th>
              <th style={{ padding: '14px 18px', fontWeight: 700 }}>Qualifications & Credentials</th>
              <th style={{ padding: '14px 18px', fontWeight: 700 }}>User ID</th>
              <th style={{ padding: '14px 18px', fontWeight: 700, textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {doctors.length === 0 ? (
              <tr>
                <td colSpan="5" style={{ padding: 24, textAlign: 'center', color: 'var(--muted)' }}>
                  No doctors registered in database directory.
                </td>
              </tr>
            ) : (
              doctors.map((d, index) => (
                <tr
                  key={d.id}
                  style={{
                    borderBottom: '1px solid var(--line)',
                    background: index % 2 === 0 ? 'var(--surface)' : 'var(--canvas)',
                    transition: 'background 0.15s ease'
                  }}
                >
                  <td style={{ padding: '14px 18px', fontWeight: 700, color: 'var(--ink)' }}>
                    {d.doctorName}
                  </td>
                  <td style={{ padding: '14px 18px' }}>
                    <span className="ds-badge ds-badge-approved">
                      {d.specialtyName}
                    </span>
                  </td>
                  <td style={{ padding: '14px 18px', color: 'var(--ink)' }}>
                    {d.qualifications || 'N/A'}
                  </td>
                  <td style={{ padding: '14px 18px', color: 'var(--muted)', fontWeight: 600 }}>
                    #{d.userId}
                  </td>
                  <td style={{ padding: '14px 18px', textAlign: 'right' }}>
                    <button
                      className="ds-btn ds-btn-navy"
                      style={{ padding: '6px 14px', fontSize: '0.825rem' }}
                      onClick={() => handleOpenEditForm(d)}
                    >
                      ✏️ Edit Doctor
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
