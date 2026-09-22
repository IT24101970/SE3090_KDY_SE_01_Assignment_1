import { useState } from 'react';
import Sidebar from '../components/Sidebar';
import WorkflowReviewTab from '../components/WorkflowConsole/WorkflowReviewTab';
import AuditTrailTab from '../components/WorkflowConsole/AuditTrailTab';
import SafetyAnalyticsTab from '../components/WorkflowConsole/SafetyAnalyticsTab';
import DoctorSchedulingPage from './DoctorSchedulingPage';

export default function AdminDashboardPage({ onSignOut }) {
  const [activeArea, setActiveArea] = useState('workflows');

  return (
    <div className="console-shell">
      <Sidebar activeArea={activeArea} onNavigate={setActiveArea} onSignOut={onSignOut} />
      <main className="console-main">
        {activeArea === 'workflows' && <WorkflowReviewTab />}
        {activeArea === 'audit' && <AuditTrailTab />}
        {activeArea === 'analytics' && <SafetyAnalyticsTab />}
        {activeArea === 'scheduling' && <DoctorSchedulingPage />}
      </main>
    </div>
  );
}
