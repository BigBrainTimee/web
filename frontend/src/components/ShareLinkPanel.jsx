import { useEffect, useState } from 'react';
import { QRCode } from 'react-qr-code';
import { buildShareUrl, formatApiDate, isShareLinkExpired } from '../models/ShareLink';
import { ApiError } from '../services/apiClient';
import { getAccessTypeLabel } from '../utils/accessTypes';
import * as shareService from '../services/shareService';

export default function ShareLinkPanel({ authToken, planId, shareApi }) {
  const api = shareApi ?? shareService;
  const [links, setLinks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [accessType, setAccessType] = useState('View');
  const [expiresAt, setExpiresAt] = useState('');
  const [copiedToken, setCopiedToken] = useState('');

  async function loadLinks() {
    setLoading(true);
    setError('');
    try {
      const data = await api.getShareLinks(authToken, planId);
      setLinks(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Učitavanje linkova nije uspelo.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadLinks();
  }, [authToken, planId]);

  async function handleCreate(event) {
    event.preventDefault();
    setError('');

    try {
      await api.createShareLink(authToken, planId, {
        accessType,
        expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      });
      setExpiresAt('');
      await loadLinks();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kreiranje linka nije uspelo.');
    }
  }

  async function handleDelete(linkId) {
    if (!window.confirm('Obrisati link za deljenje?')) return;

    try {
      await api.deleteShareLink(authToken, planId, linkId);
      await loadLinks();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Brisanje linka nije uspelo.');
    }
  }

  async function handleCopy(url, token) {
    try {
      await navigator.clipboard.writeText(url);
      setCopiedToken(token);
      setTimeout(() => setCopiedToken(''), 2000);
    } catch {
      setError('Kopiranje u clipboard nije uspelo.');
    }
  }

  if (loading) {
    return <p className="muted">Učitavanje linkova za deljenje...</p>;
  }

  return (
    <div className="share-panel">
      {error && <p className="field-error">{error}</p>}

      <form className="share-form card nested-form" onSubmit={handleCreate}>
        <h3>Kreiraj novi link</h3>
        <div className="form-row">
          <label>
            Tip pristupa
            <select value={accessType} onChange={(e) => setAccessType(e.target.value)}>
              <option value="View">Pregled (samo čitanje)</option>
              <option value="Edit">Izmena (destinacije, aktivnosti, troškovi, lista za pakovanje)</option>
            </select>
          </label>
          <label>
            Ističe (opciono)
            <input
              type="datetime-local"
              value={expiresAt}
              onChange={(e) => setExpiresAt(e.target.value)}
            />
          </label>
        </div>
        <button type="submit" className="btn btn-primary">Generiši link + QR</button>
      </form>

      {links.length === 0 ? (
        <p className="muted">Još nema linkova za deljenje ovog plana.</p>
      ) : (
        <ul className="share-link-list">
          {links.map((link) => {
            const url = buildShareUrl(link.token);
            const isExpired = isShareLinkExpired(link.expiresAt);
            const hasExpiry = Boolean(link.expiresAt);

            return (
              <li key={link.id} className="share-link-card card">
                <div className="share-link-header">
                  <span className={`badge access-${link.accessType.toLowerCase()}`}>
                    {getAccessTypeLabel(link.accessType)}
                  </span>
                  {hasExpiry && (
                    isExpired
                      ? <span className="badge expired">Istekao</span>
                      : <span className="badge active">Aktivan</span>
                  )}
                </div>

                <div className="share-link-body">
                  <div className="qr-wrap">
                    <QRCode value={url} size={128} />
                  </div>
                  <div className="share-link-details">
                    <p className="share-url">{url}</p>
                    <p className="muted">
                      Kreiran: {formatApiDate(link.createdAt)}
                      {link.expiresAt && (
                        <> · Ističe: {formatApiDate(link.expiresAt)}</>
                      )}
                    </p>
                    <div className="share-link-actions">
                      <button
                        type="button"
                        className="btn btn-secondary btn-sm"
                        onClick={() => handleCopy(url, link.token)}
                      >
                        {copiedToken === link.token ? 'Kopirano!' : 'Kopiraj link'}
                      </button>
                      <button
                        type="button"
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDelete(link.id)}
                      >
                        Obriši
                      </button>
                    </div>
                  </div>
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
