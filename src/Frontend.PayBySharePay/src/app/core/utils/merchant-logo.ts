import { DirectoryEntry } from '../models/directory.model';

const STATIC_MERCHANT_LOGO_NAMES = new Set([
  'bella-napoli',
  'burgerhuset',
  'cafe-havblik',
  'den-gyldne-wok',
  'green-bowl',
  'havfruen-fisk',
  'la-piazza',
  'moellebageren',
  'sakura-sushi',
  'sticks-and-rice'
]);

/**
 * DirectoryEntry.handle contains Participant.Name for merchants.
 * A static logo is used only when the participant is a merchant and its
 * database name exactly matches an available logo filename.
 */
export function getStaticMerchantLogoUrl(
  participant: Pick<DirectoryEntry, 'type' | 'handle'>
): string | null {
  if (participant.type !== 'Merchant') return null;

  const merchantName = participant.handle?.trim();
  if (!merchantName || !STATIC_MERCHANT_LOGO_NAMES.has(merchantName)) return null;

  return `/images/${merchantName}.png`;
}
