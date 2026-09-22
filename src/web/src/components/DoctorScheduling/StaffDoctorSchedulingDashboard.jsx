import React, { useState, useEffect } from 'react';
import DoctorSchedulesTab from './DoctorSchedulesTab';
import ConsultationRoomsTab from './ConsultationRoomsTab';
import DoctorLeaveRosterTab from './DoctorLeaveRosterTab';
import DoctorDirectoryTab from './DoctorDirectoryTab';
import './DoctorScheduling.css';

export default function StaffDoctorSchedulingDashboard() {
  const [activeTab, setActiveTab] = useState('schedules');
  const [dbConnected, setDbConnected] = useState(false);

  useEffect(() => {
    fetch('http://localhost:5066/api/doctor-scheduling/doctors')
      ? setDbConnected(true)
      : setDbConnected(false);
  }, []);

  return (
    <div className="ds-container">
      {/* Header Banner */}
      <div className="ds-header">
        <div>
          <h2 className="ds-title">Doctor Scheduling & Consultation Portal</h2>
          <p className="ds-subtitle">
            Clinic Operational Control: Sessions, Room Allocation & Leave Rosters
          </p>
        </div>
        <div className="ds-db-status">
          <span className={`ds-db-dot ${dbConnected ? 'connected' : ''}`}></span>
          PostgreSQL Database {dbConnected ? 'Connected' : 'Offline'}
        </div>
      </div>

      {/* Tabs Bar */}
      <div className="ds-nav-tabs">
        <button
          className={`ds-tab-btn ${activeTab === 'schedules' ? 'active' : ''}`}
          onClick={() => setActiveTab('schedules')}
        >
          <span>📅</span> Channel Schedules
        </button>
        <button
          className={`ds-tab-btn ${activeTab === 'rooms' ? 'active' : ''}`}
          onClick={() => setActiveTab('rooms')}
        >
          <span>🏥</span> Consultation Rooms
        </button>
        <button
          className={`ds-tab-btn ${activeTab === 'leaves' ? 'active' : ''}`}
          onClick={() => setActiveTab('leaves')}
        >
          <span>📋</span> Leave Rosters
        </button>
        <button
          className={`ds-tab-btn ${activeTab === 'doctors' ? 'active' : ''}`}
          onClick={() => setActiveTab('doctors')}
        >
          <span>👨‍⚕️</span> Doctor Directory
        </button>
      </div>

      {/* Tab Panels */}
      <div>
        {activeTab === 'schedules' && <DoctorSchedulesTab />}
        {activeTab === 'rooms' && <ConsultationRoomsTab />}
        {activeTab === 'leaves' && <DoctorLeaveRosterTab />}
        {activeTab === 'doctors' && <DoctorDirectoryTab />}
      </div>
    </div>
  );
}
