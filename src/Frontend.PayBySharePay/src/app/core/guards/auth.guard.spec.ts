import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard UC-17', () => {
  let auth: { isLoggedIn: jasmine.Spy };
  let router: { createUrlTree: jasmine.Spy };

  beforeEach(() => {
    auth = { isLoggedIn: jasmine.createSpy('isLoggedIn') };
    router = { createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue('login-tree') };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router }
      ]
    });
  });

  it('allows authenticated users to open private routes', () => {
    auth.isLoggedIn.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

    expect(result).toBeTrue();
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('redirects unauthenticated users to profile login mode', () => {
    auth.isLoggedIn.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

    expect(result).toBe('login-tree' as any);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/profile'], {
      queryParams: { mode: 'login' }
    });
  });
});
