import { of, throwError } from 'rxjs';

import { ProfileComponent } from './profile.component';

describe('ProfileComponent', () => {
  function createComponent(options?: {
    loginResult?: any;
  }): {
    component: ProfileComponent;
    auth: {
      logout: jasmine.Spy;
      login: jasmine.Spy;
      isLoggedIn: jasmine.Spy;
    };
    router: { navigate: jasmine.Spy };
  } {
    const auth = {
      logout: jasmine.createSpy('logout'),
      login: jasmine.createSpy('login').and.returnValue(options?.loginResult ?? of({})),
      isLoggedIn: jasmine.createSpy('isLoggedIn').and.returnValue(false),
      currentUserId: jasmine.createSpy('currentUserId').and.returnValue(null)
    };
    const router = {
      navigate: jasmine.createSpy('navigate')
    };

    return {
      component: new ProfileComponent(
        auth as any,
        router as any,
        {} as any,
        {} as any,
        {} as any,
        {} as any
      ),
      auth,
      router
    };
  }

  it('clears the session and returns to login', () => {
    const { component, auth, router } = createComponent();

    component.logout();

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('opens and closes the developer panel', () => {
    const { component } = createComponent();

    expect(component.devPanelOpen()).toBeFalse();
    component.toggleDevPanel();
    expect(component.devPanelOpen()).toBeTrue();
    component.toggleDevPanel();
    expect(component.devPanelOpen()).toBeFalse();
  });

  it('logs in with the selected developer account and returns home', () => {
    const { component, auth, router } = createComponent();
    component.selectedEmail = 'developer@paynsync.dk';

    component.devLogin();

    expect(auth.login).toHaveBeenCalledWith('developer@paynsync.dk', '');
    expect(component.loginLoading()).toBeFalse();
    expect(component.loginError()).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('keeps the developer on profile and shows the API error when login fails', () => {
    const { component, router } = createComponent({
      loginResult: throwError(() => ({ error: { message: 'Ukendt testbruger' } }))
    });
    component.selectedEmail = 'unknown@paynsync.dk';

    component.devLogin();

    expect(component.loginLoading()).toBeFalse();
    expect(component.loginError()).toBe('Ukendt testbruger');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('does not attempt developer login without a selected account', () => {
    const { component, auth } = createComponent();

    component.devLogin();

    expect(auth.login).not.toHaveBeenCalled();
  });
});
