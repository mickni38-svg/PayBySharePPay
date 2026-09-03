import { of, throwError } from 'rxjs';

import { ProfileComponent } from './profile.component';

describe('ProfileComponent', () => {
  function createComponent(options?: {
    mode?: string | null;
    loggedIn?: boolean;
    userId?: number | null;
    userType?: 'Person' | 'Merchant' | null;
    loginResult?: any;
  }) {
    const response = {
      token: 'token',
      participantId: 7,
      name: 'Test User',
      participantType: 'Person' as const,
      expiresAt: '2099-01-01T00:00:00Z'
    };
    const auth = {
      logout: jasmine.createSpy('logout'),
      login: jasmine.createSpy('login').and.returnValue(options?.loginResult ?? of(response)),
      register: jasmine.createSpy('register').and.returnValue(of(response)),
      registerMerchant: jasmine.createSpy('registerMerchant').and.returnValue(of({
        ...response,
        name: 'Butiksejer',
        participantType: 'Merchant'
      })),
      googleLogin: jasmine.createSpy('googleLogin').and.returnValue(of(response)),
      updateStoredName: jasmine.createSpy('updateStoredName'),
      isLoggedIn: jasmine.createSpy('isLoggedIn').and.returnValue(options?.loggedIn ?? false),
      currentUserId: jasmine.createSpy('currentUserId').and.returnValue(options?.userId ?? null),
      currentUserName: jasmine.createSpy('currentUserName').and.returnValue(null),
      currentUserType: jasmine.createSpy('currentUserType').and.returnValue(options?.userType ?? null)
    };
    const router = { navigate: jasmine.createSpy('navigate') };
    const route = {
      snapshot: {
        queryParamMap: {
          get: jasmine.createSpy('get').and.returnValue(options?.mode ?? null)
        }
      }
    };
    const profileService = {
      getProfile: jasmine.createSpy('getProfile').and.returnValue(of({
        id: 7,
        type: options?.userType ?? 'Person',
        name: 'Test User',
        email: 'test@example.com'
      })),
      updateProfile: jasmine.createSpy('updateProfile').and.returnValue(of({})),
      getVippsTestPersons: jasmine.createSpy('getVippsTestPersons').and.returnValue(of([])),
      setVippsTestUser: jasmine.createSpy('setVippsTestUser').and.returnValue(of({}))
    };
    const themeService = {
      currentTheme: jasmine.createSpy('currentTheme').and.returnValue('standard'),
      setTheme: jasmine.createSpy('setTheme')
    };
    const directory = {
      search: jasmine.createSpy('search').and.returnValue(of([
        { id: 1, type: 'Person', name: 'Person', email: 'person@example.com' },
        { id: 2, type: 'Merchant', name: 'Merchant', email: 'merchant@example.com' }
      ]))
    };
    const devService = {
      resetData: jasmine.createSpy('resetData').and.returnValue(of({}))
    };

    const component = new ProfileComponent(
      auth as any,
      router as any,
      route as any,
      profileService as any,
      themeService as any,
      directory as any,
      devService as any
    );

    return { component, auth, router, profileService, directory };
  }

  beforeEach(() => localStorage.clear());

  it('opens registration from the canonical profile route', () => {
    const { component } = createComponent({ mode: 'register' });

    component.ngOnInit();

    expect(component.mainTab()).toBe('account');
    expect(component.accountMode()).toBe('register');
  });

  it('keeps authenticated users on profile even when an old auth mode is requested', () => {
    const { component } = createComponent({
      mode: 'register',
      loggedIn: true,
      userId: 7,
      userType: 'Person'
    });

    component.ngOnInit();
    component.setAccountMode('login');

    expect(component.accountMode()).toBe('profile');
  });

  it('opens profile accordion by default for authenticated users', () => {
    const { component } = createComponent({
      loggedIn: true,
      userId: 7,
      userType: 'Person'
    });

    component.ngOnInit();

    expect(component.accordionSection()).toBe('profile');
  });

  it('opens settings and closes profile', () => {
    const { component } = createComponent({ loggedIn: true });

    component.toggleAccordion('settings');

    expect(component.accordionSection()).toBe('settings');
  });

  it('can close the active accordion so both sections are closed', () => {
    const { component } = createComponent({ loggedIn: true });

    component.toggleAccordion('profile');

    expect(component.accordionSection()).toBeNull();
  });

  it('keeps unsaved profile state when switching accordions', () => {
    const { component } = createComponent({ loggedIn: true });
    component.name.set('Ikke gemt navn');

    component.toggleAccordion('settings');
    component.toggleAccordion('profile');

    expect(component.name()).toBe('Ikke gemt navn');
  });

  it('does not make API calls when toggling accordions', () => {
    const { component, profileService, directory } = createComponent({ loggedIn: true });

    component.toggleAccordion('settings');
    component.toggleAccordion('profile');

    expect(profileService.getProfile).not.toHaveBeenCalled();
    expect(profileService.updateProfile).not.toHaveBeenCalled();
    expect(profileService.getVippsTestPersons).not.toHaveBeenCalled();
    expect(directory.search).not.toHaveBeenCalled();
  });

  it('does not eagerly load Vipps or developer data', () => {
    const { component, profileService, directory } = createComponent({
      loggedIn: true,
      userId: 7,
      userType: 'Person'
    });

    component.ngOnInit();

    expect(profileService.getProfile).toHaveBeenCalledWith(7);
    expect(profileService.getVippsTestPersons).not.toHaveBeenCalled();
    expect(directory.search).not.toHaveBeenCalled();
  });

  it('loads Vipps mapping only when an authenticated person opens the tab', () => {
    const { component, profileService } = createComponent({
      loggedIn: true,
      userId: 7,
      userType: 'Person'
    });

    component.selectMainTab('vipps');

    expect(component.mainTab()).toBe('vipps');
    expect(profileService.getVippsTestPersons).toHaveBeenCalledTimes(1);
  });

  it('does not expose Vipps mapping to merchants', () => {
    const { component, profileService } = createComponent({
      loggedIn: true,
      userId: 7,
      userType: 'Merchant'
    });

    component.selectMainTab('vipps');

    expect(component.mainTab()).toBe('account');
    expect(profileService.getVippsTestPersons).not.toHaveBeenCalled();
  });

  it('registers a merchant with account credentials and Vipps MSN', () => {
    const { component, auth, router } = createComponent();
    component.merchantName = '  Mia  ';
    component.merchantCompany = '  Mia ApS  ';
    component.merchantEmail = '  mia@example.com  ';
    component.merchantPassword = 'hemmelig';
    component.merchantPasswordConfirm = 'hemmelig';
    component.merchantMsn = '  123456  ';

    component.registerMerchant();

    expect(auth.registerMerchant).toHaveBeenCalledWith(jasmine.objectContaining({
      name: 'Mia',
      companyName: 'Mia ApS',
      email: 'mia@example.com',
      password: 'hemmelig',
      vippsMerchantSerialNumber: '123456'
    }));
    expect(component.profileType()).toBe('Merchant');
    expect(router.navigate).toHaveBeenCalledWith(['/profile'], {
      queryParams: { mode: 'profile' },
      replaceUrl: true
    });
  });

  it('sends ordinary users home after login', () => {
    const { component, auth, router } = createComponent();
    component.loginEmail = ' person@example.com ';
    component.loginPassword = 'hemmelig';

    component.login();

    expect(auth.login).toHaveBeenCalledWith('person@example.com', 'hemmelig');
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('keeps login errors generic', () => {
    const { component, router } = createComponent({
      loginResult: throwError(() => ({ status: 401 }))
    });
    component.loginEmail = 'unknown@example.com';
    component.loginPassword = 'forkert';

    component.login();

    expect(component.loginError()).toBe('Email eller adgangskode er forkert.');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('loads only person accounts for the developer tab', () => {
    const { component, directory } = createComponent();

    component.selectMainTab('developer');

    expect(directory.search).toHaveBeenCalledWith('');
    expect(component.persons().map(person => person.type)).toEqual(['Person']);
  });

  it('uses passwordless login only from the non-production developer tab', () => {
    const { component, auth, router } = createComponent();
    component.selectedEmail = 'developer@paynsync.dk';

    component.developerLogin();

    expect(auth.login).toHaveBeenCalledWith('developer@paynsync.dk', undefined);
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('clears the session and returns to profile login', () => {
    const { component, auth, router } = createComponent();

    component.logout();

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/profile'], {
      queryParams: { mode: 'login' },
      replaceUrl: true
    });
  });
});
