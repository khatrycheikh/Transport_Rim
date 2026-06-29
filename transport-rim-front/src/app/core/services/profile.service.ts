import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/auth.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  constructor(
    private readonly http: HttpClient,
    private readonly auth: AuthService,
  ) {}

  updateProfile(name: string, phoneNumber: string) {
    return this.http
      .put<AuthResponse>(`${environment.apiUrl}/profile`, { name, phoneNumber })
      .pipe(tap((user) => this.auth.updateCurrentUser(user)));
  }
}
