import { HomeComponent } from './home.component';

describe('HomeComponent UC-02', () => {
  let router: { navigate: jasmine.Spy };
  let component: HomeComponent;

  beforeEach(() => {
    localStorage.clear();
    router = { navigate: jasmine.createSpy('navigate') };

    component = new HomeComponent(
      {
        currentUserId: () => 7,
        isLoggedIn: () => true
      } as any,
      {} as any,
      {} as any,
      {} as any,
      {} as any,
      router as any
    );
  });

  it('opens the wizard with the selected merchant real ID and display state', () => {
    const merchant = {
      id: 42,
      displayName: 'Bella Napoli',
      handle: 'bella-napoli',
      initials: 'BN',
      logoUrl: '/images/bella-napoli.png',
      fallbackLogoUrl: null
    };

    (component as any).activeUserId = 7;
    component.allMerchants.set([merchant]);

    component.selectMerchant(merchant);

    expect(router.navigate).toHaveBeenCalledWith(['/orders/create'], {
      state: {
        merchant: {
          id: 42,
          displayName: 'Bella Napoli',
          handle: 'bella-napoli',
          logoUrl: '/images/bella-napoli.png',
          fallbackLogoUrl: null
        }
      }
    });
    expect(localStorage.getItem('paynsync_recent_merchants:7')).toBe('[42]');
  });

  it('scrolls the carousel with the keyboard and prevents page scrolling', () => {
    const preventDefault = jasmine.createSpy('preventDefault');
    const scrollTo = jasmine.createSpy('scrollTo');
    const carousel = {
      scrollWidth: 500,
      clientWidth: 200,
      scrollLeft: 0,
      scrollTo
    } as unknown as HTMLElement;

    component.onCarouselKeydown(
      { key: 'ArrowRight', preventDefault } as unknown as KeyboardEvent,
      carousel
    );

    expect(preventDefault).toHaveBeenCalled();
    expect(scrollTo).toHaveBeenCalledWith({ left: 102, behavior: 'smooth' });
  });

  it('ignores keyboard keys that do not navigate the carousel', () => {
    const preventDefault = jasmine.createSpy('preventDefault');
    const scrollTo = jasmine.createSpy('scrollTo');
    const carousel = {
      scrollWidth: 500,
      clientWidth: 200,
      scrollLeft: 0,
      scrollTo
    } as unknown as HTMLElement;

    component.onCarouselKeydown(
      { key: 'Enter', preventDefault } as unknown as KeyboardEvent,
      carousel
    );

    expect(preventDefault).not.toHaveBeenCalled();
    expect(scrollTo).not.toHaveBeenCalled();
  });
});
