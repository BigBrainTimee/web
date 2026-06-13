import { useState } from 'react';
import DatePickerField from './DatePickerField';

const emptyForm = {
  name: '',
  description: '',
  startDate: '',
  endDate: '',
  plannedBudget: '',
  notes: '',
};

export default function TravelPlanForm({ initialValues, onSubmit, submitLabel }) {
  const [form, setForm] = useState({ ...emptyForm, ...initialValues });
  const [errors, setErrors] = useState({});

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => {
      const next = { ...prev, [name]: value };
      if (name === 'startDate' && next.endDate && value && next.endDate < value) {
        next.endDate = '';
      }
      return next;
    });
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function handleDateChange(name, value) {
    setForm((prev) => {
      const next = { ...prev, [name]: value };
      if (name === 'startDate' && next.endDate && value && next.endDate < value) {
        next.endDate = '';
      }
      return next;
    });
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const nextErrors = {};

    if (!form.name.trim()) nextErrors.name = 'Naziv je obavezan.';
    if (!form.startDate) nextErrors.startDate = 'Početni datum je obavezan.';
    if (!form.endDate) nextErrors.endDate = 'Krajnji datum je obavezan.';
    if (form.startDate && form.endDate && form.endDate < form.startDate) {
      nextErrors.endDate = 'Krajnji datum ne može biti pre početnog.';
    }
    if (form.plannedBudget === '' || Number(form.plannedBudget) < 0) {
      nextErrors.plannedBudget = 'Budžet mora biti 0 ili više.';
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    await onSubmit({
      name: form.name.trim(),
      description: form.description.trim() || null,
      startDate: form.startDate,
      endDate: form.endDate,
      plannedBudget: Number(form.plannedBudget),
      notes: form.notes.trim() || null,
    });
  }

  return (
    <form className="card form-card" onSubmit={handleSubmit}>
      <label>
        Naziv putovanja
        <input name="name" value={form.name} onChange={handleChange} />
        {errors.name && <span className="field-error">{errors.name}</span>}
      </label>

      <label>
        Opis
        <textarea name="description" value={form.description} onChange={handleChange} rows={3} />
      </label>

      <div className="form-row">
        <DatePickerField
          name="startDate"
          label="Početak"
          value={form.startDate}
          onChange={(value) => handleDateChange('startDate', value)}
          error={errors.startDate}
          maxDate={form.endDate || undefined}
        />
        <DatePickerField
          name="endDate"
          label="Kraj"
          value={form.endDate}
          onChange={(value) => handleDateChange('endDate', value)}
          error={errors.endDate}
          minDate={form.startDate || undefined}
          openToDate={form.startDate || undefined}
        />
      </div>

      <label>
        Planirani budžet (€)
        <input
          type="number"
          min="0"
          step="0.01"
          name="plannedBudget"
          value={form.plannedBudget}
          onChange={handleChange}
        />
        {errors.plannedBudget && <span className="field-error">{errors.plannedBudget}</span>}
      </label>

      <label>
        Napomene
        <textarea name="notes" value={form.notes} onChange={handleChange} rows={3} />
      </label>

      <button type="submit" className="btn btn-primary">{submitLabel}</button>
    </form>
  );
}
