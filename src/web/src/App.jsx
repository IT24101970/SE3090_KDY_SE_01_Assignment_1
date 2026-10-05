import { useState, useEffect } from 'react';
import AdminDashboardPage from './pages/AdminDashboardPage';
import LoginPage from './pages/LoginPage';
import './App.css';

function App() {
  const [signedIn, setSignedIn] = useState(() => Boolean(localStorage.getItem('channel-center-token')));

  const signOut = () => {
    localStorage.removeItem('channel-center-token');
    localStorage.removeItem('channel-center-user');
    localStorage.removeItem('channel-center-role');
    setSignedIn(false);
  };

  // Auto-redirect to login if the token expires mid-session (triggered by 401 in API layer)
  useEffect(() => {
    const handleSessionExpired = () => setSignedIn(false);
    window.addEventListener('session-expired', handleSessionExpired);
    return () => window.removeEventListener('session-expired', handleSessionExpired);
  }, []);

  return signedIn
    ? <AdminDashboardPage onSignOut={signOut} />
    : <LoginPage onSignedIn={() => setSignedIn(true)} />;
}

export default App;
