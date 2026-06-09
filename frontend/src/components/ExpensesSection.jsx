import EstimatedExpenseList from './EstimatedExpenseList';
import ExpenseForm from './ExpenseForm';
import ExpenseList from './ExpenseList';

export default function ExpensesSection({
  expenses,
  activities,
  onAddExpense,
  onDeleteExpense,
  readOnly = false,
}) {
  return (
    <div className="expenses-split">
      <section className="expenses-column">
        <h3>Sigurni troškovi</h3>
        <p className="muted expenses-column-hint">Stvarno unijeti troškovi koji se računaju u budžet.</p>
        <ExpenseList
          expenses={expenses}
          onDelete={readOnly ? undefined : onDeleteExpense}
          readOnly={readOnly}
        />
        {!readOnly && onAddExpense && (
          <ExpenseForm onSubmit={onAddExpense} />
        )}
      </section>

      <section className="expenses-column">
        <h3>Procijenjeni troškovi</h3>
        <p className="muted expenses-column-hint">
          Automatski iz aktivnosti — dodaj procijenjeni iznos u sekciji Aktivnosti.
        </p>
        <EstimatedExpenseList activities={activities} />
      </section>
    </div>
  );
}
