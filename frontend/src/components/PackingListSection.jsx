import { splitPackingItems } from '../constants/checklistDefaults';
import ChecklistForm from './ChecklistForm';
import ChecklistList from './ChecklistList';

export default function PackingListSection({
  items,
  onToggle,
  onDelete,
  onAdd,
  readOnly = false,
}) {
  const { defaults, custom } = splitPackingItems(items);
  const packedCount = defaults.filter((item) => item.isCompleted).length;

  return (
    <div className="packing-list-section">
      <h3 className="packing-subtitle">Osnovne stavke</h3>
      <p className="muted packing-hint">
        Označi šta si spakovao pre puta.
        {defaults.length > 0 && (
          <span className="packing-progress"> ({packedCount} od {defaults.length} označeno)</span>
        )}
      </p>
      <ChecklistList
        items={defaults}
        onToggle={onToggle}
        onDelete={readOnly ? undefined : onDelete}
        readOnly={readOnly}
        emptyMessage="Osnovne stavke se učitavaju..."
      />

      <h3 className="packing-subtitle">Ostalo</h3>
      <p className="muted packing-hint">
        Sopstvene stavke koje nisu u osnovnoj listi — unesi naziv ispod i klikni „Dodaj u Ostalo“.
      </p>

      {!readOnly && onAdd && (
        <ChecklistForm onSubmit={onAdd} />
      )}

      {custom.length > 0 && (
        <ChecklistList
          items={custom}
          onToggle={onToggle}
          onDelete={readOnly ? undefined : onDelete}
          readOnly={readOnly}
        />
      )}
    </div>
  );
}
