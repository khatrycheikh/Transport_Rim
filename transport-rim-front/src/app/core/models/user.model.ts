import { UserRole } from './auth.model';

export interface User {
  id: number;
  name: string;
  phoneNumber: string;
  role: UserRole;
  companyId: number | null;
  companyName: string | null;
}
