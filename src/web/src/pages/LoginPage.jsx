import { useState } from 'react';
import { authApi } from '../api/channelCenterApi';

export default function LoginPage({ onSignedIn }) {
  const [adminUserId, setAdminUserId] = useState('');
  const [error, setError] = useState('');
  const [pending, setPending] = useState(false);

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    setPending(true);
    try {
      const session = await authApi.devLogin(Number(adminUserId));
      localStorage.setItem('channel-center-token', session.token);
      localStorage.setItem('channel-center-user', session.adminUser?.fullName || 'Administrator');
      onSignedIn();
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setPending(false);
    }
  };

  return (
    <main className="login-page">
      <form className="login-card" onSubmit={submit} aria-labelledby="login-title">
        <div className="brand-mark">CC</div>
        <p className="eyebrow">ChannelCenter / Component 04</p>
        <h1 id="login-title">Safety operations console</h1>
        <p className="login-copy">Sign in with an administrator account to review Safety Auditor workflows.</p>
        <label className="login-label" htmlFor="admin-user-id">Administrator user ID</label>
        <input id="admin-user-id" className="login-input" type="number" min="1" required value={adminUserId} onChange={(event) => setAdminUserId(event.target.value)} placeholder="Enter the seeded admin user ID" />
        {error && <div className="login-error" role="alert">{error}</div>}
        <button className="primary-button full-width" type="submit" disabled={pending}>
          {pending ? 'Signing in…' : 'Sign in'} {!pending && <span aria-hidden="true">→</span>}
        </button>
        <p className="security-note">Authentication and authorization are handled by the ASP.NET Core API.</p>
      </form>
    </main>
  );
}
