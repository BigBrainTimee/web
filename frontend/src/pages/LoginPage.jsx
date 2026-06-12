import { useState } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';

export default function LoginPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const rawReturnUrl = searchParams.get('returnUrl');
  const returnUrl = rawReturnUrl?.startsWith('/') && !rawReturnUrl.startsWith('//')
    ? rawReturnUrl
    : '/plans';
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to={returnUrl} replace />;
  }

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setError('');
  }

  async function handleSubmit(event) {
    event.preventDefault();

    if (!form.email || !form.password) {
      setError('Email i lozinka su obavezni.');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await login(form);
      navigate(returnUrl);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Prijava nije uspela.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <form className="card auth-card" onSubmit={handleSubmit}>
        <h1>Prijava</h1>
        <p className="muted">Uloguj se i upravljaj planovima putovanja.</p>

        <AlertMessage message={error} onClose={() => setError('')} />

        <label>
          Email
          <input type="email" name="email" value={form.email} onChange={handleChange} />
        </label>

        <label>
          Lozinka
          <input type="password" name="password" value={form.password} onChange={handleChange} />
        </label>

        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Prijava...' : 'Prijavi se'}
        </button>

        <p className="auth-switch">
          Nemaš nalog? <Link to="/register">Registruj se</Link>
        </p>
      </form>
    </div>
  );
}
