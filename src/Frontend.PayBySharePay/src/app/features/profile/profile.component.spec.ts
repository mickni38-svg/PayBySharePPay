import { ProfileComponent } from './profile.component';

describe('ProfileComponent logout', () => {
  it('clears the session and returns to login', () => {
    const auth = {
      logout: jasmine.createSpy('logout')
    };
    const router = {
      navigate: jasmine.createSpy('navigate')
    };
    const component = new ProfileComponent(
      auth as any,
      router as any,
      {} as any,
      {} as any
    );

    component.logout();

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
