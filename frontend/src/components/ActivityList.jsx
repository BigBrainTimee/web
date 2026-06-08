const STATUS_LABELS = {
  Planned: 'Planirano',
  Reserved: 'Rezervisano',
  Completed: 'Završeno',
  Cancelled: 'Otkazano',
};

export default function ActivityList({ activities, onDelete, readOnly = false }) {
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

  return (
    <div className="activity-calendar">
      {dates.map((date) => (
        <section key={date} className="calendar-day card">
          <h3 className="calendar-date">{date}</h3>
          <div className="activity-list">
            {grouped[date].map((activity) => (
              <article key={activity.id} className="activity-card">
                <div>
                  <strong>{activity.name}</strong>
                  <span className={`status-badge status-${activity.status.toLowerCase()}`}>
                    {STATUS_LABELS[activity.status] ?? activity.status}
                  </span>
                  {activity.activityTime && <p className="muted">Vreme: {activity.activityTime}</p>}
                  {activity.location && <p className="muted">{activity.location}</p>}
                  {activity.estimatedCost != null && <p>Proc. trošak: {activity.estimatedCost} €</p>}
                  {activity.description && <p>{activity.description}</p>}
                </div>
                {!readOnly && onDelete && (
                <button type="button" className="btn btn-danger" onClick={() => onDelete(activity.id)}>
                  Obriši
                </button>
                )}
              </article>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}
