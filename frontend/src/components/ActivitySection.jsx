import { useState } from 'react';
import ActivityCalendar from './ActivityCalendar';
import ActivityForm, { activityToFormValues } from './ActivityForm';
import ActivityList from './ActivityList';

export default function ActivitySection({
  activities,
  destinations,
  plan,
  onDelete,
  onSubmit,
  onUpdate,
  readOnly = false,
}) {
  const [view, setView] = useState('calendar');
  const [selectedCalendarDate, setSelectedCalendarDate] = useState(null);
  const [editingActivity, setEditingActivity] = useState(null);

  function handleViewChange(nextView) {
    setView(nextView);
    if (nextView === 'list') {
      setSelectedCalendarDate(null);
    }
    setEditingActivity(null);
  }

  function handleEdit(activity) {
    setEditingActivity(activity);
    if (view === 'calendar') {
      setSelectedCalendarDate(activity.activityDate);
    }
  }

  async function handleFormSubmit(payload) {
    if (editingActivity && onUpdate) {
      await onUpdate(editingActivity.id, payload);
      setEditingActivity(null);
      return;
    }

    if (onSubmit) {
      await onSubmit(payload);
    }
  }

  const showForm = !readOnly && (onSubmit || onUpdate) && (
    view === 'list' || selectedCalendarDate || editingActivity
  );

  return (
    <section className="section-block">
      <div className="section-header-row">
        <h2>Aktivnosti po danima</h2>
        <div className="view-toggle" role="tablist" aria-label="Prikaz aktivnosti">
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
        <ActivityCalendar
          activities={activities}
          planStartDate={plan.startDate}
          planEndDate={plan.endDate}
          selectedDate={selectedCalendarDate}
          onSelectedDateChange={(date) => {
            setSelectedCalendarDate(date);
            setEditingActivity(null);
          }}
          onDelete={onDelete}
          onEdit={readOnly ? undefined : handleEdit}
          readOnly={readOnly}
        />
      ) : (
        <ActivityList
          activities={activities}
          onDelete={onDelete}
          onEdit={readOnly ? undefined : handleEdit}
          readOnly={readOnly}
        />
      )}

      {!readOnly && view === 'calendar' && !selectedCalendarDate && !editingActivity && (
        <p className="muted calendar-form-hint">Izaberi dan u kalendaru da dodaš aktivnost.</p>
      )}

      {showForm && (
        <ActivityForm
          key={editingActivity?.id ?? `new-${view}-${selectedCalendarDate ?? 'list'}`}
          destinations={destinations}
          fixedDate={!editingActivity && view === 'calendar' ? selectedCalendarDate : null}
          initialValues={editingActivity ? activityToFormValues(editingActivity) : null}
          onSubmit={handleFormSubmit}
          onCancel={editingActivity ? () => setEditingActivity(null) : undefined}
        />
      )}
    </section>
  );
}
