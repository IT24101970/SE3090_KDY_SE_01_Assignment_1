export default function Sidebar({ activeArea, onNavigate, onSignOut }) {
  const userName = localStorage.getItem('channel-center-user') || 'Administrator';
  const items = [
    ['patients', '👥', 'Patient Directory'],
    ['appointments', '📅', 'Appointments'],
    ['intake', '⚡', 'AI Intake Agent'],
    ['workflows', '🛡️', 'Workflow Review'],
    ['audit', '📋', 'Audit Trail'],
    ['analytics', '📊', 'Safety Analytics'],
    ['scheduling', '👨‍⚕️', 'Doctor Scheduling'],
    ['admin-register', '🔑', 'Register Admin'],
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
        <div className="api-status">
          <span className="status-dot" /> API Connected
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
