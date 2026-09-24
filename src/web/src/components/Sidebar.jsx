export default function Sidebar({ activeArea, onNavigate, onSignOut }) {
  const userName = localStorage.getItem('channel-center-user') || 'Administrator';
  const items = [
    ['patients', '≡ƒæÑ', 'Patient directory'],
    ['appointments', '≡ƒôà', 'Appointments'],
    ['intake', 'ΓÜí', 'AI Intake Agent'],
    ['workflows', 'Γùê', 'Workflow review'],
    ['audit', 'Γëí', 'Audit trail'],
    ['analytics', 'ΓûÑ', 'Safety analytics'],
    ['scheduling', 'Γîü', 'Doctor scheduling'],
  ];

  return (
    <aside className="console-sidebar">
      <div className="sidebar-brand">
        <div className="brand-mark">CC</div>
        <div><strong>ChannelCenter</strong><span>Safety operations</span></div>
      </div>
      <div className="sidebar-section-label">Workspace</div>
      <nav aria-label="Primary navigation">
        {items.map(([key, icon, label]) => (
          <button
            key={key}
            className={activeArea === key ? 'nav-item active' : 'nav-item'}
            onClick={() => onNavigate(key)}
          >
            <span>{icon}</span> {label}
          </button>
        ))}
      </nav>
      <div className="sidebar-footer">
        <div className="api-status"><span className="status-dot" /> API connected</div>
        <button className="user-menu" onClick={onSignOut}>
          <span className="avatar">KP</span>
          <span><strong>{userName}</strong><small>Admin ┬╖ Sign out</small></span>
        </button>
      </div>
    </aside>
  );
}
