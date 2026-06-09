import { enrichBudgetSummary, getEstimatedActivities } from '../utils/budgetUtils';

export default function BudgetSummary({ summary, activities = [] }) {
  const enriched = enrichBudgetSummary(summary, activities);

  if (!enriched) {
    return null;
  }

  const estimatedActivities = getEstimatedActivities(activities);
  const overBudget = enriched.remaining < 0;
  const usagePercent = enriched.plannedBudget > 0
    ? Math.min(100, (enriched.totalUsed / enriched.plannedBudget) * 100)
    : 0;

  return (
    <section className="card budget-summary">
      <h2>Pregled budžeta</h2>

      <div className="budget-stats">
        <div>
          <span className="stat-label">Planirano</span>
          <strong>{enriched.plannedBudget.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Sigurni troškovi</span>
          <strong>{enriched.totalSpent.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Procijenjeni</span>
          <strong>{enriched.totalEstimated.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Ukupno</span>
          <strong>{enriched.totalUsed.toFixed(2)} €</strong>
        </div>
        <div>
          <span className="stat-label">Preostalo</span>
          <strong className={overBudget ? 'text-danger' : 'text-success'}>
            {enriched.remaining.toFixed(2)} €
          </strong>
        </div>
      </div>

      <div className="budget-bar">
        <div
          className={`budget-bar-fill ${overBudget ? 'over-budget' : ''}`}
          style={{ width: `${usagePercent}%` }}
        />
      </div>

      {enriched.byCategory.length > 0 && (
        <>
          <h3 className="budget-subtitle">Sigurni troškovi po kategorijama</h3>
          <ul className="category-breakdown">
            {enriched.byCategory.map((item) => (
              <li key={item.category}>
                <span>{item.category}</span>
                <strong>{item.amount.toFixed(2)} €</strong>
              </li>
            ))}
          </ul>
        </>
      )}

      {estimatedActivities.length > 0 && (
        <>
          <h3 className="budget-subtitle">Procijenjeni troškovi po aktivnostima</h3>
          <ul className="category-breakdown estimated-breakdown">
            {estimatedActivities.map((activity) => (
              <li key={activity.id}>
                <span className="estimated-breakdown-label">
                  {activity.name}
                  <span className="badge badge-estimated">procijenjeno</span>
                  <span className="muted estimated-breakdown-date">{activity.activityDate}</span>
                </span>
                <strong>{activity.estimatedCost.toFixed(2)} €</strong>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
