export type PaymentMethod = 'Bankily' | 'Masrivi' | 'CarteBancaire' | 'Cash';

export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded';

export interface CreatePaymentRequest {
  reservationId: number;
  method: PaymentMethod;
  transactionId: string;
}

export interface Payment {
  id: number;
  reservationId: number;
  amount: number;
  method: PaymentMethod;
  transactionId: string;
  status: PaymentStatus;
  createdAt: string;
}
