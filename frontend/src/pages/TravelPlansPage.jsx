import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as travelPlanService from '../services/travelPlanService';

export default function TravelPlansPage() {
  const { token } = useAuth();
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadPlans() {
    setLoading(true);
    setError('');

    try {
      const data = await travelPlanService.getTravelPlans(token);
      setPlans(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje planova nije uspelo.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadPlans();
  }, [token]);

  async function handleDelete(id) {
    if (!window.confirm('Da li sigurno želiš da obrišeš plan?')) {
      return;
    }

    try {
      await travelPlanService.deleteTravelPlan(token, id);
      setSuccess('Plan je obrisan.');
      await loadPlans();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje nije uspelo.');
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Moji planovi</h1>
          <p className="muted">Pregled svih planova putovanja.</p>
        </div>
        <Link to="/plans/new" className="btn btn-primary">Novi plan</Link>
      </div>

      <AlertMessage message={error} onClose={() => setError('')} />
      <AlertMessage type="success" message={success} onClose={() => setSuccess('')} />

      {loading ? (
        <p>Učitavanje planova...</p>
      ) : plans.length === 0 ? (
        <div className="card empty-state">
          <p>Još nemaš planova putovanja.</p>
          <Link to="/plans/new" className="btn btn-primary">Kreiraj prvi plan</Link>
        </div>
      ) : (
        <div className="plan-grid">
          {plans.map((plan) => (
            <article key={plan.id} className="card plan-card">
              <div>
                <h2>{plan.name}</h2>
                <p className="muted">{plan.startDate} → {plan.endDate}</p>
                <p>Budžet: {plan.plannedBudget} €</p>
                {plan.description && <p>{plan.description}</p>}
              </div>
              <div className="card-actions">
                <Link to={`/plans/${plan.id}`} className="btn btn-secondary">Detalji</Link>
                <button type="button" className="btn btn-danger" onClick={() => handleDelete(plan.id)}>
                  Obriši
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
