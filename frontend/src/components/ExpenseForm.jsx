import { useState } from 'react';
import { EXPENSE_CATEGORIES, getExpenseCategoryLabel } from '../models/Expense';

const emptyForm = {
  name: '',
  category: 'Food',
  amount: '',
  expenseDate: '',
  description: '',
};

export default function ExpenseForm({ onSubmit }) {
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
    if (!form.expenseDate) nextErrors.expenseDate = 'Datum je obavezan.';
    if (form.amount === '' || Number(form.amount) < 0) {
      nextErrors.amount = 'Iznos mora biti 0 ili veći.';
    }
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    await onSubmit({
      name: form.name.trim(),
      category: form.category,
      amount: Number(form.amount),
      expenseDate: form.expenseDate,
      description: form.description.trim() || null,
    });

    setForm(emptyForm);
  }

  return (
    <form className="card form-card nested-form" onSubmit={handleSubmit}>
      <h3>Dodaj trošak</h3>

      <label>
        Naziv
        <input name="name" value={form.name} onChange={handleChange} />
        {errors.name && <span className="field-error">{errors.name}</span>}
      </label>

      <div className="form-row">
        <label>
          Kategorija
          <select name="category" value={form.category} onChange={handleChange}>
            {EXPENSE_CATEGORIES.map((category) => (
              <option key={category} value={category}>{getExpenseCategoryLabel(category)}</option>
            ))}
          </select>
        </label>
        <label>
          Iznos (€)
          <input type="number" min="0" step="0.01" name="amount" value={form.amount} onChange={handleChange} />
          {errors.amount && <span className="field-error">{errors.amount}</span>}
        </label>
      </div>

      <label>
        Datum
        <input type="date" name="expenseDate" value={form.expenseDate} onChange={handleChange} />
        {errors.expenseDate && <span className="field-error">{errors.expenseDate}</span>}
      </label>

      <label>
        Opis
        <textarea name="description" value={form.description} onChange={handleChange} rows={2} />
      </label>

      <button type="submit" className="btn btn-primary">Dodaj trošak</button>
    </form>
  );
}
