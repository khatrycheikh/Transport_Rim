export type UserRole = 'Admin' | 'Company' | 'Traveler';

export interface AuthResponse {
  id: number;
  name: string;
  phoneNumber: string;
  role: UserRole;
  token: string;
}

export interface LoginRequest {
  phoneNumber: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  phoneNumber: string;
  password: string;
  role: UserRole;
}
