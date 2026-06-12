import { useState } from 'react';

const emptyForm = {
  name: '',
  lastName: '',
  email: '',
  password: '',
  role: 'User',
};

export default function AdminUserForm({ onSubmit }) {
  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function validate() {
    const nextErrors = {};
    const trimmedName = form.name.trim();
    const trimmedLastName = form.lastName.trim();

    if (!trimmedName) nextErrors.name = 'Ime je obavezno.';
    else if (trimmedName.length < 2) nextErrors.name = 'Ime mora imati bar 2 karaktera.';
    if (!trimmedLastName) nextErrors.lastName = 'Prezime je obavezno.';
    else if (trimmedLastName.length < 2) nextErrors.lastName = 'Prezime mora imati bar 2 karaktera.';
    if (!form.email.trim()) nextErrors.email = 'Email je obavezan.';
    if (!form.password) nextErrors.password = 'Lozinka je obavezna.';
    else if (form.password.length < 6) nextErrors.password = 'Lozinka mora imati bar 6 karaktera.';

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate()) return;

    await onSubmit({
      name: form.name.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      password: form.password,
      role: form.role,
    });

    setForm(emptyForm);
    setErrors({});
  }

  return (
    <form className="card form-card admin-user-form" onSubmit={handleSubmit}>
      <h2>Dodaj korisnika</h2>

      <div className="form-row">
        <label>
          Ime
          <input name="name" value={form.name} onChange={handleChange} />
          {errors.name && <span className="field-error">{errors.name}</span>}
        </label>
        <label>
          Prezime
          <input name="lastName" value={form.lastName} onChange={handleChange} />
          {errors.lastName && <span className="field-error">{errors.lastName}</span>}
        </label>
      </div>

      <div className="form-row">
        <label>
          Email
          <input type="email" name="email" value={form.email} onChange={handleChange} />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </label>
        <label>
          Lozinka
          <input type="password" name="password" value={form.password} onChange={handleChange} />
          {errors.password && <span className="field-error">{errors.password}</span>}
        </label>
      </div>

      <div className="form-row">
        <label>
          Uloga
          <select name="role" value={form.role} onChange={handleChange}>
            <option value="User">Korisnik</option>
            <option value="Admin">Administrator</option>
          </select>
        </label>
      </div>

      <div className="form-actions">
        <button type="submit" className="btn btn-primary">Dodaj korisnika</button>
      </div>
    </form>
  );
}
