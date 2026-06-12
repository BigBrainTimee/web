export default function EstimatedExpenseList({ activities }) {
  const estimatedActivities = activities.filter(
    (activity) => activity.estimatedCost != null && activity.estimatedCost > 0,
  );

  if (estimatedActivities.length === 0) {
    return <p className="muted">Nema procenjenih troškova iz aktivnosti.</p>;
  }

  const sorted = [...estimatedActivities].sort((a, b) => {
    if (a.activityDate === b.activityDate) {
      return a.name.localeCompare(b.name);
    }
    return a.activityDate.localeCompare(b.activityDate);
  });

  return (
    <ul className="item-list estimated-expense-list">
      {sorted.map((activity) => (
        <li key={activity.id} className="item-row estimated-expense-row">
          <div>
            <strong>{activity.name}</strong>
            <span className="badge badge-estimated">Procenjeno</span>
            <p className="muted">{activity.activityDate} · {activity.estimatedCost.toFixed(2)} €</p>
          </div>
        </li>
      ))}
    </ul>
  );
}
