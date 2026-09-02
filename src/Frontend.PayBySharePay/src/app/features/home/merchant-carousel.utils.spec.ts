import {
  filterAndLimitMerchants,
  getCarouselScrollTarget,
  markMerchantAsRecentlyUsed,
  parseRecentMerchantIds,
  sortMerchantsByRecentUse
} from './merchant-carousel.utils';

interface TestMerchant {
  id: number;
  displayName: string;
}

function merchants(count: number): TestMerchant[] {
  return Array.from({ length: count }, (_, index) => ({
    id: index + 1,
    displayName: `Merchant ${String(index + 1).padStart(2, '0')}`
  }));
}

describe('merchant carousel utilities', () => {
  describe('filterAndLimitMerchants', () => {
    it('supports empty, single and eight-merchant lists', () => {
      expect(filterAndLimitMerchants([], '')).toEqual([]);
      expect(filterAndLimitMerchants(merchants(1), '')).toHaveSize(1);
      expect(filterAndLimitMerchants(merchants(8), '')).toHaveSize(8);
    });

    it('limits default and search results to eight merchants', () => {
      expect(filterAndLimitMerchants(merchants(10), '')).toHaveSize(8);
      expect(filterAndLimitMerchants(merchants(10), 'merchant')).toHaveSize(8);
    });

    it('matches full and partial names without case sensitivity', () => {
      const values = [
        { id: 1, displayName: 'Bella Napoli' },
        { id: 2, displayName: 'Café Havblik' }
      ];

      expect(filterAndLimitMerchants(values, 'BELLA')).toEqual([values[0]]);
      expect(filterAndLimitMerchants(values, 'havblik')).toEqual([values[1]]);
      expect(filterAndLimitMerchants(values, 'findes ikke')).toEqual([]);
    });
  });

  describe('recent merchant ordering', () => {
    const values = [
      { id: 1, displayName: 'Café Havblik' },
      { id: 2, displayName: 'Bella Napoli' },
      { id: 3, displayName: 'Den Gyldne Wok' }
    ];

    it('places recent merchants first and sorts the rest alphabetically', () => {
      expect(sortMerchantsByRecentUse(values, [3])).toEqual([
        values[2],
        values[1],
        values[0]
      ]);
    });

    it('moves a selected merchant to the front without duplicates', () => {
      expect(markMerchantAsRecentlyUsed([3, 2, 1], 2)).toEqual([2, 3, 1]);
    });

    it('parses valid unique IDs and ignores invalid stored history', () => {
      expect(parseRecentMerchantIds('[3,2,3,-1,"1"]')).toEqual([3, 2]);
      expect(parseRecentMerchantIds('not json')).toEqual([]);
      expect(parseRecentMerchantIds('{"id": 1}')).toEqual([]);
    });
  });

  describe('keyboard navigation', () => {
    it('calculates bounded arrow, home and end targets', () => {
      expect(getCarouselScrollTarget('ArrowLeft', 100, 400, 102)).toBe(0);
      expect(getCarouselScrollTarget('ArrowRight', 350, 400, 102)).toBe(400);
      expect(getCarouselScrollTarget('Home', 250, 400, 102)).toBe(0);
      expect(getCarouselScrollTarget('End', 0, 400, 102)).toBe(400);
    });

    it('ignores unsupported keys', () => {
      expect(getCarouselScrollTarget('Enter', 0, 400, 102)).toBeNull();
    });
  });
});
