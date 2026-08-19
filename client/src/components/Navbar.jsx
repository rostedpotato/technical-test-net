import React from 'react';

export default function Navbar({ user, onOpenAuth, onLogout, onOpenAddProduct }) {
  return (
    <header className="navbar">
      <div className="navbar-container">
        <div className="brand">
          <div className="brand-logo">📦</div>
          <div className="brand-text">
            <span className="brand-title">ProductHub</span>
            <span className="brand-subtitle">Catalog & Inventory</span>
          </div>
        </div>

        <div className="navbar-actions">
          {user ? (
            <div className="user-section">
              <div className="user-badge">
                <span className="user-icon">👤</span>
                <span className="user-name">{user.username}</span>
                <span className="user-role">{user.role}</span>
              </div>
              <button 
                className="btn btn-primary"
                onClick={onOpenAddProduct}
              >
                ➕ Add Product
              </button>
              <button 
                className="btn btn-outline"
                onClick={onLogout}
              >
                Logout
              </button>
            </div>
          ) : (
            <button 
              className="btn btn-primary"
              onClick={onOpenAuth}
            >
              🔐 Login / Register
            </button>
          )}
        </div>
      </div>
    </header>
  );
}
