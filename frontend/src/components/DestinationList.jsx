export default function DestinationList({ destinations, onDelete, readOnly = false }) {
  if (destinations.length === 0) {
    return <p className="muted">Još nema destinacija za ovaj plan.</p>;
  }

  return (
    <div className="destination-list">
      {destinations.map((destination) => (
        <article key={destination.id} className="card destination-card">
          <div>
            <h3>{destination.name}</h3>
            <p className="muted">{destination.location}</p>
            <p>
              {destination.arrivalDate} → {destination.departureDate}
            </p>
            {destination.description && <p>{destination.description}</p>}
          </div>
          {!readOnly && onDelete && (
          <button
            type="button"
            className="btn btn-danger"
            onClick={() => onDelete(destination.id)}
          >
            Obriši
          </button>
          )}
        </article>
      ))}
    </div>
  );
}
