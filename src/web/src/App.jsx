import { useState } from 'react';
import AdminDashboardPage from './pages/AdminDashboardPage';
import LoginPage from './pages/LoginPage';
import './App.css';

function App() {
  const [signedIn, setSignedIn] = useState(() => Boolean(localStorage.getItem('channel-center-token')));

  const signOut = () => {
    localStorage.removeItem('channel-center-token');
    localStorage.removeItem('channel-center-user');
    setSignedIn(false);
  };

  return signedIn
    ? <AdminDashboardPage onSignOut={signOut} />
    : <LoginPage onSignedIn={() => setSignedIn(true)} />;
}

export default App;
