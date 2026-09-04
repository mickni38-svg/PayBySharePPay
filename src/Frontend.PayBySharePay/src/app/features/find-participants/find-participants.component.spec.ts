import { of } from 'rxjs';
import { FindParticipantsComponent } from './find-participants.component';

describe('FindParticipantsComponent merchant logos', () => {
  it('shows the static merchant logo and keeps the API logo as fallback', () => {
    const merchant = {
      id: 42,
      type: 'Merchant',
      displayName: 'Bella Napoli',
      handle: 'bella-napoli',
      logoUrl: '/api/participants/42/logo'
    };
    const component = new FindParticipantsComponent(
      { search: () => of([]) } as any,
      { getFriends: () => of([merchant]) } as any,
      { currentUserId: () => 7 } as any
    );

    component.ngOnInit();

    expect(component.friendEntries()[0].logoUrl).toBe('/images/bella-napoli.png');
    expect(component.friendEntries()[0].fallbackLogoUrl)
      .toMatch(/\/api\/participants\/42\/logo$/);

    component.onMerchantLogoError(42);

    expect(component.friendEntries()[0].logoUrl)
      .toMatch(/\/api\/participants\/42\/logo$/);
  });
});
