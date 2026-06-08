import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import ActivityForm from '../components/ActivityForm';
import ActivityList from '../components/ActivityList';
import BudgetSummary from '../components/BudgetSummary';
import ChecklistForm from '../components/ChecklistForm';
import ChecklistList from '../components/ChecklistList';
import ExpenseForm from '../components/ExpenseForm';
import ExpenseList from '../components/ExpenseList';
import DestinationForm from '../components/DestinationForm';
import DestinationList from '../components/DestinationList';
import TravelPlanForm from '../components/TravelPlanForm';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as budgetService from '../services/budgetService';
import * as travelPlanService from '../services/travelPlanService';

export default function TravelPlanDetailPage() {
  const { id } = useParams();
  const { token } = useAuth();
  const navigate = useNavigate();

  const [plan, setPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [checklistItems, setChecklistItems] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadData() {
    setLoading(true);
    setError('');

    try {
      const [planData, destinationData, activityData, checklistData, expenseData, summaryData] = await Promise.all([
        travelPlanService.getTravelPlan(token, id),
        travelPlanService.getDestinations(token, id),
        travelPlanService.getActivities(token, id),
        travelPlanService.getChecklistItems(token, id),
        budgetService.getExpenses(token, id),
        budgetService.getBudgetSummary(token, id),
      ]);
      setPlan(planData);
      setDestinations(destinationData);
      setActivities(activityData);
      setChecklistItems(checklistData);
      setExpenses(expenseData);
      setBudgetSummary(summaryData);
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

  async function handleAddActivity(payload) {
    try {
      await travelPlanService.createActivity(token, id, payload);
      setSuccess('Aktivnost je dodata.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Dodavanje aktivnosti nije uspelo.');
    }
  }

  async function handleDeleteActivity(activityId) {
    if (!window.confirm('Obrisati aktivnost?')) return;

    try {
      await travelPlanService.deleteActivity(token, id, activityId);
      setSuccess('Aktivnost je obrisana.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje aktivnosti nije uspelo.');
    }
  }

  async function handleAddChecklistItem(payload) {
    try {
      await travelPlanService.createChecklistItem(token, id, payload);
      setSuccess('Stavka je dodata u checklistu.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Dodavanje stavke nije uspelo.');
    }
  }

  async function handleToggleChecklistItem(itemId) {
    try {
      await travelPlanService.toggleChecklistItem(token, id, itemId);
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ažuriranje checkliste nije uspelo.');
    }
  }

  async function handleDeleteChecklistItem(itemId) {
    if (!window.confirm('Obrisati stavku?')) return;

    try {
      await travelPlanService.deleteChecklistItem(token, id, itemId);
      setSuccess('Stavka je obrisana.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje stavke nije uspelo.');
    }
  }

  async function handleAddExpense(payload) {
    try {
      await budgetService.createExpense(token, id, payload);
      setSuccess('Trošak je dodat.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Dodavanje troška nije uspelo.');
    }
  }

  async function handleDeleteExpense(expenseId) {
    if (!window.confirm('Obrisati trošak?')) return;

    try {
      await budgetService.deleteExpense(token, id, expenseId);
      setSuccess('Trošak je obrisan.');
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje troška nije uspelo.');
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

      <BudgetSummary summary={budgetSummary} />

      <section className="section-block">
        <h2>Troškovi</h2>
        <ExpenseList expenses={expenses} onDelete={handleDeleteExpense} />
        <ExpenseForm onSubmit={handleAddExpense} />
      </section>

      <section className="section-block">
        <h2>Destinacije</h2>
        <DestinationList destinations={destinations} onDelete={handleDeleteDestination} />
        <DestinationForm onSubmit={handleAddDestination} />
      </section>

      <section className="section-block">
        <h2>Aktivnosti po danima</h2>
        <ActivityList activities={activities} onDelete={handleDeleteActivity} />
        <ActivityForm destinations={destinations} onSubmit={handleAddActivity} />
      </section>

      <section className="section-block">
        <h2>Packing lista</h2>
        <ChecklistList
          items={checklistItems}
          onToggle={handleToggleChecklistItem}
          onDelete={handleDeleteChecklistItem}
        />
        <ChecklistForm onSubmit={handleAddChecklistItem} />
      </section>
    </div>
  );
}
