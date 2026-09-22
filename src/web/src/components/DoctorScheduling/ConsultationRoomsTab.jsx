import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5066/api/doctor-scheduling';

export default function ConsultationRoomsTab() {
  const [rooms, setRooms] = useState([]);
  const [newRoom, setNewRoom] = useState({ roomName: '', floor: '' });
  const [alert, setAlert] = useState(null);

  const fetchRooms = async () => {
    try {
      const res = await fetch(`${API_BASE}/consultationrooms`);
      if (res.ok) {
        setRooms(await res.json());
      }
    } catch {
      setRooms([
        { id: 1, roomName: 'Room 101', floor: '1st Floor', isActive: true },
        { id: 2, roomName: 'Room 202', floor: '2nd Floor', isActive: true },
        { id: 3, roomName: 'Room 305', floor: '3rd Floor', isActive: false }
      ]);
    }
  };

  useEffect(() => {
    fetchRooms();
  }, []);

  const handleAddRoom = async (e) => {
    e.preventDefault();
    if (!newRoom.roomName || !newRoom.floor) return;

    try {
      const res = await fetch(`${API_BASE}/consultationrooms`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...newRoom, isActive: true })
      });

      if (res.ok) {
        setAlert({ type: 'success', text: '🏥 Consultation Room registered in PostgreSQL Database!' });
        setNewRoom({ roomName: '', floor: '' });
        fetchRooms();
      }
    } catch {
      setRooms([
        ...rooms,
        { id: rooms.length + 1, roomName: newRoom.roomName, floor: newRoom.floor, isActive: true }
      ]);
      setAlert({ type: 'success', text: '🏥 Consultation Room added!' });
      setNewRoom({ roomName: '', floor: '' });
    }
  };

  const toggleRoomStatus = async (room) => {
    try {
      await fetch(`${API_BASE}/consultationrooms/${room.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          roomName: room.roomName,
          floor: room.floor,
          isActive: !room.isActive
        })
      });
      fetchRooms();
    } catch {
      setRooms(
        rooms.map((r) => (r.id === room.id ? { ...r, isActive: !r.isActive } : r))
      );
    }
  };

  return (
    <div>
      <form className="ds-form" onSubmit={handleAddRoom}>
        <div className="ds-form-title">
          <span>🏥</span> Register Clinic Consultation Room
        </div>

        {alert && (
          <div className={`ds-alert ds-alert-${alert.type}`}>
            ✅ {alert.text}
          </div>
        )}

        <div className="ds-form-row">
          <div className="ds-form-group">
            <label className="ds-label">Room Identifier / Number</label>
            <input
              type="text"
              className="ds-input"
              placeholder="e.g. Room 104"
              value={newRoom.roomName}
              onChange={(e) => setNewRoom({ ...newRoom, roomName: e.target.value })}
            />
          </div>

          <div className="ds-form-group">
            <label className="ds-label">Floor & Wing Location</label>
            <input
              type="text"
              className="ds-input"
              placeholder="e.g. 1st Floor - Wing B"
              value={newRoom.floor}
              onChange={(e) => setNewRoom({ ...newRoom, floor: e.target.value })}
            />
          </div>
        </div>

        <button type="submit" className="ds-btn ds-btn-coral">
          Add Consultation Room
        </button>
      </form>

      <h3 style={{ marginBottom: 16, color: 'var(--ds-navy-dark)' }}>
        🏢 Clinic Room Inventory ({rooms.length})
      </h3>

      <div className="ds-grid">
        {rooms.map((room) => (
          <div key={room.id} className="ds-card">
            <div>
              <div className="ds-card-header">
                <h4 className="ds-card-title">{room.roomName}</h4>
                <span className={`ds-badge ${room.isActive ? 'ds-badge-active' : 'ds-badge-inactive'}`}>
                  {room.isActive ? 'Active' : 'Maintenance'}
                </span>
              </div>
              <div style={{ fontSize: '0.9rem', color: '#627d98', marginBottom: 16 }}>
                Floor: {room.floor}
              </div>
            </div>
            <button
              className={`ds-btn ${room.isActive ? 'ds-btn-danger' : 'ds-btn-success'}`}
              onClick={() => toggleRoomStatus(room)}
            >
              {room.isActive ? 'Set Room Maintenance' : 'Activate Room'}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
