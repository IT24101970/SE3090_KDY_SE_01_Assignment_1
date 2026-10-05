import { useState } from 'react';
import { authApi } from '../api/channelCenterApi';

export default function LoginPage({ onSignedIn }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [pending, setPending] = useState(false);

  const handleLoginSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setPending(true);

    try {
      const session = await authApi.adminLogin({
        email,
        password,
      });

      if (session.user && session.user.role !== 'Admin') {
        throw new Error('Access denied. Only users with the Admin role can log into this administrative portal.');
      }

      localStorage.setItem('channel-center-token', session.token);
      localStorage.setItem('channel-center-user', session.user?.fullName || session.user?.email || 'Administrator');
      localStorage.setItem('channel-center-role', session.user?.role || 'Admin');
      onSignedIn();
    } catch (requestError) {
      setError(requestError.message || 'Login failed. Please check your admin credentials.');
    } finally {
      setPending(false);
    }
  };

  return (
    <main className="login-page">
      <div className="login-card" aria-labelledby="login-title">
        <div className="login-header">
          <div className="brand-mark">CC</div>
          <span className="admin-badge">
            <span className="badge-dot"></span> Admin Access Only
          </span>
        </div>

        <p className="eyebrow">ChannelCenter / Administrative Portal</p>
        <h1 id="login-title">Admin Sign In</h1>
        <p className="login-copy">
          Sign in with your hospital administrator account credentials to access the central operations console.
        </p>

        {error && <div className="login-error" role="alert">{error}</div>}

        <form onSubmit={handleLoginSubmit}>
          <div className="form-field">
            <label className="login-label" htmlFor="login-email">
              Admin Email / Username
            </label>
            <input
              id="login-email"
              className="login-input"
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="admin@channelcenter.hospital"
              autoComplete="username"
            />
          </div>

          <div className="form-field">
            <label className="login-label" htmlFor="login-password">
              Password
            </label>
            <input
              id="login-password"
              className="login-input"
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
            />
          </div>

          <button className="primary-button full-width" type="submit" disabled={pending}>
            {pending ? 'Verifying & Signing in…' : 'Sign In to Dashboard'} {!pending && <span aria-hidden="true">→</span>}
          </button>
        </form>

        <div className="auth-footer-notes">
          <p className="security-note">
            🔒 Only existing administrators can sign in. New admin accounts must be created by an active administrator inside the dashboard.
          </p>
        </div>
      </div>
    </main>
  );
}
