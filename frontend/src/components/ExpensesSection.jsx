import EstimatedExpenseList from './EstimatedExpenseList';
import ExpenseSection from './ExpenseSection';

export default function ExpensesSection({
  expenses,
  activities,
  plan,
  onAddExpense,
  onDeleteExpense,
  readOnly = false,
}) {
  return (
    <div className="expenses-split">
      <ExpenseSection
        expenses={expenses}
        plan={plan}
        onDelete={readOnly ? undefined : onDeleteExpense}
        onSubmit={readOnly ? undefined : onAddExpense}
        readOnly={readOnly}
      />

      <section className="expenses-column">
        <h3>Procenjeni troškovi</h3>
        <p className="muted expenses-column-hint">
          Automatski iz aktivnosti — dodaj procenjeni iznos u sekciji Aktivnosti.
        </p>
        <EstimatedExpenseList activities={activities} />
      </section>
    </div>
  );
}
