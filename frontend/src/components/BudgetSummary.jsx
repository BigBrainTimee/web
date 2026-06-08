export default function BudgetSummary({ summary }) {
  if (!summary) {
    return null;
  }

  const overBudget = summary.remaining < 0;
  const usagePercent = summary.plannedBudget > 0
    ? Math.min(100, (summary.totalSpent / summary.plannedBudget) * 100)
    : 0;

  return (
    <section className="card budget-summary">
      <h2>Pregled budžeta</h2>

      <div className="budget-stats">
        <div>
          <span className="stat-label">Planirano</span>
          <strong>{summary.plannedBudget.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Potrošeno</span>
          <strong>{summary.totalSpent.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Preostalo</span>
          <strong className={overBudget ? 'text-danger' : 'text-success'}>
            {summary.remaining.toFixed(2)} €
          </strong>
        </div>
      </div>

      <div className="budget-bar">
        <div
          className={`budget-bar-fill ${overBudget ? 'over-budget' : ''}`}
          style={{ width: `${usagePercent}%` }}
        />
      </div>

      {summary.byCategory.length > 0 && (
        <ul className="category-breakdown">
          {summary.byCategory.map((item) => (
            <li key={item.category}>
              <span>{item.category}</span>
              <strong>{item.amount.toFixed(2)} €</strong>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
