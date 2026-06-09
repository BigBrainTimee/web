const WEEKDAY_LABELS = ['Po', 'Ut', 'Sr', 'Če', 'Pe', 'Su', 'Ne'];

const MONTH_LABELS = [
  'Januar', 'Februar', 'Mart', 'April', 'Maj', 'Jun',
  'Jul', 'Avgust', 'Septembar', 'Oktobar', 'Novembar', 'Decembar',
];

export function parseDateKey(dateKey) {
  const [year, month, day] = dateKey.split('-').map(Number);
  return { year, month: month - 1, day };
}

export function toDateKey(year, month, day) {
  const m = String(month + 1).padStart(2, '0');
  const d = String(day).padStart(2, '0');
  return `${year}-${m}-${d}`;
}

export function isDateInRange(dateKey, startDate, endDate) {
  return dateKey >= startDate && dateKey <= endDate;
}

export function getMonthMatrix(year, month) {
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const firstWeekday = new Date(year, month, 1).getDay();
  const leadingEmpty = firstWeekday === 0 ? 6 : firstWeekday - 1;

  const cells = [];
  for (let i = 0; i < leadingEmpty; i += 1) {
    cells.push(null);
  }
  for (let day = 1; day <= daysInMonth; day += 1) {
    cells.push(day);
  }
  return cells;
}

export function getInitialMonth(startDate, endDate) {
  const today = new Date();
  const todayKey = toDateKey(today.getFullYear(), today.getMonth(), today.getDate());

  if (isDateInRange(todayKey, startDate, endDate)) {
    return { year: today.getFullYear(), month: today.getMonth() };
  }

  return parseDateKey(startDate);
}

export function formatMonthTitle(year, month) {
  return `${MONTH_LABELS[month]} ${year}`;
}

export function addDays(dateKey, days) {
  const { year, month, day } = parseDateKey(dateKey);
  const next = new Date(year, month, day + days);
  return toDateKey(next.getFullYear(), next.getMonth(), next.getDate());
}

export function isDateInSpan(dateKey, startDate, endDate) {
  return dateKey >= startDate && dateKey <= endDate;
}

export function getDestinationsForDate(destinations, dateKey) {
  if (!dateKey) return [];

  return destinations.filter(
    (destination) => dateKey >= destination.arrivalDate && dateKey <= destination.departureDate,
  );
}

export function buildDestinationsByDate(destinations, planStartDate, planEndDate) {
  return destinations.reduce((acc, destination) => {
    const spanStart = destination.arrivalDate < planStartDate ? planStartDate : destination.arrivalDate;
    const spanEnd = destination.departureDate > planEndDate ? planEndDate : destination.departureDate;

    if (spanStart > spanEnd) {
      return acc;
    }

    let current = spanStart;
    while (current <= spanEnd) {
      if (!acc[current]) acc[current] = [];
      acc[current].push(destination);
      current = addDays(current, 1);
    }

    return acc;
  }, {});
}

export { WEEKDAY_LABELS };
