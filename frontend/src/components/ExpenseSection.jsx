import { useState } from 'react';
import ExpenseCalendar from './ExpenseCalendar';
import ExpenseForm from './ExpenseForm';
import ExpenseList from './ExpenseList';

export default function ExpenseSection({
  expenses,
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

  const showForm = !readOnly && onSubmit && view === 'calendar' && selectedCalendarDate;

  return (
    <section className="expenses-column section-block">
      <div className="section-header-row">
        <h3>Sigurni troškovi</h3>
        <div className="view-toggle" role="tablist" aria-label="Prikaz troškova">
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

      <p className="muted expenses-column-hint">Stvarno uneti troškovi koji se računaju u budžet.</p>

      {view === 'calendar' ? (
        <ExpenseCalendar
          expenses={expenses}
          planStartDate={plan.startDate}
          planEndDate={plan.endDate}
          selectedDate={selectedCalendarDate}
          onSelectedDateChange={setSelectedCalendarDate}
          onDelete={onDelete}
          readOnly={readOnly}
        />
      ) : (
        <ExpenseList
          expenses={expenses}
          onDelete={onDelete}
          readOnly={readOnly}
        />
      )}

      {!readOnly && view === 'calendar' && !selectedCalendarDate && (
        <p className="muted calendar-form-hint">Izaberi dan u kalendaru da dodaš trošak.</p>
      )}

      {showForm && (
        <ExpenseForm
          key={`new-${selectedCalendarDate}`}
          planStartDate={plan.startDate}
          planEndDate={plan.endDate}
          fixedDate={selectedCalendarDate}
          onSubmit={onSubmit}
        />
      )}
    </section>
  );
}
