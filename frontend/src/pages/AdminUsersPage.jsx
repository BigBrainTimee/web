import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import AdminUserForm from '../components/AdminUserForm';
import AlertMessage from '../components/AlertMessage';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as adminService from '../services/adminService';

export default function AdminUsersPage() {
  const { token, user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadUsers() {
    setLoading(true);
    setError('');

    try {
      const data = await adminService.getUsers(token);
      setUsers(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje korisnika nije uspelo.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadUsers();
  }, [token]);

  async function handleRoleChange(userId, role) {
    try {
      await adminService.updateUserRole(token, userId, role);
      setSuccess('Uloga je ažurirana.');
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ažuriranje uloge nije uspelo.');
    }
  }

  async function handleCreate(payload) {
    try {
      await adminService.addUser(token, payload);
      setSuccess('Korisnik je dodat.');
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Dodavanje korisnika nije uspelo.');
    }
  }

  async function handleDelete(userId, name) {
    if (!window.confirm(`Obrisati korisnika "${name}"? Svi njegovi planovi će biti obrisani.`)) {
      return;
    }

    try {
      await adminService.deleteUser(token, userId);
      setSuccess('Korisnik je obrisan.');
      await loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje korisnika nije uspelo.');
    }
  }

  if (loading) {
    return <div className="page">Učitavanje korisnika...</div>;
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Administracija — korisnici</h1>
          <p className="muted">Upravljanje nalozima i ulogama u sistemu.</p>
        </div>
      </div>

      <AlertMessage message={error} onClose={() => setError('')} />
      <AlertMessage type="success" message={success} onClose={() => setSuccess('')} />

      <AdminUserForm onSubmit={handleCreate} />

      <div className="card admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Ime</th>
              <th>Prezime</th>
              <th>Email</th>
              <th>Uloga</th>
              <th>Kreiran</th>
              <th>Akcije</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => {
              const isSelf = user.id === currentUser?.id;

              return (
                <tr key={user.id}>
                  <td>{user.name}</td>
                  <td>{user.lastName || '—'}</td>
                  <td>{user.email}</td>
                  <td>
                    {isSelf ? (
                      <span className="badge access-edit">{user.role}</span>
                    ) : (
                      <select
                        value={user.role}
                        onChange={(e) => handleRoleChange(user.id, e.target.value)}
                      >
                        <option value="User">Korisnik</option>
                        <option value="Admin">Administrator</option>
                      </select>
                    )}
                  </td>
                  <td>{new Date(user.createdAt).toLocaleString('sr-RS')}</td>
                  <td>
                    <div className="admin-actions">
                      <Link
                        to={`/admin/users/${user.id}/plans`}
                        className="btn btn-secondary btn-sm"
                      >
                        Planovi
                      </Link>
                      {!isSelf && (
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          onClick={() => handleDelete(user.id, user.fullName || user.name)}
                        >
                          Obriši
                        </button>
                      )}
                      {isSelf && <span className="muted">Tvoj nalog</span>}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
