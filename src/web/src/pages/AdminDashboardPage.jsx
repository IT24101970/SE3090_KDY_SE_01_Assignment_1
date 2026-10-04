import { useState, useCallback } from 'react';
import Sidebar from '../components/Sidebar';
import WorkflowGovernanceConsole from '../components/WorkflowConsole/WorkflowGovernanceConsole';
import DoctorSchedulingPage from './DoctorSchedulingPage';

// Student 1 ΓÇö Patient Management & Appointment Lifecycle
import PatientDirectoryTab from '../components/PatientManagement/PatientDirectoryTab';
import AppointmentDirectoryTab from '../components/PatientManagement/AppointmentDirectoryTab';
import IntakeAgentConsoleTab from '../components/PatientManagement/IntakeAgentConsoleTab';
import { ClinicalReviewView } from '../components/ClinicalReviewView';
import AdminRegisterTab from '../components/AdminRegisterTab';

export default function AdminDashboardPage({ onSignOut }) {
  const [activeArea, setActiveArea] = useState('patients');
  // When staff clicks "AI Intake" from the patient directory for a specific patient
  const [intakePatient, setIntakePatient] = useState(null);
  const [isAdminRegisterOpen, setIsAdminRegisterOpen] = useState(false);

  const handleLaunchIntake = useCallback((patient) => {
    setIntakePatient(patient);
    setActiveArea('intake');
  }, []);

  const handleNavigate = useCallback((area) => {
    if (area === 'admin-register') {
      setIsAdminRegisterOpen(true);
      return;
    }
    if (area !== 'intake') setIntakePatient(null);
    setActiveArea(area);
  }, []);

  return (
    <div className="console-shell">
      <Sidebar
        activeArea={activeArea}
        onNavigate={handleNavigate}
        onOpenAdminRegister={() => setIsAdminRegisterOpen(true)}
        isAdminRegisterOpen={isAdminRegisterOpen}
        onSignOut={onSignOut}
      />
      <main className="console-main">
        {/* Student 1: Patient Management & Appointment Lifecycle */}
        {activeArea === 'patients' && (
          <PatientDirectoryTab onLaunchIntake={handleLaunchIntake} />
        )}
        {activeArea === 'appointments' && <AppointmentDirectoryTab />}
        {activeArea === 'intake' && (
          <IntakeAgentConsoleTab initialPatient={intakePatient} />
        )}

        {/* Student 3: Medical Triage & Specialist Matching */}
        {activeArea === 'triage' && <ClinicalReviewView />}

        {/* Student 4: Admin & Safety Workflows (Unified Governance Console) */}
        {(activeArea === 'workflows' || activeArea === 'audit' || activeArea === 'analytics') && (
          <WorkflowGovernanceConsole />
        )}

        {/* Student 2: Doctor Scheduling */}
        {activeArea === 'scheduling' && <DoctorSchedulingPage />}
      </main>

      {/* Admin Registration Popup Modal */}
      {isAdminRegisterOpen && (
        <AdminRegisterTab
          isModal={true}
          onClose={() => setIsAdminRegisterOpen(false)}
        />
      )}
    </div>
  );
}

