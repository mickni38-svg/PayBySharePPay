import { CreateOrderComponent } from './create-order.component';

describe('CreateOrderComponent merchant state', () => {
  it('redirects home when opened without a valid merchant', () => {
    history.replaceState({}, '', window.location.href);
    const router = {
      navigate: jasmine.createSpy('navigate')
    };
    const component = new CreateOrderComponent(
      {} as any,
      {} as any,
      router as any,
      { currentUserId: () => 7 } as any
    );

    component.ngOnInit();

    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('locks a valid preselected merchant into the wizard state', () => {
    history.replaceState({
      merchant: {
        id: 42,
        displayName: 'Bella Napoli',
        handle: 'bella-napoli',
        logoUrl: '/images/bella-napoli.png',
        fallbackLogoUrl: null
      }
    }, '', window.location.href);

    const directoryService = {
      getFriends: () => ({
        subscribe: () => undefined
      })
    };
    const router = {
      navigate: jasmine.createSpy('navigate')
    };
    const component = new CreateOrderComponent(
      {} as any,
      directoryService as any,
      router as any,
      { currentUserId: () => 7 } as any
    );

    component.ngOnInit();

    expect(component.selectedMerchant()?.id).toBe(42);
    expect(component.merchantPreselected()).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
