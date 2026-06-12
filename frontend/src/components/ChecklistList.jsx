export default function ChecklistList({
  items,
  onToggle,
  onDelete,
  readOnly = false,
  emptyMessage = 'Nema stavki.',
}) {
  if (items.length === 0) {
    return <p className="muted">{emptyMessage}</p>;
  }

  return (
    <ul className="checklist-list">
      {items.map((item) => (
        <li key={item.id} className={item.isCompleted ? 'completed' : ''}>
          <label className="checklist-item">
            <input
              type="checkbox"
              checked={item.isCompleted}
              disabled={readOnly || !onToggle}
              onChange={() => onToggle?.(item.id)}
            />
            <span>{item.title}</span>
          </label>
          {!readOnly && onDelete && (
          <button type="button" className="btn btn-danger btn-sm" onClick={() => onDelete(item.id)}>
            Obriši
          </button>
          )}
        </li>
      ))}
    </ul>
  );
}
