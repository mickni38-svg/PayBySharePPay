export interface MerchantListItem {
  id: number;
  displayName: string;
}

export const MAX_MERCHANTS = 8;

export function filterAndLimitMerchants<T extends MerchantListItem>(
  merchants: readonly T[],
  searchTerm: string,
  maximum = MAX_MERCHANTS
): T[] {
  const term = searchTerm.trim().toLocaleLowerCase('da');
  const filtered = term
    ? merchants.filter(merchant =>
        merchant.displayName.toLocaleLowerCase('da').includes(term))
    : merchants;

  return filtered.slice(0, maximum);
}

export function sortMerchantsByRecentUse<T extends MerchantListItem>(
  merchants: readonly T[],
  recentMerchantIds: readonly number[]
): T[] {
  const recentIndex = new Map(
    recentMerchantIds.map((merchantId, index) => [merchantId, index])
  );

  return [...merchants].sort((left, right) => {
    const leftIndex = recentIndex.get(left.id) ?? Number.MAX_SAFE_INTEGER;
    const rightIndex = recentIndex.get(right.id) ?? Number.MAX_SAFE_INTEGER;

    if (leftIndex !== rightIndex) {
      return leftIndex - rightIndex;
    }

    return left.displayName.localeCompare(right.displayName, 'da', {
      sensitivity: 'base'
    });
  });
}

export function parseRecentMerchantIds(value: string | null): number[] {
  if (!value) return [];

  try {
    const parsed: unknown = JSON.parse(value);
    if (!Array.isArray(parsed)) return [];

    return parsed.filter(
      (id, index): id is number =>
        Number.isInteger(id) && id > 0 && parsed.indexOf(id) === index
    );
  } catch {
    return [];
  }
}

export function markMerchantAsRecentlyUsed(
  recentMerchantIds: readonly number[],
  merchantId: number,
  maximum = 50
): number[] {
  return [
    merchantId,
    ...recentMerchantIds.filter(id => id !== merchantId)
  ].slice(0, maximum);
}

export type CarouselNavigationKey =
  | 'ArrowLeft'
  | 'ArrowRight'
  | 'Home'
  | 'End';

export function getCarouselScrollTarget(
  key: string,
  currentLeft: number,
  maximumLeft: number,
  cardStep: number
): number | null {
  switch (key as CarouselNavigationKey) {
    case 'ArrowLeft':
      return Math.max(0, currentLeft - cardStep);
    case 'ArrowRight':
      return Math.min(maximumLeft, currentLeft + cardStep);
    case 'Home':
      return 0;
    case 'End':
      return maximumLeft;
    default:
      return null;
  }
}
