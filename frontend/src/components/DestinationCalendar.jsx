import { useMemo, useState } from 'react';
import DestinationList from './DestinationList';
import {
  buildDestinationsByDate,
  formatMonthTitle,
  getInitialMonth,
  getMonthMatrix,
  isDateInRange,
  toDateKey,
  WEEKDAY_LABELS,
} from '../utils/calendarUtils';

export default function DestinationCalendar({
  destinations,
  planStartDate,
  planEndDate,
  selectedDate,
  onSelectedDateChange,
  onDelete,
  onEdit,
  readOnly = false,
}) {
  const initial = getInitialMonth(planStartDate, planEndDate);
  const [year, setYear] = useState(initial.year);
  const [month, setMonth] = useState(initial.month);

  const destinationsByDate = useMemo(
    () => buildDestinationsByDate(destinations, planStartDate, planEndDate),
    [destinations, planStartDate, planEndDate],
  );

  const cells = getMonthMatrix(year, month);
  const monthStart = toDateKey(year, month, 1);
  const monthEnd = toDateKey(year, month, new Date(year, month + 1, 0).getDate());

  const canGoPrev = monthStart > planStartDate;
  const canGoNext = monthEnd < planEndDate;

  function goPrevMonth() {
    if (!canGoPrev) return;
    if (month === 0) {
      setYear((y) => y - 1);
      setMonth(11);
    } else {
      setMonth((m) => m - 1);
    }
    onSelectedDateChange(null);
  }

  function goNextMonth() {
    if (!canGoNext) return;
    if (month === 11) {
      setYear((y) => y + 1);
      setMonth(0);
    } else {
      setMonth((m) => m + 1);
    }
    onSelectedDateChange(null);
  }

  function handleDayClick(day) {
    const dateKey = toDateKey(year, month, day);
    if (!isDateInRange(dateKey, planStartDate, planEndDate)) return;
    onSelectedDateChange(selectedDate === dateKey ? null : dateKey);
  }

  const selectedDestinations = selectedDate ? (destinationsByDate[selectedDate] ?? []) : [];

  return (
    <div className="month-calendar">
      <div className="month-calendar-toolbar">
        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={goPrevMonth}
          disabled={!canGoPrev}
          aria-label="Prethodni mesec"
        >
          ←
        </button>
        <h3 className="month-calendar-title">{formatMonthTitle(year, month)}</h3>
        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={goNextMonth}
          disabled={!canGoNext}
          aria-label="Sledeći mesec"
        >
          →
        </button>
      </div>

      <p className="muted month-calendar-hint">
        Period putovanja: {planStartDate} → {planEndDate}. Klikni na dan da vidiš destinacije.
      </p>

      <div className="month-calendar-weekdays">
        {WEEKDAY_LABELS.map((label) => (
          <span key={label} className="month-calendar-weekday">{label}</span>
        ))}
      </div>

      <div className="month-calendar-grid">
        {cells.map((day, index) => {
          if (day === null) {
            return <div key={`empty-${index}`} className="month-calendar-cell empty" />;
          }

          const dateKey = toDateKey(year, month, day);
          const inRange = isDateInRange(dateKey, planStartDate, planEndDate);
          const dayDestinations = destinationsByDate[dateKey] ?? [];
          const isSelected = selectedDate === dateKey;
          const isToday = dateKey === toDateKey(
            new Date().getFullYear(),
            new Date().getMonth(),
            new Date().getDate(),
          );

          return (
            <button
              key={dateKey}
              type="button"
              className={[
                'month-calendar-cell',
                inRange ? 'in-range' : 'out-of-range',
                isSelected ? 'selected' : '',
                isToday ? 'today' : '',
                dayDestinations.length > 0 ? 'has-destinations' : '',
              ].filter(Boolean).join(' ')}
              onClick={() => handleDayClick(day)}
              disabled={!inRange}
            >
              <span className="month-calendar-day-num">{day}</span>
              {dayDestinations.length > 0 && (
                <span className="month-calendar-count">{dayDestinations.length}</span>
              )}
            </button>
          );
        })}
      </div>

      {selectedDate && (
        <div className="month-calendar-day-detail card">
          <h4>Destinacije — {selectedDate}</h4>
          {selectedDestinations.length === 0 ? (
            <p className="muted">Nema destinacija za ovaj dan.</p>
          ) : (
            <DestinationList
              destinations={selectedDestinations}
              onDelete={onDelete}
              onEdit={onEdit}
              readOnly={readOnly}
            />
          )}
        </div>
      )}
    </div>
  );
}
