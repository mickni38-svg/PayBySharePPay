import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  };

  if (token && !req.url.includes('/api/auth/login')) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const clonedRequest = req.clone({ setHeaders: headers });

  return next(clonedRequest).pipe(
    catchError((error) => {
      console.error('API Error:', error);
      return throwError(() => error);
    })
  );
};
