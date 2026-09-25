import { useState, useCallback } from 'react';
import Sidebar from '../components/Sidebar';
import WorkflowReviewTab from '../components/WorkflowConsole/WorkflowReviewTab';
import AuditTrailTab from '../components/WorkflowConsole/AuditTrailTab';
import SafetyAnalyticsTab from '../components/WorkflowConsole/SafetyAnalyticsTab';
import DoctorSchedulingPage from './DoctorSchedulingPage';

// Student 1 ΓÇö Patient Management & Appointment Lifecycle
import PatientDirectoryTab from '../components/PatientManagement/PatientDirectoryTab';
import AppointmentDirectoryTab from '../components/PatientManagement/AppointmentDirectoryTab';
import IntakeAgentConsoleTab from '../components/PatientManagement/IntakeAgentConsoleTab';
import AdminRegisterTab from '../components/AdminRegisterTab';

export default function AdminDashboardPage({ onSignOut }) {
  const [activeArea, setActiveArea] = useState('patients');
  // When staff clicks "AI Intake" from the patient directory for a specific patient
  const [intakePatient, setIntakePatient] = useState(null);

  const handleLaunchIntake = useCallback((patient) => {
    setIntakePatient(patient);
    setActiveArea('intake');
  }, []);

  const handleNavigate = useCallback((area) => {
    if (area !== 'intake') setIntakePatient(null);
    setActiveArea(area);
  }, []);

  return (
    <div className="console-shell">
      <Sidebar activeArea={activeArea} onNavigate={handleNavigate} onSignOut={onSignOut} />
      <main className="console-main">
        {/* Student 1: Patient Management & Appointment Lifecycle */}
        {activeArea === 'patients' && (
          <PatientDirectoryTab onLaunchIntake={handleLaunchIntake} />
        )}
        {activeArea === 'appointments' && <AppointmentDirectoryTab />}
        {activeArea === 'intake' && (
          <IntakeAgentConsoleTab initialPatient={intakePatient} />
        )}

        {/* Student 4: Admin & Safety Workflows (unchanged) */}
        {activeArea === 'workflows' && <WorkflowReviewTab />}
        {activeArea === 'audit' && <AuditTrailTab />}
        {activeArea === 'analytics' && <SafetyAnalyticsTab />}

        {/* Student 2: Doctor Scheduling (unchanged) */}
        {activeArea === 'scheduling' && <DoctorSchedulingPage />}

        {/* Admin Management: Register Admin */}
        {activeArea === 'admin-register' && <AdminRegisterTab />}
      </main>
    </div>
  );
}

