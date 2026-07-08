/**
 * Reference list of cities served by the platform. Kept as the single source of truth so a trip
 * created by a company (departureCity/arrivalCity) always matches the exact spelling used by the
 * public search dropdowns — trip search is an exact, accent-sensitive match on these strings.
 */
export const MAURITANIA_CITIES = [
  'Nouakchott',
  'Nouadhibou',
  'Rosso',
  'Kiffa',
  'Ayoun',
  'Néma',
  'Kaédi',
  'Sélibaby',
  'Atar',
  'Tagant',
  'Zouérat',
  'Akjoujt',
] as const;
