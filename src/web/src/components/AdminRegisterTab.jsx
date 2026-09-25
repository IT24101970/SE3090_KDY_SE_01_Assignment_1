import { useState } from 'react';
import { authApi } from '../api/channelCenterApi';

export default function AdminRegisterTab() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [pending, setPending] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters long.');
      return;
    }

    setPending(true);

    try {
      const response = await authApi.adminRegister({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
      });

      setSuccess(`Admin user "${response.user?.fullName || fullName}" was registered successfully! Password has been securely hashed in the database.`);
      setFullName('');
      setEmail('');
      setPassword('');
      setConfirmPassword('');
    } catch (err) {
      setError(err.message || 'Failed to register admin account.');
    } finally {
      setPending(false);
    }
  };

  return (
    <div className="tab-container" style={{ padding: '28px', maxWidth: '800px' }}>
      <div className="tab-header" style={{ marginBottom: '24px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <span style={{ fontSize: '24px' }}>🔑</span>
          <div>
            <h2 style={{ font: "700 22px 'Space Grotesk', sans-serif", margin: 0, color: 'var(--navy)' }}>
              Admin User Management
            </h2>
            <p style={{ color: 'var(--muted)', fontSize: '13px', margin: '4px 0 0' }}>
              Register new administrator accounts. Only logged-in administrators can perform this action.
            </p>
          </div>
        </div>
      </div>

      <div
        className="admin-security-banner"
        style={{
          background: 'linear-gradient(135deg, #edf4ff 0%, #e0ecff 100%)',
          border: '1px solid #cce0ff',
          borderRadius: '12px',
          padding: '16px 20px',
          marginBottom: '24px',
          display: 'flex',
          alignItems: 'flex-start',
          gap: '12px',
        }}
      >
        <span style={{ fontSize: '20px', lineHeight: 1 }}>🛡️</span>
        <div style={{ fontSize: '13px', color: '#1e40af', lineHeight: '1.5' }}>
          <strong>Role-Restricted Provisioning</strong>
          <br />
          You are currently logged in as an authorized <strong>Admin</strong>. Newly created accounts will be granted full Admin role privileges and saved with salted password hashing in the database (`User` table).
        </div>
      </div>

      {error && (
        <div className="login-error" role="alert" style={{ marginBottom: '20px' }}>
          {error}
        </div>
      )}

      {success && (
        <div className="login-success" role="status" style={{ marginBottom: '20px' }}>
          {success}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ background: '#fff', border: '1px solid var(--line)', borderRadius: '16px', padding: '28px', boxShadow: '0 4px 20px rgba(0,0,0,0.03)' }}>
        <div className="form-field" style={{ marginBottom: '18px' }}>
          <label className="login-label" htmlFor="admin-fullname">
            Full Name <span style={{ color: 'var(--red)' }}>*</span>
          </label>
          <input
            id="admin-fullname"
            className="login-input"
            type="text"
            required
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="e.g. Dr. Arthur Pendelton"
          />
        </div>

        <div className="form-field" style={{ marginBottom: '18px' }}>
          <label className="login-label" htmlFor="admin-email">
            Email Address <span style={{ color: 'var(--red)' }}>*</span>
          </label>
          <input
            id="admin-email"
            className="login-input"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="e.g. admin.arthur@channelcenter.hospital"
            autoComplete="username"
          />
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '24px' }}>
          <div className="form-field">
            <label className="login-label" htmlFor="admin-password">
              Password <span style={{ color: 'var(--red)' }}>*</span>
            </label>
            <input
              id="admin-password"
              className="login-input"
              type="password"
              required
              minLength={6}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Minimum 6 characters"
              autoComplete="new-password"
            />
          </div>

          <div className="form-field">
            <label className="login-label" htmlFor="admin-confirmpassword">
              Confirm Password <span style={{ color: 'var(--red)' }}>*</span>
            </label>
            <input
              id="admin-confirmpassword"
              className="login-input"
              type="password"
              required
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Re-enter password"
              autoComplete="new-password"
            />
          </div>
        </div>

        <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end' }}>
          <button
            type="button"
            className="outline-button"
            onClick={() => {
              setFullName('');
              setEmail('');
              setPassword('');
              setConfirmPassword('');
              setError('');
              setSuccess('');
            }}
          >
            Clear Form
          </button>
          <button type="submit" className="primary-button" disabled={pending}>
            {pending ? 'Registering Admin…' : '🔑 Register New Admin'}
          </button>
        </div>
      </form>
    </div>
  );
}
