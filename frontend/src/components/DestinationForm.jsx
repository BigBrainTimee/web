import { useEffect, useState } from 'react';

const emptyForm = {
  name: '',
  location: '',
  arrivalDate: '',
  departureDate: '',
  description: '',
};

export function destinationToFormValues(destination) {
  return {
    name: destination.name,
    location: destination.location,
    arrivalDate: destination.arrivalDate,
    departureDate: destination.departureDate,
    description: destination.description ?? '',
  };
}

export default function DestinationForm({
  onSubmit,
  onCancel,
  initialValues = null,
  fixedArrivalDate = null,
  planStartDate = null,
  planEndDate = null,
  submitLabel,
}) {
  const isEditing = Boolean(initialValues);
  const [form, setForm] = useState(() => {
    if (initialValues) return initialValues;
    return fixedArrivalDate ? { ...emptyForm, arrivalDate: fixedArrivalDate } : emptyForm;
  });
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (initialValues) {
      setForm(initialValues);
      setErrors({});
      return;
    }

    setForm(fixedArrivalDate ? { ...emptyForm, arrivalDate: fixedArrivalDate } : emptyForm);
    setErrors({});
  }, [initialValues, fixedArrivalDate]);

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const nextErrors = {};

    if (!form.name.trim()) nextErrors.name = 'Naziv je obavezan.';
    if (!form.location.trim()) nextErrors.location = 'Lokacija je obavezna.';
    if (!form.arrivalDate) nextErrors.arrivalDate = 'Datum dolaska je obavezan.';
    if (!form.departureDate) nextErrors.departureDate = 'Datum odlaska je obavezan.';
    if (form.arrivalDate && form.departureDate && form.departureDate < form.arrivalDate) {
      nextErrors.departureDate = 'Datum odlaska ne može biti pre dolaska.';
    }
    if (planStartDate && form.arrivalDate && form.arrivalDate < planStartDate) {
      nextErrors.arrivalDate = `Dolazak mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }
    if (planEndDate && form.arrivalDate && form.arrivalDate > planEndDate) {
      nextErrors.arrivalDate = `Dolazak mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }
    if (planStartDate && form.departureDate && form.departureDate < planStartDate) {
      nextErrors.departureDate = `Odlazak mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }
    if (planEndDate && form.departureDate && form.departureDate > planEndDate) {
      nextErrors.departureDate = `Odlazak mora biti u periodu putovanja (${planStartDate} – ${planEndDate}).`;
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    await onSubmit({
      name: form.name.trim(),
      location: form.location.trim(),
      arrivalDate: form.arrivalDate,
      departureDate: form.departureDate,
      description: form.description.trim() || null,
    });

    if (!isEditing) {
      setForm(emptyForm);
    }
  }

  const resolvedSubmitLabel = submitLabel ?? (isEditing ? 'Sačuvaj izmene' : 'Dodaj');

  return (
    <form className="card form-card nested-form" onSubmit={handleSubmit}>
      <h3>{isEditing ? 'Izmeni destinaciju' : 'Dodaj destinaciju'}</h3>

      <label>
        Naziv
        <input name="name" value={form.name} onChange={handleChange} />
        {errors.name && <span className="field-error">{errors.name}</span>}
      </label>

      <label>
        Lokacija
        <input name="location" value={form.location} onChange={handleChange} />
        {errors.location && <span className="field-error">{errors.location}</span>}
      </label>

      <div className="form-row">
        <label>
          Dolazak
          <input
            type="date"
            name="arrivalDate"
            value={form.arrivalDate}
            onChange={handleChange}
            min={planStartDate ?? undefined}
            max={planEndDate ?? undefined}
            readOnly={Boolean(fixedArrivalDate && !isEditing)}
          />
          {errors.arrivalDate && <span className="field-error">{errors.arrivalDate}</span>}
        </label>
        <label>
          Odlazak
          <input
            type="date"
            name="departureDate"
            value={form.departureDate}
            onChange={handleChange}
            min={form.arrivalDate || planStartDate || undefined}
            max={planEndDate ?? undefined}
          />
          {errors.departureDate && <span className="field-error">{errors.departureDate}</span>}
        </label>
      </div>

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
