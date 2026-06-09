export default function ActivityList({
  activities,
  onDelete,
  onEdit,
  readOnly = false,
  compact = false,
}) {
  if (activities.length === 0) {
    return <p className="muted">Još nema aktivnosti za ovaj plan.</p>;
  }

  const grouped = activities.reduce((acc, activity) => {
    const key = activity.activityDate;
    if (!acc[key]) acc[key] = [];
    acc[key].push(activity);
    return acc;
  }, {});

  const dates = Object.keys(grouped).sort();

  function renderActivity(activity) {
    return (
      <article key={activity.id} className="activity-card">
        <div>
          <strong>{activity.name}</strong>
          {activity.activityTime && <p className="muted">Vreme: {activity.activityTime}</p>}
          {activity.estimatedCost != null && activity.estimatedCost > 0 && (
            <p className="muted">Procijenjeno: {activity.estimatedCost} €</p>
          )}
          {activity.description && <p>{activity.description}</p>}
        </div>
        {!readOnly && (onEdit || onDelete) && (
          <div className="card-actions">
            {onEdit && (
              <button type="button" className="btn btn-secondary btn-sm" onClick={() => onEdit(activity)}>
                Izmeni
              </button>
            )}
            {onDelete && (
              <button type="button" className="btn btn-danger btn-sm" onClick={() => onDelete(activity.id)}>
                Obriši
              </button>
            )}
          </div>
        )}
      </article>
    );
  }

  if (compact) {
    return (
      <div className="activity-list">
        {activities.map(renderActivity)}
      </div>
    );
  }

  return (
    <div className="activity-calendar">
      {dates.map((date) => (
        <section key={date} className="calendar-day card">
          <h3 className="calendar-date">{date}</h3>
          <div className="activity-list">
            {grouped[date].map(renderActivity)}
          </div>
        </section>
      ))}
    </div>
  );
}
