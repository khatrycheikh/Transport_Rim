export type CompanyStatus = 'Pending' | 'Active' | 'Suspended';

export interface Company {
  id: number;
  name: string;
  phone: string;
  email: string;
  address: string;
  status: CompanyStatus;
  createdAt: string;
}

export interface CreateCompanyRequest {
  name: string;
  phone: string;
  email: string;
  address: string;
  adminName: string;
  adminPhoneNumber: string;
  adminPassword: string;
}
