import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import DestinationForm from '../components/DestinationForm';
import DestinationList from '../components/DestinationList';
import TravelPlanForm from '../components/TravelPlanForm';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as travelPlanService from '../services/travelPlanService';

export default function TravelPlanDetailPage() {
  const { id } = useParams();
  const { token } = useAuth();
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadData() {
    setLoading(true);
    setError('');

    try {
      const [planData, destinationData] = await Promise.all([
        travelPlanService.getTravelPlan(token, id),
        travelPlanService.getDestinations(token, id),
      ]);
      setPlan(planData);
      setDestinations(destinationData);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje plana nije uspelo.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, [token, id]);

  async function handleUpdate(payload) {
    try {
      const updated = await travelPlanService.updateTravelPlan(token, id, payload);
      setPlan(updated);
      setEditing(false);
      setSuccess('Plan je ažuriran.');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ažuriranje nije uspelo.');
    }
  }

  async function handleDeletePlan() {
    if (!window.confirm('Obrisati ceo plan putovanja?')) return;

    try {
      await travelPlanService.deleteTravelPlan(token, id);
      navigate('/plans');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje nije uspelo.');
    }
  }

  async function handleAddDestination(payload) {
    try {
      await travelPlanService.createDestination(token, id, payload);
      setSuccess('Destinacija je dodata.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Dodavanje destinacije nije uspelo.');
    }
  }

  async function handleDeleteDestination(destinationId) {
    if (!window.confirm('Obrisati destinaciju?')) return;

    try {
      await travelPlanService.deleteDestination(token, id, destinationId);
      setSuccess('Destinacija je obrisana.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje destinacije nije uspelo.');
    }
  }

  if (loading) {
    return <div className="page">Učitavanje plana...</div>;
  }

  if (!plan) {
    return (
      <div className="page">
        <AlertMessage message={error || 'Plan nije pronađen.'} />
        <Link to="/plans" className="btn btn-secondary">Nazad na planove</Link>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <Link to="/plans" className="back-link">← Nazad</Link>
          <h1>{plan.name}</h1>
          <p className="muted">{plan.startDate} → {plan.endDate}</p>
        </div>
        <div className="header-actions">
          <button type="button" className="btn btn-secondary" onClick={() => setEditing((prev) => !prev)}>
            {editing ? 'Otkaži izmenu' : 'Izmeni plan'}
          </button>
          <button type="button" className="btn btn-danger" onClick={handleDeletePlan}>
            Obriši plan
          </button>
        </div>
      </div>

      <AlertMessage message={error} onClose={() => setError('')} />
      <AlertMessage type="success" message={success} onClose={() => setSuccess('')} />

      {editing ? (
        <TravelPlanForm
          initialValues={{
            name: plan.name,
            description: plan.description,
            startDate: plan.startDate,
            endDate: plan.endDate,
            plannedBudget: plan.plannedBudget,
            notes: plan.notes,
          }}
          submitLabel="Sačuvaj izmene"
          onSubmit={handleUpdate}
        />
      ) : (
        <section className="card plan-summary">
          <p><strong>Budžet:</strong> {plan.plannedBudget} €</p>
          {plan.description && <p><strong>Opis:</strong> {plan.description}</p>}
          {plan.notes && <p><strong>Napomene:</strong> {plan.notes}</p>}
        </section>
      )}

      <section className="section-block">
        <h2>Destinacije</h2>
        <DestinationList destinations={destinations} onDelete={handleDeleteDestination} />
        <DestinationForm onSubmit={handleAddDestination} />
      </section>
    </div>
  );
}
