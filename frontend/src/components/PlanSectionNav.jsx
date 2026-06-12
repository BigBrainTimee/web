export const PLAN_SECTIONS = {
  overview: { id: 'overview', label: 'Pregled' },
  expenses: { id: 'expenses', label: 'Troškovi' },
  destinations: { id: 'destinations', label: 'Destinacije' },
  activities: { id: 'activities', label: 'Aktivnosti' },
  sharing: { id: 'sharing', label: 'Deljenje plana' },
  checklist: { id: 'checklist', label: 'Lista za pakovanje' },
};

const SHARED_SECTION_IDS = ['overview', 'expenses', 'destinations', 'activities', 'checklist'];

export function getPlanSections({ includeSharing = true } = {}) {
  const ids = includeSharing
    ? Object.keys(PLAN_SECTIONS)
    : SHARED_SECTION_IDS;

  return ids.map((id) => PLAN_SECTIONS[id]);
}

export default function PlanSectionNav({ sections, activeSection, onSectionChange }) {
  return (
    <nav className="plan-detail-nav" aria-label="Sekcije plana">
      {sections.map((section) => (
        <button
          key={section.id}
          type="button"
          className={activeSection === section.id ? 'active' : ''}
          aria-current={activeSection === section.id ? 'page' : undefined}
          onClick={() => onSectionChange(section.id)}
        >
          {section.label}
        </button>
      ))}
    </nav>
  );
}
