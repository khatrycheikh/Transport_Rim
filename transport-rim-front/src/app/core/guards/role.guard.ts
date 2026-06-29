import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.model';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const requiredRole = route.data['role'] as UserRole | undefined;

  if (!requiredRole || auth.role() === requiredRole) {
    return true;
  }

  return inject(Router).createUrlTree(['/']);
};
