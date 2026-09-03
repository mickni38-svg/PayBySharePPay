import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Protects user-specific routes. Public entry points remain /home and /profile.
 * Unauthenticated users are sent to the canonical account center in login mode.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);

  if (auth.isLoggedIn()) {
    return true;
  }

  return inject(Router).createUrlTree(['/profile'], {
    queryParams: { mode: 'login' }
  });
};
