import { useEffect, useState } from 'react';

const emptyForm = {
  name: '',
  activityDate: '',
  activityTime: '',
  description: '',
  estimatedCost: '',
  destinationId: '',
};

export function activityToFormValues(activity) {
  return {
    name: activity.name,
    activityDate: activity.activityDate,
    activityTime: activity.activityTime ?? '',
    description: activity.description ?? '',
    estimatedCost: activity.estimatedCost ?? '',
    destinationId: activity.destinationId ?? '',
  };
}

function buildEmptyForm(fixedDate, defaultDestinationId) {
  const form = fixedDate ? { ...emptyForm, activityDate: fixedDate } : { ...emptyForm };

  if (defaultDestinationId) {
    form.destinationId = String(defaultDestinationId);
  }

  return form;
}

export default function ActivityForm({
  destinations,
  onSubmit,
  onCancel,
  fixedDate = null,
  defaultDestinationId = null,
  planStartDate = null,
  planEndDate = null,
  initialValues = null,
  submitLabel,
}) {
  const isEditing = Boolean(initialValues);
  const [form, setForm] = useState(() => {
    if (initialValues) return initialValues;
    return buildEmptyForm(fixedDate, defaultDestinationId);
  });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (initialValues) {
      setForm(initialValues);
      setErrors({});
      return;
    }

    setForm(buildEmptyForm(fixedDate, defaultDestinationId));
    setErrors({});
  }, [initialValues, fixedDate, defaultDestinationId]);

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const nextErrors = {};
    const trimmedName = form.name.trim();

    if (!trimmedName) nextErrors.name = 'Naziv je obavezan.';
    else if (trimmedName.length < 2) nextErrors.name = 'Naziv mora imati bar 2 karaktera.';
    if (!form.activityDate) nextErrors.activityDate = 'Datum je obavezan.';
    if (planStartDate && form.activityDate && form.activityDate < planStartDate) {
      nextErrors.activityDate = `Datum mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }
    if (planEndDate && form.activityDate && form.activityDate > planEndDate) {
      nextErrors.activityDate = `Datum mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }
    if (form.estimatedCost !== '' && Number(form.estimatedCost) < 0) {
      nextErrors.estimatedCost = 'Trošak ne može biti negativan.';
    }
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    const payload = {
      name: form.name.trim(),
      activityDate: form.activityDate,
      status: 'Planned',
    };

    if (form.activityTime) {
      payload.activityTime = form.activityTime;
    }

    const description = form.description.trim();
    if (description) payload.description = description;

    if (form.estimatedCost !== '') {
      payload.estimatedCost = Number(form.estimatedCost);
    }

    if (form.destinationId) {
      payload.destinationId = Number(form.destinationId);
    }

    await onSubmit(payload);

    if (!isEditing) {
      setForm(buildEmptyForm(fixedDate, defaultDestinationId));
    }
  }

  const showFixedDate = fixedDate && !isEditing;
  const resolvedSubmitLabel = submitLabel ?? (isEditing ? 'Sačuvaj izmene' : 'Dodaj aktivnost');
  const autoSelectedDestination = !isEditing && defaultDestinationId
    ? destinations.find((destination) => destination.id === defaultDestinationId)
    : null;

  return (
    <form className="card form-card nested-form" onSubmit={handleSubmit}>
      <h3>{isEditing ? 'Izmeni aktivnost' : 'Dodaj aktivnost'}</h3>

      {showFixedDate && (
        <>
          <p className="fixed-date-label">
            <strong>Datum:</strong> {fixedDate}
          </p>
          <input type="hidden" name="activityDate" value={form.activityDate} readOnly />
        </>
      )}

      <label>
        Naziv
        <input name="name" value={form.name} onChange={handleChange} placeholder="npr. Obilazak muzeja" />
        {errors.name && <span className="field-error">{errors.name}</span>}
      </label>

      <div className="form-row">
        {!showFixedDate && (
          <label>
            Datum
            <input
              type="date"
              name="activityDate"
              value={form.activityDate}
              onChange={handleChange}
              min={planStartDate ?? undefined}
              max={planEndDate ?? undefined}
            />
            {errors.activityDate && <span className="field-error">{errors.activityDate}</span>}
          </label>
        )}
        <label>
          Vreme
          <input type="time" name="activityTime" value={form.activityTime} onChange={handleChange} />
        </label>
      </div>

      <label>
        Procijenjeni trošak (€)
        <input type="number" min="0" step="0.01" name="estimatedCost" value={form.estimatedCost} onChange={handleChange} />
        {errors.estimatedCost && <span className="field-error">{errors.estimatedCost}</span>}
        <span className="field-hint muted">Pojavljuje se u Troškovima kao procijenjeni trošak.</span>
      </label>

      {destinations.length > 0 && (
        <label>
          Destinacija (opciono)
          <select name="destinationId" value={form.destinationId} onChange={handleChange}>
            <option value="">— Bez destinacije —</option>
            {destinations.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
          {autoSelectedDestination && (
            <span className="field-hint muted">
              Automatski izabrano: {autoSelectedDestination.name} (već si na ovoj destinaciji taj dan).
            </span>
          )}
        </label>
      )}

      <label>
        Opis
        <textarea name="description" value={form.description} onChange={handleChange} rows={2} />
      </label>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary">{resolvedSubmitLabel}</button>
        {onCancel && (
          <button type="button" className="btn btn-secondary" onClick={onCancel}>
            Otkaži
          </button>
        )}
      </div>
    </form>
  );
}
