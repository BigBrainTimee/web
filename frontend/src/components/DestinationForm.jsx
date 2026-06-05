import { useState } from 'react';

const emptyForm = {
  name: '',
  location: '',
  arrivalDate: '',
  departureDate: '',
  description: '',
};

export default function DestinationForm({ onSubmit, onCancel }) {
  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});

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

    setForm(emptyForm);
  }

  return (
    <form className="card form-card nested-form" onSubmit={handleSubmit}>
      <h3>Dodaj destinaciju</h3>

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
          <input type="date" name="arrivalDate" value={form.arrivalDate} onChange={handleChange} />
          {errors.arrivalDate && <span className="field-error">{errors.arrivalDate}</span>}
        </label>
        <label>
          Odlazak
          <input type="date" name="departureDate" value={form.departureDate} onChange={handleChange} />
          {errors.departureDate && <span className="field-error">{errors.departureDate}</span>}
        </label>
      </div>

      <label>
        Opis
        <textarea name="description" value={form.description} onChange={handleChange} rows={2} />
      </label>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary">Dodaj</button>
        {onCancel && (
          <button type="button" className="btn btn-secondary" onClick={onCancel}>
            Otkaži
          </button>
        )}
      </div>
    </form>
  );
}
