import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout({ children }) {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="brand">
          Planer putovanja
        </Link>
        {isAuthenticated && (
          <nav className="nav-links">
            <Link to="/plans">Planovi</Link>
            {user?.role === 'Admin' && <Link to="/admin/users">Administracija</Link>}
            <span className="user-badge">{user?.name}</span>
            <button type="button" className="btn btn-secondary" onClick={handleLogout}>
              Odjava
            </button>
          </nav>
        )}
      </header>
      <main className="app-main">{children}</main>
    </div>
  );
}
