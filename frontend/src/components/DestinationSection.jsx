import { useState } from 'react';
import DestinationCalendar from './DestinationCalendar';
import DestinationForm, { destinationToFormValues } from './DestinationForm';
import DestinationList from './DestinationList';

export default function DestinationSection({
  destinations,
  plan,
  onDelete,
  onSubmit,
  onUpdate,
  readOnly = false,
}) {
  const [view, setView] = useState('calendar');
  const [selectedCalendarDate, setSelectedCalendarDate] = useState(null);
  const [editingDestination, setEditingDestination] = useState(null);

  function handleViewChange(nextView) {
    setView(nextView);
    if (nextView === 'list') {
      setSelectedCalendarDate(null);
    }
    setEditingDestination(null);
  }

  function handleEdit(destination) {
    setEditingDestination(destination);
    if (view === 'calendar') {
      setSelectedCalendarDate(destination.arrivalDate);
    }
  }

  async function handleFormSubmit(payload) {
    if (editingDestination && onUpdate) {
      await onUpdate(editingDestination.id, payload);
      setEditingDestination(null);
      return;
    }

    if (onSubmit) {
      await onSubmit(payload);
    }
  }

  const showForm = !readOnly && (onSubmit || onUpdate) && (
    editingDestination || (view === 'calendar' && selectedCalendarDate)
  );

  return (
    <section className="section-block">
      <div className="section-header-row">
        <h2>Destinacije</h2>
        <div className="view-toggle" role="tablist" aria-label="Prikaz destinacija">
          <button
            type="button"
            role="tab"
            aria-selected={view === 'calendar'}
            className={view === 'calendar' ? 'active' : ''}
            onClick={() => handleViewChange('calendar')}
          >
            Kalendar
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={view === 'list'}
            className={view === 'list' ? 'active' : ''}
            onClick={() => handleViewChange('list')}
          >
            Lista
          </button>
        </div>
      </div>

      {view === 'calendar' ? (
        <DestinationCalendar
          destinations={destinations}
          planStartDate={plan.startDate}
          planEndDate={plan.endDate}
          selectedDate={selectedCalendarDate}
          onSelectedDateChange={(date) => {
            setSelectedCalendarDate(date);
            setEditingDestination(null);
          }}
          onDelete={onDelete}
          onEdit={readOnly ? undefined : handleEdit}
          readOnly={readOnly}
        />
      ) : (
        <DestinationList
          destinations={destinations}
          onDelete={onDelete}
          onEdit={readOnly ? undefined : handleEdit}
          readOnly={readOnly}
        />
      )}

      {!readOnly && view === 'calendar' && !selectedCalendarDate && !editingDestination && (
        <p className="muted calendar-form-hint">Izaberi dan u kalendaru da vidiš ili dodaš destinaciju.</p>
      )}

      {showForm && (
        <DestinationForm
          key={editingDestination?.id ?? `new-${view}-${selectedCalendarDate ?? 'list'}`}
          planStartDate={plan.startDate}
          planEndDate={plan.endDate}
          fixedArrivalDate={!editingDestination && view === 'calendar' ? selectedCalendarDate : null}
          initialValues={editingDestination ? destinationToFormValues(editingDestination) : null}
          onSubmit={handleFormSubmit}
          onCancel={editingDestination ? () => setEditingDestination(null) : undefined}
        />
      )}
    </section>
  );
}
