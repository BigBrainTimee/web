import { useEffect, useState } from 'react';

const STATUS_OPTIONS = ['Planned', 'Reserved', 'Completed', 'Cancelled'];

const emptyForm = {
  name: '',
  activityDate: '',
  activityTime: '',
  location: '',
  description: '',
  estimatedCost: '',
  status: 'Planned',
  destinationId: '',
};

export default function ActivityForm({ destinations, onSubmit, fixedDate = null }) {
  const [form, setForm] = useState(() => (
    fixedDate ? { ...emptyForm, activityDate: fixedDate } : emptyForm
  ));
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (fixedDate) {
      setForm((prev) => ({ ...prev, activityDate: fixedDate }));
    }
  }, [fixedDate]);

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const nextErrors = {};
    if (!form.name.trim()) nextErrors.name = 'Naziv je obavezan.';
    if (!form.activityDate) nextErrors.activityDate = 'Datum je obavezan.';
    if (form.estimatedCost !== '' && Number(form.estimatedCost) < 0) {
      nextErrors.estimatedCost = 'Trošak ne može biti negativan.';
    }
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    await onSubmit({
      name: form.name.trim(),
      activityDate: form.activityDate,
      activityTime: form.activityTime || null,
      location: form.location.trim() || null,
      description: form.description.trim() || null,
      estimatedCost: form.estimatedCost === '' ? null : Number(form.estimatedCost),
      status: form.status,
      destinationId: form.destinationId ? Number(form.destinationId) : null,
    });

    setForm(fixedDate ? { ...emptyForm, activityDate: fixedDate } : emptyForm);
  }

  return (
    <form className="card form-card nested-form" onSubmit={handleSubmit}>
      <h3>Dodaj aktivnost</h3>

      {fixedDate && (
        <p className="fixed-date-label">
          <strong>Datum:</strong> {fixedDate}
        </p>
      )}

      <label>
        Naziv
        <input name="name" value={form.name} onChange={handleChange} />
        {errors.name && <span className="field-error">{errors.name}</span>}
      </label>

      <div className="form-row">
        {!fixedDate && (
          <label>
            Datum
            <input type="date" name="activityDate" value={form.activityDate} onChange={handleChange} />
            {errors.activityDate && <span className="field-error">{errors.activityDate}</span>}
          </label>
        )}
        <label>
          Vreme
          <input type="time" name="activityTime" value={form.activityTime} onChange={handleChange} />
        </label>
      </div>

      <label>
        Lokacija
        <input name="location" value={form.location} onChange={handleChange} />
      </label>

      <div className="form-row">
        <label>
          Status
          <select name="status" value={form.status} onChange={handleChange}>
            {STATUS_OPTIONS.map((status) => (
              <option key={status} value={status}>{status}</option>
            ))}
          </select>
        </label>
        <label>
          Proc. trošak (€)
          <input type="number" min="0" step="0.01" name="estimatedCost" value={form.estimatedCost} onChange={handleChange} />
          {errors.estimatedCost && <span className="field-error">{errors.estimatedCost}</span>}
        </label>
      </div>

      {destinations.length > 0 && (
        <label>
          Destinacija (opciono)
          <select name="destinationId" value={form.destinationId} onChange={handleChange}>
            <option value="">— Bez destinacije —</option>
            {destinations.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
        </label>
      )}

      <label>
        Opis
        <textarea name="description" value={form.description} onChange={handleChange} rows={2} />
      </label>

      <button type="submit" className="btn btn-primary">Dodaj aktivnost</button>
    </form>
  );
}
