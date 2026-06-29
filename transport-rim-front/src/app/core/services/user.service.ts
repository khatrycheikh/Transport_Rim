import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private readonly http: HttpClient) {}

  getAll(search?: string) {
    const params = search ? new HttpParams().set('search', search) : undefined;
    return this.http.get<User[]>(`${environment.apiUrl}/users`, { params });
  }

  delete(id: number) {
    return this.http.delete<void>(`${environment.apiUrl}/users/${id}`);
  }
}
