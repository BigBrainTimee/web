import { useState } from 'react';
import { CUSTOM_PACKING_SORT_ORDER } from '../constants/checklistDefaults';

export default function ChecklistForm({ onSubmit }) {
  const [title, setTitle] = useState('');
  const [error, setError] = useState('');

  async function handleSubmit(event) {
    event.preventDefault();
    if (!title.trim()) {
      setError('Unesi naziv stavke.');
      return;
    }

    await onSubmit({ title: title.trim(), sortOrder: CUSTOM_PACKING_SORT_ORDER });
    setTitle('');
    setError('');
  }

  return (
    <form className="checklist-form" onSubmit={handleSubmit}>
      <input
        placeholder="npr. Lekovi, Kišobran, Adapter..."
        value={title}
        onChange={(e) => { setTitle(e.target.value); setError(''); }}
        aria-label="Dodatna stavka za pakovanje"
      />
      <button type="submit" className="btn btn-primary">Dodaj u Ostalo</button>
      {error && <span className="field-error">{error}</span>}
    </form>
  );
}
