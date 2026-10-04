export default function Sidebar({ activeArea, onNavigate, onSignOut }) {
  const userName = localStorage.getItem('channel-center-user') || 'Administrator';
  const items = [
    ['patients', '👥', 'Patient Directory'],
    ['appointments', '📅', 'Appointments'],
    //['intake', '⚡', 'AI Intake Agent'],
    ['triage', '🩺', 'Triage & Referrals'],
    ['workflows', '🛡️', 'Workflow Governance'],
    ['scheduling', '👨‍⚕️', 'Doctor Scheduling'],
  ];

  const getInitials = (name) => {
    if (!name) return 'AD';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    return name.slice(0, 2).toUpperCase();
  };

  return (
    <aside className="console-sidebar">
      <div className="sidebar-brand">
        <div className="brand-mark">CC</div>
        <div>
          <strong>ChannelCenter</strong>
          <span>Medical Operations</span>
        </div>
      </div>
      <div className="sidebar-section-label">Medical Workspace</div>
      <nav aria-label="Primary navigation">
        {items.map(([key, icon, label]) => (
          <button
            key={key}
            className={activeArea === key ? 'nav-item active' : 'nav-item'}
            onClick={() => onNavigate(key)}
          >
            <span className="nav-icon">{icon}</span> {label}
          </button>
        ))}
      </nav>
      <div className="sidebar-footer">
        <div className="api-status" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span className="status-dot" /> API Connected
          </div>
          <button
            className={activeArea === 'admin-register' ? 'admin-add-btn active' : 'admin-add-btn'}
            onClick={() => onNavigate('admin-register')}
            title="Register New Admin Account"
            aria-label="Register New Admin Account"
          >
            +
          </button>
        </div>
        <button className="user-menu" onClick={onSignOut} title="Click to Sign Out">
          <span className="avatar">{getInitials(userName)}</span>
          <span className="user-info">
            <strong>{userName}</strong>
            <small>Admin · Sign Out</small>
          </span>
        </button>
      </div>
    </aside>
  );
}
