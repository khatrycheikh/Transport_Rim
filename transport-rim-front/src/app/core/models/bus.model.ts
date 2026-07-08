export interface Bus {
  id: number;
  companyId: number;
  companyName: string;
  busNumber: string;
  capacity: number;
}

export interface CreateBusRequest {
  busNumber: string;
  capacity: number;
}

export interface UpdateBusRequest {
  busNumber: string;
  capacity: number;
}
