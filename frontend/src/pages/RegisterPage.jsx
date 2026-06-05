import { useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';

export default function RegisterPage() {
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ name: '', email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/plans" replace />;
  }

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setError('');
  }

  async function handleSubmit(event) {
    event.preventDefault();

    if (!form.name.trim() || !form.email || !form.password) {
      setError('Sva polja su obavezna.');
      return;
    }

    if (form.password.length < 6) {
      setError('Lozinka mora imati najmanje 6 karaktera.');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await register(form);
      navigate('/plans');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Registracija nije uspela.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <form className="card auth-card" onSubmit={handleSubmit}>
        <h1>Registracija</h1>
        <p className="muted">Kreiraj nalog za planiranje putovanja.</p>

        <AlertMessage message={error} onClose={() => setError('')} />

        <label>
          Ime
          <input name="name" value={form.name} onChange={handleChange} />
        </label>

        <label>
          Email
          <input type="email" name="email" value={form.email} onChange={handleChange} />
        </label>

        <label>
          Lozinka
          <input type="password" name="password" value={form.password} onChange={handleChange} />
        </label>

        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Registracija...' : 'Registruj se'}
        </button>

        <p className="auth-switch">
          Već imaš nalog? <Link to="/login">Prijavi se</Link>
        </p>
      </form>
    </div>
  );
}
