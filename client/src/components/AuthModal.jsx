import React, { useState } from 'react';

export default function AuthModal({ isOpen, onClose, onLoginSuccess, onRegisterSuccess }) {
  const [tab, setTab] = useState('login'); // 'login' | 'register'
  const [loginData, setLoginData] = useState({ usernameOrEmail: 'admin', password: 'Admin123!' });
  const [registerData, setRegisterData] = useState({ username: '', email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleLoginSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await onLoginSuccess(loginData.usernameOrEmail, loginData.password);
      onClose();
    } catch (err) {
      setError(err.message || 'Login failed. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  const handleRegisterSubmit = async (e) => {
    e.preventDefault();
    setError('');
    if (registerData.password.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }
    setLoading(true);
    try {
      await onRegisterSuccess(registerData.username, registerData.email, registerData.password);
      onClose();
    } catch (err) {
      const errList = err.errors && err.errors.length > 0 ? err.errors.join(' ') : err.message;
      setError(errList || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{tab === 'login' ? 'Welcome Back' : 'Create Account'}</h2>
          <button className="btn-close" onClick={onClose}>✕</button>
        </div>

        <div className="auth-tabs">
          <button 
            className={`tab-btn ${tab === 'login' ? 'active' : ''}`}
            onClick={() => { setTab('login'); setError(''); }}
          >
            Sign In
          </button>
          <button 
            className={`tab-btn ${tab === 'register' ? 'active' : ''}`}
            onClick={() => { setTab('register'); setError(''); }}
          >
            Register
          </button>
        </div>

        {error && <div className="alert alert-danger">{error}</div>}

        {tab === 'login' ? (
          <form onSubmit={handleLoginSubmit} className="form">
            <div className="form-group">
              <label>Username or Email</label>
              <input 
                type="text" 
                className="input-field" 
                value={loginData.usernameOrEmail}
                onChange={(e) => setLoginData({ ...loginData, usernameOrEmail: e.target.value })}
                required 
                placeholder="e.g. admin or user@example.com"
              />
            </div>
            <div className="form-group">
              <label>Password</label>
              <input 
                type="password" 
                className="input-field" 
                value={loginData.password}
                onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
                required 
                placeholder="Enter your password"
              />
            </div>
            <div className="demo-credentials">
              💡 <strong>Default Demo Accounts:</strong><br />
              • <code>admin</code> / <code>Admin123!</code> (Admin Role)<br />
              • <code>demo_user</code> / <code>User123!</code> (User Role)
            </div>
            <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
              {loading ? 'Signing in...' : 'Sign In'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} className="form">
            <div className="form-group">
              <label>Username</label>
              <input 
                type="text" 
                className="input-field" 
                value={registerData.username}
                onChange={(e) => setRegisterData({ ...registerData, username: e.target.value })}
                required 
                minLength={3}
                maxLength={50}
                placeholder="At least 3 characters"
              />
            </div>
            <div className="form-group">
              <label>Email Address</label>
              <input 
                type="email" 
                className="input-field" 
                value={registerData.email}
                onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
                required 
                placeholder="name@example.com"
              />
            </div>
            <div className="form-group">
              <label>Password</label>
              <input 
                type="password" 
                className="input-field" 
                value={registerData.password}
                onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
                required 
                minLength={6}
                placeholder="Min. 6 characters"
              />
            </div>
            <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
              {loading ? 'Creating account...' : 'Create Account'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
