import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import ActivitySection from '../components/ActivitySection';
import AlertMessage from '../components/AlertMessage';
import BudgetSummary from '../components/BudgetSummary';
import ChecklistForm from '../components/ChecklistForm';
import ChecklistList from '../components/ChecklistList';
import DestinationSection from '../components/DestinationSection';
import ExpensesSection from '../components/ExpensesSection';
import PlanSectionNav, { getPlanSections, PLAN_SECTIONS } from '../components/PlanSectionNav';
import { ApiError } from '../services/apiClient';
import * as shareService from '../services/shareService';

const sections = getPlanSections({ includeSharing: false });

export default function SharedPlanPage() {
  const { token } = useParams();
  const [data, setData] = useState(null);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [expenses, setExpenses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeSection, setActiveSection] = useState('overview');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadData() {
    setLoading(true);
    setError('');

    try {
      const shared = await shareService.getSharedPlan(token);
      setData(shared);
      setBudgetSummary(shared.budgetSummary);
      setExpenses(shared.expenses ?? []);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Link nije validan ili je istekao.');
      setData(null);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, [token]);

  function handleSectionChange(sectionId) {
    setActiveSection(sectionId);
  }

  async function runAction(action, successMessage) {
    try {
      await action();
      if (successMessage) setSuccess(successMessage);
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Operacija nije uspela.');
    }
  }

  if (loading) {
    return <div className="page">Učitavanje deljenog plana...</div>;
  }

  if (!data) {
    return (
      <div className="page">
        <AlertMessage message={error || 'Plan nije dostupan.'} />
      </div>
    );
  }

  const { plan, destinations, activities, checklistItems, canEdit, accessType } = data;

  function renderSectionContent() {
    switch (activeSection) {
      case 'overview':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.overview.label}</h2>
            <section className="card plan-summary">
              <p><strong>Budžet:</strong> {plan.plannedBudget} €</p>
              {plan.description && <p><strong>Opis:</strong> {plan.description}</p>}
              {plan.notes && <p><strong>Napomene:</strong> {plan.notes}</p>}
            </section>
            <BudgetSummary summary={budgetSummary} activities={activities} />
          </div>
        );

      case 'expenses':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.expenses.label}</h2>
            <ExpensesSection
              expenses={expenses}
              activities={activities}
              readOnly={!canEdit}
              onDeleteExpense={canEdit ? (id) => {
                if (!window.confirm('Obrisati trošak?')) return;
                runAction(() => shareService.deleteSharedExpense(token, id), 'Trošak je obrisan.');
              } : undefined}
              onAddExpense={canEdit ? (payload) => runAction(
                () => shareService.addSharedExpense(token, payload),
                'Trošak je dodat.',
              ) : undefined}
            />
          </div>
        );

      case 'destinations':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.destinations.label}</h2>
            <DestinationSection
              destinations={destinations}
              plan={plan}
              readOnly={!canEdit}
              onDelete={canEdit ? (destinationId) => {
                if (!window.confirm('Obrisati destinaciju?')) return;
                runAction(() => shareService.deleteSharedDestination(token, destinationId), 'Destinacija je obrisana.');
              } : undefined}
              onSubmit={canEdit ? (payload) => runAction(
                () => shareService.addSharedDestination(token, payload),
                'Destinacija je dodata.',
              ) : undefined}
              onUpdate={canEdit ? (destinationId, payload) => runAction(
                () => shareService.updateSharedDestination(token, destinationId, payload),
                'Destinacija je ažurirana.',
              ) : undefined}
            />
          </div>
        );

      case 'activities':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.activities.label}</h2>
            <ActivitySection
              activities={activities}
              destinations={destinations}
              plan={plan}
              readOnly={!canEdit}
              onDelete={canEdit ? (activityId) => {
                if (!window.confirm('Obrisati aktivnost?')) return;
                runAction(() => shareService.deleteSharedActivity(token, activityId), 'Aktivnost je obrisana.');
              } : undefined}
              onSubmit={canEdit ? (payload) => runAction(
                () => shareService.addSharedActivity(token, payload),
                'Aktivnost je dodata.',
              ) : undefined}
              onUpdate={canEdit ? (activityId, payload) => runAction(
                () => shareService.updateSharedActivity(token, activityId, payload),
                'Aktivnost je ažurirana.',
              ) : undefined}
            />
          </div>
        );

      case 'checklist':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.checklist.label}</h2>
            <ChecklistList
              items={checklistItems}
              onToggle={canEdit ? (itemId) => runAction(() => shareService.toggleSharedChecklistItem(token, itemId)) : undefined}
              onDelete={canEdit ? (itemId) => {
                if (!window.confirm('Obrisati stavku?')) return;
                runAction(() => shareService.deleteSharedChecklistItem(token, itemId), 'Stavka je obrisana.');
              } : undefined}
              readOnly={!canEdit}
            />
            {canEdit && (
              <ChecklistForm onSubmit={(payload) => runAction(
                () => shareService.addSharedChecklistItem(token, payload),
                'Stavka je dodata.',
              )} />
            )}
          </div>
        );

      default:
        return null;
    }
  }

  return (
    <div className="page">
      <div className="shared-banner">
        Deljeni plan · pristup: <strong>{accessType}</strong>
        {!canEdit && <span> (samo pregled)</span>}
        {canEdit && <span> (možeš menjati destinacije, aktivnosti, troškove i packing listu)</span>}
      </div>

      <div className="page-header">
        <div>
          <h1>{plan.name}</h1>
          <p className="muted">{plan.startDate} → {plan.endDate}</p>
        </div>
      </div>

      <AlertMessage message={error} onClose={() => setError('')} />
      <AlertMessage type="success" message={success} onClose={() => setSuccess('')} />

      <div className="plan-detail-layout">
        <PlanSectionNav
          sections={sections}
          activeSection={activeSection}
          onSectionChange={handleSectionChange}
        />
        <div className="plan-detail-content">
          {renderSectionContent()}
        </div>
      </div>
    </div>
  );
}
