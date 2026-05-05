import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  // Clone request and add common headers
  const clonedRequest = req.clone({
    setHeaders: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  });

  return next(clonedRequest).pipe(
    catchError((error) => {
      // Handle errors globally
      console.error('API Error:', error);

      // You can add error handling logic here
      // e.g., show toast notification, redirect to error page, etc.

      return throwError(() => error);
    })
  );
};
