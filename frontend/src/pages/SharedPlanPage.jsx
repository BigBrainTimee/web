import { useEffect, useState } from 'react';

import { useParams } from 'react-router-dom';

import ActivityForm from '../components/ActivityForm';

import ActivityList from '../components/ActivityList';

import AlertMessage from '../components/AlertMessage';

import BudgetSummary from '../components/BudgetSummary';

import ChecklistForm from '../components/ChecklistForm';

import ChecklistList from '../components/ChecklistList';

import DestinationForm from '../components/DestinationForm';

import DestinationList from '../components/DestinationList';

import ExpenseForm from '../components/ExpenseForm';

import ExpenseList from '../components/ExpenseList';

import { ApiError } from '../services/apiClient';

import * as shareService from '../services/shareService';



export default function SharedPlanPage() {

  const { token } = useParams();

  const [data, setData] = useState(null);

  const [budgetSummary, setBudgetSummary] = useState(null);

  const [expenses, setExpenses] = useState([]);

  const [loading, setLoading] = useState(true);

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



      <section className="card plan-summary">

        <p><strong>Budžet:</strong> {plan.plannedBudget} €</p>

        {plan.description && <p><strong>Opis:</strong> {plan.description}</p>}

        {plan.notes && <p><strong>Napomene:</strong> {plan.notes}</p>}

      </section>



      <BudgetSummary summary={budgetSummary} />



      <section className="section-block">

        <h2>Troškovi</h2>

        <ExpenseList

          expenses={expenses}

          onDelete={canEdit ? (id) => {

            if (!window.confirm('Obrisati trošak?')) return;

            runAction(() => shareService.deleteSharedExpense(token, id), 'Trošak je obrisan.');

          } : undefined}

          readOnly={!canEdit}

        />

        {canEdit && (

          <ExpenseForm onSubmit={(payload) => runAction(

            () => shareService.addSharedExpense(token, payload),

            'Trošak je dodat.',

          )} />

        )}

      </section>



      <section className="section-block">

        <h2>Destinacije</h2>

        <DestinationList

          destinations={destinations}

          onDelete={canEdit ? (id) => {

            if (!window.confirm('Obrisati destinaciju?')) return;

            runAction(() => shareService.deleteSharedDestination(token, id), 'Destinacija je obrisana.');

          } : undefined}

          readOnly={!canEdit}

        />

        {canEdit && (

          <DestinationForm onSubmit={(payload) => runAction(

            () => shareService.addSharedDestination(token, payload),

            'Destinacija je dodata.',

          )} />

        )}

      </section>



      <section className="section-block">

        <h2>Aktivnosti po danima</h2>

        <ActivityList

          activities={activities}

          onDelete={canEdit ? (id) => {

            if (!window.confirm('Obrisati aktivnost?')) return;

            runAction(() => shareService.deleteSharedActivity(token, id), 'Aktivnost je obrisana.');

          } : undefined}

          readOnly={!canEdit}

        />

        {canEdit && (

          <ActivityForm

            destinations={destinations}

            onSubmit={(payload) => runAction(

              () => shareService.addSharedActivity(token, payload),

              'Aktivnost je dodata.',

            )}

          />

        )}

      </section>



      <section className="section-block">

        <h2>Packing lista</h2>

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

      </section>

    </div>

  );

}


