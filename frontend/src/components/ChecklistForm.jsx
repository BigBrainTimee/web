import { useState } from 'react';

export default function ChecklistForm({ onSubmit }) {
  const [title, setTitle] = useState('');
  const [error, setError] = useState('');

  async function handleSubmit(event) {
    event.preventDefault();
    if (!title.trim()) {
      setError('Naslov je obavezan.');
      return;
    }

    await onSubmit({ title: title.trim(), sortOrder: 0 });
    setTitle('');
    setError('');
  }

  return (
    <form className="checklist-form" onSubmit={handleSubmit}>
      <input
        placeholder="npr. Pasoš, Karta, Punjač..."
        value={title}
        onChange={(e) => { setTitle(e.target.value); setError(''); }}
      />
      <button type="submit" className="btn btn-primary">Dodaj</button>
      {error && <span className="field-error">{error}</span>}
    </form>
  );
}
