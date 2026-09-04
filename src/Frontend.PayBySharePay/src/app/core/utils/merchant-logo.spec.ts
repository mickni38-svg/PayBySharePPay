import {
  getStaticMerchantLogoUrl,
  getStaticMerchantLogoUrlByDisplayName
} from './merchant-logo';

describe('getStaticMerchantLogoUrl', () => {
  it('returns a static logo when merchant Name matches a logo filename', () => {
    expect(getStaticMerchantLogoUrl({ type: 'Merchant', handle: 'bella-napoli' }))
      .toBe('/images/bella-napoli.png');
  });

  it('does not return a merchant logo for a person', () => {
    expect(getStaticMerchantLogoUrl({ type: 'Person', handle: 'bella-napoli' })).toBeNull();
  });

  it('does not return a logo when merchant Name has no matching file', () => {
    expect(getStaticMerchantLogoUrl({ type: 'Merchant', handle: 'unknown-merchant' })).toBeNull();
  });

  it('requires an exact filename match', () => {
    expect(getStaticMerchantLogoUrl({ type: 'Merchant', handle: 'Bella-Napoli' })).toBeNull();
  });

  it('resolves display names with Danish characters and ampersands', () => {
    expect(getStaticMerchantLogoUrlByDisplayName('Café Havblik'))
      .toBe('/images/cafe-havblik.png');
    expect(getStaticMerchantLogoUrlByDisplayName('Møllebageren'))
      .toBe('/images/moellebageren.png');
    expect(getStaticMerchantLogoUrlByDisplayName('Sticks & Rice'))
      .toBe('/images/sticks-and-rice.png');
  });

  it('does not guess a static logo for an unknown display name', () => {
    expect(getStaticMerchantLogoUrlByDisplayName('Pizza Roma ApS')).toBeNull();
  });
});
