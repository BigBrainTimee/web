import { useState } from 'react';
import ActivityCalendar from './ActivityCalendar';
import ActivityForm from './ActivityForm';
import ActivityList from './ActivityList';

export default function ActivitySection({
  activities,
  destinations,
  plan,
  onDelete,
  onSubmit,
  readOnly = false,
}) {
  const [view, setView] = useState('calendar');
  const [selectedCalendarDate, setSelectedCalendarDate] = useState(null);

  function handleViewChange(nextView) {
    setView(nextView);
    if (nextView === 'list') {
      setSelectedCalendarDate(null);
    }
  }

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
          onSelectedDateChange={setSelectedCalendarDate}
          onDelete={onDelete}
          readOnly={readOnly}
        />
      ) : (
        <ActivityList
          activities={activities}
          onDelete={onDelete}
          readOnly={readOnly}
        />
      )}

      {!readOnly && onSubmit && view === 'calendar' && !selectedCalendarDate && (
        <p className="muted calendar-form-hint">Izaberi dan u kalendaru da dodaš aktivnost.</p>
      )}

      {!readOnly && onSubmit && (view === 'list' || selectedCalendarDate) && (
        <ActivityForm
          destinations={destinations}
          fixedDate={view === 'calendar' ? selectedCalendarDate : null}
          onSubmit={onSubmit}
        />
      )}
    </section>
  );
}
