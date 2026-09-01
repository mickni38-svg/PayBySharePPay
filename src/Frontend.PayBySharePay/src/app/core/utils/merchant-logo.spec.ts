import { getStaticMerchantLogoUrl } from './merchant-logo';

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
});
