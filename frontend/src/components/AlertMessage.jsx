export default function AlertMessage({ type = 'error', message, onClose }) {
  if (!message) {
    return null;
  }

  return (
    <div className={`alert alert-${type}`}>
      <span>{message}</span>
      {onClose && (
        <button type="button" className="alert-close" onClick={onClose}>
          ×
        </button>
      )}
    </div>
  );
}
