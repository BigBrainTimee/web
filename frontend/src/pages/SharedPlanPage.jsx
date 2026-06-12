import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import ActivitySection from '../components/ActivitySection';
import AlertMessage from '../components/AlertMessage';
import BudgetSummary from '../components/BudgetSummary';
import PackingListSection from '../components/PackingListSection';
import DestinationSection from '../components/DestinationSection';
import ExpensesSection from '../components/ExpensesSection';
import PlanSectionNav, { getPlanSections, PLAN_SECTIONS } from '../components/PlanSectionNav';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as shareService from '../services/shareService';
import { getAccessTypeLabel } from '../utils/accessTypes';

const sections = getPlanSections({ includeSharing: false });

export default function SharedPlanPage() {
  const { token: shareToken } = useParams();
  const { token: authToken } = useAuth();
  const [data, setData] = useState(null);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [expenses, setExpenses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [requiresLogin, setRequiresLogin] = useState(false);
  const [activeSection, setActiveSection] = useState('overview');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function loadData() {
    setLoading(true);
    setError('');
    setRequiresLogin(false);

    try {
      const shared = await shareService.getSharedPlan(shareToken, authToken);
      setData(shared);
      setBudgetSummary(shared.budgetSummary);
      setExpenses(shared.expenses ?? []);
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        setRequiresLogin(true);
        setData(null);
        return;
      }
      setError(err instanceof ApiError ? err.message : 'Link nije validan ili je istekao.');
      setData(null);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, [shareToken, authToken]);

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

  if (requiresLogin) {
    const returnUrl = encodeURIComponent(`/shared/${shareToken}`);
    return (
      <div className="page">
        <div className="card shared-login-required">
          <h1>Potrebna prijava</h1>
          <p className="muted">
            Link za izmenu plana mogu da koriste samo prijavljeni korisnici.
            Prijavi se da bi nastavio.
          </p>
          <Link to={`/login?returnUrl=${returnUrl}`} className="btn btn-primary">
            Prijavi se
          </Link>
        </div>
      </div>
    );
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
                runAction(
                  () => shareService.deleteSharedExpense(shareToken, id, authToken),
                  'Trošak je obrisan.',
                );
              } : undefined}
              onAddExpense={canEdit ? (payload) => runAction(
                () => shareService.addSharedExpense(shareToken, payload, authToken),
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
                runAction(
                  () => shareService.deleteSharedDestination(shareToken, destinationId, authToken),
                  'Destinacija je obrisana.',
                );
              } : undefined}
              onSubmit={canEdit ? (payload) => runAction(
                () => shareService.addSharedDestination(shareToken, payload, authToken),
                'Destinacija je dodata.',
              ) : undefined}
              onUpdate={canEdit ? (destinationId, payload) => runAction(
                () => shareService.updateSharedDestination(shareToken, destinationId, payload, authToken),
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
                runAction(
                  () => shareService.deleteSharedActivity(shareToken, activityId, authToken),
                  'Aktivnost je obrisana.',
                );
              } : undefined}
              onSubmit={canEdit ? (payload) => runAction(
                () => shareService.addSharedActivity(shareToken, payload, authToken),
                'Aktivnost je dodata.',
              ) : undefined}
              onUpdate={canEdit ? (activityId, payload) => runAction(
                () => shareService.updateSharedActivity(shareToken, activityId, payload, authToken),
                'Aktivnost je ažurirana.',
              ) : undefined}
            />
          </div>
        );

      case 'checklist':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.checklist.label}</h2>
            <PackingListSection
              items={checklistItems}
              onToggle={canEdit ? (itemId) => runAction(
                () => shareService.toggleSharedChecklistItem(shareToken, itemId, authToken),
              ) : undefined}
              onDelete={canEdit ? (itemId) => {
                if (!window.confirm('Obrisati stavku?')) return;
                runAction(
                  () => shareService.deleteSharedChecklistItem(shareToken, itemId, authToken),
                  'Stavka je obrisana.',
                );
              } : undefined}
              onAdd={canEdit ? (payload) => runAction(
                () => shareService.addSharedChecklistItem(shareToken, payload, authToken),
                'Stavka je dodata u listu za pakovanje.',
              ) : undefined}
              readOnly={!canEdit}
            />
          </div>
        );

      default:
        return null;
    }
  }

  return (
    <div className="page">
      <div className="shared-banner">
        Deljeni plan · pristup: <strong>{getAccessTypeLabel(accessType)}</strong>
        {!canEdit && <span> (samo pregled — ne moraš biti prijavljen)</span>}
        {canEdit && <span> (izmena — prijavljen si i možeš menjati plan)</span>}
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
