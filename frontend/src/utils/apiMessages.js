const API_MESSAGE_TRANSLATIONS = {
  'Request failed.': 'Zahtev nije uspeo.',
  'Travel plan not found.': 'Plan putovanja nije pronađen.',
  'User not found.': 'Korisnik nije pronađen.',
  'Invalid token.': 'Neispravan token.',
  'Invalid email or password.': 'Pogrešan email ili lozinka.',
  'A user with this email already exists.': 'Korisnik sa ovim emailom već postoji.',
  'You cannot change your own role.': 'Ne možeš menjati sopstvenu ulogu.',
  'You cannot delete your own account.': 'Ne možeš obrisati sopstveni nalog.',
  'Invalid role. Use User or Admin.': 'Neispravna uloga.',
  'Invalid expense category.': 'Neispravna kategorija troška.',
  'Invalid access type. Use View or Edit.': 'Neispravan tip pristupa.',
  'Invalid activity status.': 'Neispravan status aktivnosti.',
  'Estimated cost cannot be negative.': 'Procenjeni trošak ne može biti negativan.',
  'Expiry date must be in the future.': 'Datum isteka mora biti u budućnosti.',
  'Destination does not belong to this travel plan.': 'Destinacija ne pripada ovom planu.',
  'End date cannot be before start date.': 'Krajnji datum ne može biti pre početnog.',
  'Planned budget cannot be negative.': 'Planirani budžet ne može biti negativan.',
  'Departure date cannot be before arrival date.': 'Datum odlaska ne može biti pre dolaska.',
  'Share link not found or expired.': 'Link za deljenje nije pronađen ili je istekao.',
  'Share link not found, expired, or read-only.': 'Link nije pronađen, istekao je ili je samo za pregled.',
  'Checklist item not found.': 'Stavka liste za pakovanje nije pronađena.',
  'Destination not found.': 'Destinacija nije pronađena.',
  'Activity not found.': 'Aktivnost nije pronađena.',
  'Expense not found.': 'Trošak nije pronađen.',
  'Share link not found.': 'Link za deljenje nije pronađen.',
  'Morate biti prijavljeni da biste koristili link za izmenu.': 'Morate biti prijavljeni da biste koristili link za izmenu.',
  'Unauthorized. Valid JWT token required.': 'Morate biti prijavljeni za ovu akciju.',
  'The Title field is required.': 'Naslov je obavezan.',
  'The Name field is required.': 'Naziv je obavezan.',
  'The Email field is required.': 'Email je obavezan.',
  'The Password field is required.': 'Lozinka je obavezna.',
};

export function translateApiMessage(message) {
  if (!message || typeof message !== 'string') {
    return 'Zahtev nije uspeo.';
  }

  const trimmed = message.trim();
  if (API_MESSAGE_TRANSLATIONS[trimmed]) {
    return API_MESSAGE_TRANSLATIONS[trimmed];
  }

  return trimmed
    .replace(/\bEstimated cost cannot be negative\./gi, 'Procenjeni trošak ne može biti negativan.')
    .replace(/\bTravel plan not found\./gi, 'Plan putovanja nije pronađen.')
    .replace(/\bInvalid expense category\./gi, 'Neispravna kategorija troška.');
}
