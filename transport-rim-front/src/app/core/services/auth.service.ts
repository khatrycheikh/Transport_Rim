import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '../models/auth.model';

const STORAGE_KEY = 'transport-rim-auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly currentUser = signal<AuthResponse | null>(this.getStoredUser());
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly role = computed<UserRole | null>(() => this.currentUser()?.role ?? null);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((user) => this.updateCurrentUser(user)));
  }

  register(request: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
      .pipe(tap((user) => this.updateCurrentUser(user)));
  }

  logout(): void {
    this.currentUser.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  updateCurrentUser(user: AuthResponse): void {
    this.currentUser.set(user);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
  }

  get token(): string | null {
    return this.currentUser()?.token ?? null;
  }

  private getStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}
