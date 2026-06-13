import { useEffect, useId, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import {
  formatMonthTitle,
  getMonthMatrix,
  parseDateKey,
  toDateKey,
  WEEKDAY_LABELS,
} from '../utils/calendarUtils';

const POPOVER_WIDTH = 300;
const POPOVER_ESTIMATED_HEIGHT = 340;
const VIEWPORT_MARGIN = 12;

function formatDisplayDate(dateKey) {
  if (!dateKey) return '';
  const { year, month, day } = parseDateKey(dateKey);
  return `${String(day).padStart(2, '0')}.${String(month + 1).padStart(2, '0')}.${year}`;
}

function resolveVisibleMonth(openToDate, value, minDate, maxDate) {
  const key = openToDate || value || minDate || maxDate;
  if (key) {
    return parseDateKey(key);
  }

  const today = new Date();
  return { year: today.getFullYear(), month: today.getMonth() };
}

function isDaySelectable(dateKey, minDate, maxDate) {
  if (minDate && dateKey < minDate) return false;
  if (maxDate && dateKey > maxDate) return false;
  return true;
}

function getPopoverPosition(triggerElement) {
  const rect = triggerElement.getBoundingClientRect();
  let left = rect.left;
  let top = rect.bottom + 6;

  if (left + POPOVER_WIDTH > window.innerWidth - VIEWPORT_MARGIN) {
    left = window.innerWidth - POPOVER_WIDTH - VIEWPORT_MARGIN;
  }
  left = Math.max(VIEWPORT_MARGIN, left);

  if (top + POPOVER_ESTIMATED_HEIGHT > window.innerHeight - VIEWPORT_MARGIN) {
    top = rect.top - POPOVER_ESTIMATED_HEIGHT - 6;
  }
  top = Math.max(VIEWPORT_MARGIN, top);

  return { top, left };
}

export default function DatePickerField({
  id,
  name,
  label,
  value,
  onChange,
  error,
  minDate,
  maxDate,
  openToDate,
  readOnly = false,
  placeholder = 'Izaberi datum',
}) {
  const generatedId = useId();
  const fieldId = id ?? `${generatedId}-${name ?? 'date'}`;
  const containerRef = useRef(null);
  const triggerRef = useRef(null);
  const popoverRef = useRef(null);
  const [isOpen, setIsOpen] = useState(false);
  const [popoverStyle, setPopoverStyle] = useState(null);
  const [year, setYear] = useState(() => resolveVisibleMonth(openToDate, value, minDate, maxDate).year);
  const [month, setMonth] = useState(() => resolveVisibleMonth(openToDate, value, minDate, maxDate).month);

  useLayoutEffect(() => {
    if (!isOpen || !triggerRef.current) return undefined;

    function updatePosition() {
      if (!triggerRef.current) return;
      const { top, left } = getPopoverPosition(triggerRef.current);
      setPopoverStyle({ top, left });
    }

    updatePosition();
    window.addEventListener('resize', updatePosition);
    window.addEventListener('scroll', updatePosition, true);

    return () => {
      window.removeEventListener('resize', updatePosition);
      window.removeEventListener('scroll', updatePosition, true);
    };
  }, [isOpen, year, month]);

  useEffect(() => {
    if (!isOpen) return undefined;

    const visibleMonth = resolveVisibleMonth(openToDate, value, minDate, maxDate);
    setYear(visibleMonth.year);
    setMonth(visibleMonth.month);

    function handlePointerDown(event) {
      const inTrigger = containerRef.current?.contains(event.target);
      const inPopover = popoverRef.current?.contains(event.target);
      if (!inTrigger && !inPopover) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handlePointerDown);
    return () => document.removeEventListener('mousedown', handlePointerDown);
  }, [isOpen, openToDate, value, minDate, maxDate]);

  function togglePicker() {
    if (readOnly) return;
    setIsOpen((prev) => !prev);
  }

  function handleDayClick(day) {
    const dateKey = toDateKey(year, month, day);
    if (!isDaySelectable(dateKey, minDate, maxDate)) return;
    onChange(dateKey);
    setIsOpen(false);
  }

  function goPrevMonth() {
    if (month === 0) {
      setYear((currentYear) => currentYear - 1);
      setMonth(11);
      return;
    }
    setMonth((currentMonth) => currentMonth - 1);
  }

  function goNextMonth() {
    if (month === 11) {
      setYear((currentYear) => currentYear + 1);
      setMonth(0);
      return;
    }
    setMonth((currentMonth) => currentMonth + 1);
  }

  const cells = getMonthMatrix(year, month);
  const todayKey = toDateKey(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());

  const popover = isOpen && popoverStyle
    ? createPortal(
      <div
        ref={popoverRef}
        className="date-picker-popover month-calendar date-picker-calendar"
        role="dialog"
        aria-label={label}
        style={{
          position: 'fixed',
          top: `${popoverStyle.top}px`,
          left: `${popoverStyle.left}px`,
          width: `${POPOVER_WIDTH}px`,
        }}
      >
        <div className="month-calendar-toolbar">
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={goPrevMonth}
            aria-label="Prethodni mesec"
          >
            ‹
          </button>
          <h3 className="month-calendar-title">{formatMonthTitle(year, month)}</h3>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={goNextMonth}
            aria-label="Sledeći mesec"
          >
            ›
          </button>
        </div>

        <div className="month-calendar-weekdays">
          {WEEKDAY_LABELS.map((weekdayLabel) => (
            <span key={weekdayLabel} className="month-calendar-weekday">{weekdayLabel}</span>
          ))}
        </div>

        <div className="month-calendar-grid">
          {cells.map((day, index) => {
            if (day === null) {
              return <div key={`empty-${index}`} className="month-calendar-cell empty" />;
            }

            const dateKey = toDateKey(year, month, day);
            const selectable = isDaySelectable(dateKey, minDate, maxDate);

            return (
              <button
                key={dateKey}
                type="button"
                className={[
                  'month-calendar-cell',
                  selectable ? 'in-range' : 'out-of-range',
                  value === dateKey ? 'selected' : '',
                  todayKey === dateKey ? 'today' : '',
                ].filter(Boolean).join(' ')}
                onClick={() => handleDayClick(day)}
                disabled={!selectable}
                aria-label={dateKey}
                aria-pressed={value === dateKey}
              >
                <span className="month-calendar-day-num">{day}</span>
              </button>
            );
          })}
        </div>
      </div>,
      document.body,
    )
    : null;

  return (
    <div className="date-picker-field" ref={containerRef}>
      <label htmlFor={fieldId}>{label}</label>
      <input type="hidden" id={fieldId} name={name} value={value} readOnly />
      <button
        ref={triggerRef}
        type="button"
        className={`date-picker-trigger${error ? ' has-error' : ''}${!value ? ' is-empty' : ''}`}
        onClick={togglePicker}
        disabled={readOnly}
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        aria-label={value ? `${label}: ${formatDisplayDate(value)}` : `${label}: ${placeholder}`}
      >
        {value ? formatDisplayDate(value) : placeholder}
      </button>

      {popover}

      {error && <span className="field-error">{error}</span>}
    </div>
  );
}
