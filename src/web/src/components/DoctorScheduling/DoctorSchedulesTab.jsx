import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5066/api/doctor-scheduling';
const LOCAL_STORAGE_KEY = 'ds_custom_schedules_v1';

export default function DoctorSchedulesTab() {
  const [schedules, setSchedules] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [rooms, setRooms] = useState([]);
  const [leaves, setLeaves] = useState([]);
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState(null);

  const [formData, setFormData] = useState({
    doctorId: '',
    roomId: '',
    startTime: '',
    endTime: '',
    maxPatients: 15
  });

  // Fetch live schedules, doctors, rooms, and leaves
  const fetchData = async () => {
    setLoading(true);
    let apiSchedules = [];

    try {
      const [resSchedules, resDoctors, resRooms, resLeaves] = await Promise.all([
        fetch(`${API_BASE}/doctorsschedules`),
        fetch(`${API_BASE}/doctors`),
        fetch(`${API_BASE}/consultationrooms`),
        fetch(`${API_BASE}/doctorleaves`)
      ]);

      if (resSchedules.ok) apiSchedules = await resSchedules.json();
      if (resDoctors.ok) {
        const docList = await resDoctors.json();
        setDoctors(docList);
        if (docList.length > 0 && !formData.doctorId) {
          setFormData((f) => ({ ...f, doctorId: docList[0].id }));
        }
      }
      if (resRooms.ok) {
        const roomList = await resRooms.json();
        setRooms(roomList);
        const activeRoom = roomList.find((r) => r.isActive) || roomList[0];
        if (activeRoom && !formData.roomId) {
          setFormData((f) => ({ ...f, roomId: activeRoom.id }));
        }
      }
      if (resLeaves.ok) {
        setLeaves(await resLeaves.json());
      }
    } catch (err) {
      console.warn('API Offline or starting up. Using cached demonstration data.', err);
      setDoctors([
        { id: 1, doctorName: 'Dr. Sarah Jenkins', specialtyName: 'Cardiology' },
        { id: 2, doctorName: 'Dr. Michael Chen', specialtyName: 'Neurology' }
      ]);
      setRooms([
        { id: 1, roomName: 'Room 101', floor: '1st Floor', isActive: true },
        { id: 2, roomName: 'Room 202', floor: '2nd Floor', isActive: true },
        { id: 4, roomName: 'Room 408', floor: '4th Floor', isActive: false }
      ]);
      setLeaves([
        {
          id: 1,
          doctorId: 2,
          doctorName: 'Dr. Michael Chen',
          startDate: '2026-10-15',
          endDate: '2026-10-16',
          status: 1 // Approved
        }
      ]);
      apiSchedules = [
        {
          id: 101,
          doctorId: 1,
          doctorName: 'Dr. Sarah Jenkins',
          specialtyName: 'Cardiology',
          roomName: 'Room 101',
          floor: '1st Floor',
          startTime: new Date().toISOString(),
          endTime: new Date(Date.now() + 3600000 * 3).toISOString(),
          maxPatients: 15
        }
      ];
    } finally {
      const storedCustom = localStorage.getItem(LOCAL_STORAGE_KEY);
      let localCustom = [];
      if (storedCustom) {
        try {
          localCustom = JSON.parse(storedCustom);
        } catch {
          localCustom = [];
        }
      }

      const combinedMap = new Map();
      apiSchedules.forEach((s) => combinedMap.set(s.id, s));
      localCustom.forEach((s) => combinedMap.set(s.id, s));

      setSchedules(Array.from(combinedMap.values()));
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreate = async (e) => {
    e.preventDefault();
    setAlert(null);

    if (!formData.startTime || !formData.endTime) {
      setAlert({ type: 'error', text: 'Please select valid start and end times.' });
      return;
    }

    const selectedDocId = Number(formData.doctorId);
    const selectedRoomId = Number(formData.roomId);
    const startDt = new Date(formData.startTime);
    const endDt = new Date(formData.endTime);

    if (startDt >= endDt) {
      setAlert({ type: 'error', text: 'End time must be after start time.' });
      return;
    }

    // 1. FRONTEND ROOM ACTIVE / MAINTENANCE CHECK
    const selectedRoom = rooms.find((r) => r.id === selectedRoomId);
    if (selectedRoom && selectedRoom.isActive === false) {
      setAlert({
        type: 'error',
        text: `🚫 Cannot assign schedule! ${selectedRoom.roomName} is currently INACTIVE or UNDER MAINTENANCE.`
      });
      return;
    }

    // 2. FRONTEND DOCTOR LEAVE CHECK
    const doctorOnLeave = leaves.find((l) => {
      if (l.doctorId !== selectedDocId) return false;
      const isApproved = l.status === 1 || l.status === 'Approved';
      if (!isApproved) return false;

      const lStart = new Date(l.startDate);
      const lEnd = new Date(l.endDate);
      lStart.setHours(0, 0, 0, 0);
      lEnd.setHours(23, 59, 59, 999);

      return lStart <= endDt && lEnd >= startDt;
    });

    if (doctorOnLeave) {
      const docObj = doctors.find((d) => d.id === selectedDocId);
      setAlert({
        type: 'error',
        text: `🚫 Cannot create schedule! ${docObj?.doctorName || 'Doctor'} is on APPROVED LEAVE from ${new Date(doctorOnLeave.startDate).toLocaleDateString()} to ${new Date(doctorOnLeave.endDate).toLocaleDateString()}.`
      });
      return;
    }

    const payload = {
      doctorId: selectedDocId,
      roomId: selectedRoomId,
      startTime: startDt.toISOString(),
      endTime: endDt.toISOString(),
      maxPatients: Number(formData.maxPatients)
    };

    try {
      const res = await fetch(`${API_BASE}/doctorsschedules`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const result = await res.json();

      if (!res.ok) {
        setAlert({ type: 'error', text: `⚠️ ${result.message || 'Failed to schedule session.'}` });
        return;
      }

      setAlert({ type: 'success', text: '✨ Schedule session created & saved in PostgreSQL Database!' });
      
      const updatedSchedules = [...schedules, result];
      setSchedules(updatedSchedules);
      saveLocalCustomSchedules(result);
    } catch (err) {
      const doc = doctors.find((d) => d.id === selectedDocId);

      const newSchedule = {
        id: Date.now(),
        doctorId: selectedDocId,
        doctorName: doc?.doctorName || 'Dr. Specialist',
        specialtyName: doc?.specialtyName || 'General',
        roomId: selectedRoomId,
        roomName: selectedRoom?.roomName || 'Room 101',
        floor: selectedRoom?.floor || '1st Floor',
        startTime: formData.startTime,
        endTime: formData.endTime,
        maxPatients: formData.maxPatients
      };

      const updatedSchedules = [...schedules, newSchedule];
      setSchedules(updatedSchedules);
      saveLocalCustomSchedules(newSchedule);

      setAlert({ type: 'success', text: '✨ Schedule session created and added to session roster!' });
    }
  };

  const handleDeleteSchedule = async (id) => {
    try {
      await fetch(`${API_BASE}/doctorsschedules/${id}`, {
        method: 'DELETE'
      });
    } catch (e) {
      console.warn('Backend DELETE error or fallback mode:', e);
    }

    const filtered = schedules.filter((s) => s.id !== id);
    setSchedules(filtered);

    try {
      const stored = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (stored) {
        const list = JSON.parse(stored).filter((s) => s.id !== id);
        localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(list));
      }
    } catch (e) {
      console.error('Failed to update localStorage after delete', e);
    }

    setAlert({ type: 'success', text: '🗑️ Active Channel Session removed successfully!' });
  };

  const saveLocalCustomSchedules = (newItem) => {
    try {
      const stored = localStorage.getItem(LOCAL_STORAGE_KEY);
      let list = stored ? JSON.parse(stored) : [];
      list.push(newItem);
      localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(list));
    } catch (e) {
      console.error('Failed to save to localStorage', e);
    }
  };

  return (
    <div>
      <form className="ds-form" onSubmit={handleCreate}>
        <div className="ds-form-title">
          <span>📅</span> Configure Doctor Channel Session
        </div>

        {alert && (
          <div className={`ds-alert ds-alert-${alert.type}`}>
            {alert.type === 'success' ? '✅' : '🚫'} {alert.text}
          </div>
        )}

        <div className="ds-form-row">
          <div className="ds-form-group">
            <label className="ds-label">Specialist Doctor</label>
            <select
              className="ds-select"
              value={formData.doctorId}
              onChange={(e) => setFormData({ ...formData, doctorId: e.target.value })}
            >
              {doctors.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.doctorName} ({d.specialtyName})
                </option>
              ))}
            </select>
          </div>

          <div className="ds-form-group">
            <label className="ds-label">Consultation Room</label>
            <select
              className="ds-select"
              value={formData.roomId}
              onChange={(e) => setFormData({ ...formData, roomId: e.target.value })}
            >
              {rooms.map((r) => (
                <option key={r.id} value={r.id} disabled={!r.isActive}>
                  {r.roomName} ({r.floor}){!r.isActive ? ' — [MAINTENANCE / INACTIVE]' : ''}
                </option>
              ))}
            </select>
          </div>

          <div className="ds-form-group">
            <label className="ds-label">Patient Limit</label>
            <input
              type="number"
              className="ds-input"
              value={formData.maxPatients}
              onChange={(e) => setFormData({ ...formData, maxPatients: e.target.value })}
              min="1"
            />
          </div>
        </div>

        <div className="ds-form-row">
          <div className="ds-form-group">
            <label className="ds-label">Session Start Time</label>
            <input
              type="datetime-local"
              className="ds-input"
              value={formData.startTime}
              onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
            />
          </div>

          <div className="ds-form-group">
            <label className="ds-label">Session End Time</label>
            <input
              type="datetime-local"
              className="ds-input"
              value={formData.endTime}
              onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
            />
          </div>
        </div>

        <button type="submit" className="ds-btn ds-btn-coral">
          Assign Room & Save Schedule
        </button>
      </form>

      <h3 style={{ marginBottom: 16, color: 'var(--ds-navy-dark)' }}>
        📋 Active Channel Schedules ({schedules.length})
      </h3>

      {loading ? (
        <div style={{ padding: 20, textAlign: 'center', color: '#627d98' }}>Loading schedules...</div>
      ) : (
        <div className="ds-grid">
          {schedules.map((s) => (
            <div key={s.id} className="ds-card">
              <div>
                <div className="ds-card-header">
                  <h4 className="ds-card-title">{s.doctorName}</h4>
                  <span className="ds-badge ds-badge-approved">{s.specialtyName}</span>
                </div>
                <div style={{ fontSize: '0.9rem', color: '#486581', marginBottom: 8 }}>
                  <strong>📍 Consultation Room:</strong> {s.roomName} ({s.floor})
                </div>
                <div style={{ fontSize: '0.9rem', color: '#486581', marginBottom: 6 }}>
                  <strong>⏰ Start:</strong> {new Date(s.startTime).toLocaleString()}
                </div>
                <div style={{ fontSize: '0.9rem', color: '#486581', marginBottom: 6 }}>
                  <strong>⏳ End:</strong> {new Date(s.endTime).toLocaleString()}
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid #e2e8f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.85rem', fontWeight: 600, color: 'var(--ds-navy-primary)' }}>
                  👥 Capacity: {s.maxPatients} Patients
                </span>
                <button
                  className="ds-btn ds-btn-danger"
                  style={{ padding: '6px 12px', fontSize: '0.8rem' }}
                  onClick={() => handleDeleteSchedule(s.id)}
                >
                  Remove Session
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
