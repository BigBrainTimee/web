import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import ActivitySection from '../components/ActivitySection';
import AlertMessage from '../components/AlertMessage';
import BudgetSummary from '../components/BudgetSummary';
import DestinationSection from '../components/DestinationSection';
import ExpensesSection from '../components/ExpensesSection';
import PackingListSection from '../components/PackingListSection';
import PlanSectionNav, { getPlanSections, PLAN_SECTIONS } from '../components/PlanSectionNav';
import ShareLinkPanel from '../components/ShareLinkPanel';
import TravelPlanForm from '../components/TravelPlanForm';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as adminBudgetService from '../services/adminBudgetService';
import * as adminService from '../services/adminService';
import * as adminTravelService from '../services/adminTravelService';
import { ensurePackingDefaults } from '../utils/ensurePackingDefaults';

const sections = getPlanSections({ includeSharing: true });

export default function AdminUserPlansPage() {
  const { userId } = useParams();
  const { token } = useAuth();
  const [user, setUser] = useState(null);
  const [plans, setPlans] = useState([]);
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [plan, setPlan] = useState(null);
  const [destinations, setDestinations] = useState([]);
  const [activities, setActivities] = useState([]);
  const [checklistItems, setChecklistItems] = useState([]);
  const [expenses, setExpenses] = useState([]);
  const [budgetSummary, setBudgetSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [planLoading, setPlanLoading] = useState(false);
  const [editing, setEditing] = useState(false);
  const [creatingPlan, setCreatingPlan] = useState(false);
  const [activeSection, setActiveSection] = useState('overview');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [downloadingPdf, setDownloadingPdf] = useState(false);

  const shareApi = useMemo(() => {
    if (!token || !selectedPlanId) return null;
    return adminTravelService.createAdminShareApi(token, userId, selectedPlanId);
  }, [token, userId, selectedPlanId]);

  async function loadUserAndPlans(keepPlanId = selectedPlanId) {
    setLoading(true);
    setError('');

    try {
      const [userData, planData] = await Promise.all([
        adminService.getUser(token, userId),
        adminTravelService.getUserTravelPlans(token, userId),
      ]);

      setUser(userData);
      setPlans(planData);

      if (planData.length > 0) {
        const nextPlanId = keepPlanId && planData.some((p) => String(p.id) === keepPlanId)
          ? keepPlanId
          : String(planData[0].id);
        setSelectedPlanId(nextPlanId);
      } else {
        setSelectedPlanId('');
        setPlan(null);
        setDestinations([]);
        setActivities([]);
        setChecklistItems([]);
        setExpenses([]);
        setBudgetSummary(null);
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje korisnika nije uspelo.');
    } finally {
      setLoading(false);
    }
  }

  async function loadPlanData(planId = selectedPlanId) {
    if (!planId) return;

    setPlanLoading(true);
    setError('');

    try {
      const [
        planData,
        destinationData,
        activityData,
        checklistData,
        expenseData,
        summaryData,
      ] = await Promise.all([
        adminTravelService.getUserTravelPlan(token, userId, planId),
        adminTravelService.getUserDestinations(token, userId, planId),
        adminTravelService.getUserActivities(token, userId, planId),
        adminTravelService.getUserChecklistItems(token, userId, planId),
        adminBudgetService.getUserExpenses(token, userId, planId),
        adminBudgetService.getUserBudgetSummary(token, userId, planId),
      ]);

      setPlan(planData);
      setDestinations(destinationData);
      setActivities(activityData);
      const seededChecklist = await ensurePackingDefaults(
        (entry) => adminTravelService.createUserChecklistItem(token, userId, planId, entry),
        checklistData,
      );
      setChecklistItems(
        seededChecklist ?? await adminTravelService.getUserChecklistItems(token, userId, planId),
      );
      setExpenses(expenseData);
      setBudgetSummary(summaryData);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje plana nije uspelo.');
    } finally {
      setPlanLoading(false);
    }
  }

  useEffect(() => {
    loadUserAndPlans();
  }, [token, userId]);

  useEffect(() => {
    if (selectedPlanId) {
      loadPlanData(selectedPlanId);
    }
  }, [selectedPlanId, token, userId]);

  async function reloadPlan() {
    await loadPlanData(selectedPlanId);
  }

  async function handleCreatePlan(payload) {
    setError('');
    setSuccess('');

    try {
      const created = await adminTravelService.createUserTravelPlan(token, userId, payload);
      setSuccess('Plan putovanja je kreiran.');
      setCreatingPlan(false);
      await loadUserAndPlans(String(created.id));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kreiranje plana nije uspelo.');
    }
  }

  async function handleUpdatePlan(payload) {
    try {
      const updated = await adminTravelService.updateUserTravelPlan(token, userId, selectedPlanId, payload);
      setPlan(updated);
      setEditing(false);
      setSuccess('Plan je ažuriran.');
      await loadUserAndPlans(selectedPlanId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Ažuriranje plana nije uspelo.');
    }
  }

  async function handleDeletePlan() {
    if (!window.confirm('Obrisati ceo plan putovanja?')) return;

    try {
      await adminTravelService.deleteUserTravelPlan(token, userId, selectedPlanId);
      setSuccess('Plan je obrisan.');
      setEditing(false);
      await loadUserAndPlans();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje plana nije uspelo.');
    }
  }

  async function handleDownloadReport() {
    setDownloadingPdf(true);
    setError('');

    try {
      await adminTravelService.downloadUserPlanReport(token, userId, selectedPlanId);
      setSuccess('PDF izveštaj je preuzet.');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Preuzimanje PDF-a nije uspelo.');
    } finally {
      setDownloadingPdf(false);
    }
  }

  async function runAction(action, successMessage) {
    try {
      await action();
      if (successMessage) setSuccess(successMessage);
      await reloadPlan();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Operacija nije uspela.');
    }
  }

  function renderSectionContent() {
    if (!plan) return null;

    switch (activeSection) {
      case 'overview':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.overview.label}</h2>
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
                onSubmit={handleUpdatePlan}
              />
            ) : (
              <section className="card plan-summary">
                <p><strong>Budžet:</strong> {plan.plannedBudget} €</p>
                {plan.description && <p><strong>Opis:</strong> {plan.description}</p>}
                {plan.notes && <p><strong>Napomene:</strong> {plan.notes}</p>}
              </section>
            )}
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
              plan={plan}
              onAddExpense={(payload) => runAction(
                () => adminBudgetService.createUserExpense(token, userId, selectedPlanId, payload),
                'Trošak je dodat.',
              )}
              onDeleteExpense={(expenseId) => {
                if (!window.confirm('Obrisati trošak?')) return;
                runAction(
                  () => adminBudgetService.deleteUserExpense(token, userId, selectedPlanId, expenseId),
                  'Trošak je obrisan.',
                );
              }}
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
              onDelete={(destinationId) => {
                if (!window.confirm('Obrisati destinaciju?')) return;
                runAction(
                  () => adminTravelService.deleteUserDestination(token, userId, selectedPlanId, destinationId),
                  'Destinacija je obrisana.',
                );
              }}
              onSubmit={(payload) => runAction(
                () => adminTravelService.createUserDestination(token, userId, selectedPlanId, payload),
                'Destinacija je dodata.',
              )}
              onUpdate={(destinationId, payload) => runAction(
                () => adminTravelService.updateUserDestination(token, userId, selectedPlanId, destinationId, payload),
                'Destinacija je ažurirana.',
              )}
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
              onDelete={(activityId) => {
                if (!window.confirm('Obrisati aktivnost?')) return;
                runAction(
                  () => adminTravelService.deleteUserActivity(token, userId, selectedPlanId, activityId),
                  'Aktivnost je obrisana.',
                );
              }}
              onSubmit={(payload) => runAction(
                () => adminTravelService.createUserActivity(token, userId, selectedPlanId, payload),
                'Aktivnost je dodata.',
              )}
              onUpdate={(activityId, payload) => runAction(
                () => adminTravelService.updateUserActivity(token, userId, selectedPlanId, activityId, payload),
                'Aktivnost je ažurirana.',
              )}
            />
          </div>
        );

      case 'sharing':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.sharing.label}</h2>
            {shareApi && (
              <ShareLinkPanel authToken={token} planId={selectedPlanId} shareApi={shareApi} />
            )}
          </div>
        );

      case 'checklist':
        return (
          <div className="plan-detail-panel">
            <h2>{PLAN_SECTIONS.checklist.label}</h2>
            <PackingListSection
              items={checklistItems}
              onToggle={(itemId) => runAction(
                () => adminTravelService.toggleUserChecklistItem(token, userId, selectedPlanId, itemId),
              )}
              onDelete={(itemId) => {
                if (!window.confirm('Obrisati stavku?')) return;
                runAction(
                  () => adminTravelService.deleteUserChecklistItem(token, userId, selectedPlanId, itemId),
                  'Stavka je obrisana.',
                );
              }}
              onAdd={(payload) => runAction(
                () => adminTravelService.createUserChecklistItem(token, userId, selectedPlanId, payload),
                'Stavka je dodata u listu za pakovanje.',
              )}
            />
          </div>
        );

      default:
        return null;
    }
  }

  if (loading) {
    return <div className="page">Učitavanje...</div>;
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <Link to="/admin/users" className="back-link">← Nazad na korisnike</Link>
          <h1>Planovi korisnika</h1>
          <p className="muted">
            {user ? `${user.fullName || user.name} · ${user.email}` : 'Korisnik'}
          </p>
        </div>
        {plan && (
          <div className="header-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={handleDownloadReport}
              disabled={downloadingPdf}
            >
              {downloadingPdf ? 'Generisanje PDF-a...' : 'Preuzmi PDF'}
            </button>
            <button type="button" className="btn btn-secondary" onClick={() => setCreatingPlan((prev) => !prev)}>
              {creatingPlan ? 'Zatvori formu' : 'Novi plan'}
            </button>
            <button type="button" className="btn btn-secondary" onClick={() => setEditing((prev) => !prev)}>
              {editing ? 'Otkaži izmenu' : 'Izmeni plan'}
            </button>
            <button type="button" className="btn btn-danger" onClick={handleDeletePlan}>
              Obriši plan
            </button>
          </div>
        )}
      </div>

      <AlertMessage message={error} onClose={() => setError('')} />
      <AlertMessage type="success" message={success} onClose={() => setSuccess('')} />

      {(creatingPlan || plans.length === 0) && (
        <section>
          <h2>{plans.length === 0 ? 'Kreiraj prvi plan' : 'Kreiraj novi plan'}</h2>
          <TravelPlanForm
            onSubmit={handleCreatePlan}
            submitLabel="Kreiraj plan za korisnika"
          />
        </section>
      )}

      {plans.length > 0 && (
        <section className="card plan-summary">
          <label>
            Plan putovanja
            <select
              value={selectedPlanId}
              onChange={(e) => {
                setSelectedPlanId(e.target.value);
                setEditing(false);
                setActiveSection('overview');
              }}
            >
              {plans.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.name} ({item.startDate} → {item.endDate})
                </option>
              ))}
            </select>
          </label>
        </section>
      )}

      {planLoading && <p className="muted">Učitavanje plana...</p>}

      {plan && !planLoading && (
        <>
          <div className="page-header">
            <div>
              <h2>{plan.name}</h2>
              <p className="muted">{plan.startDate} → {plan.endDate}</p>
            </div>
          </div>

          <div className="plan-detail-layout">
            <PlanSectionNav
              sections={sections}
              activeSection={activeSection}
              onSectionChange={setActiveSection}
            />
            <div className="plan-detail-content">
              {renderSectionContent()}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
