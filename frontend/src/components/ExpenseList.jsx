import { getExpenseCategoryLabel } from '../models/Expense';

export default function ExpenseList({ expenses, onDelete, readOnly = false }) {
  if (expenses.length === 0) {
    return <p className="muted">Nema unetih troškova.</p>;
  }

  return (
    <ul className="item-list">
      {expenses.map((expense) => (
        <li key={expense.id} className="item-row">
          <div>
            <strong>{expense.name}</strong>
            <span className="badge">{getExpenseCategoryLabel(expense.category)}</span>
            <p className="muted">{expense.expenseDate} · {expense.amount.toFixed(2)} €</p>
            {expense.description && <p>{expense.description}</p>}
          </div>
          {!readOnly && onDelete && (
          <button type="button" className="btn btn-danger btn-sm" onClick={() => onDelete(expense.id)}>
            Obriši
          </button>
          )}
        </li>
      ))}
    </ul>
  );
}
