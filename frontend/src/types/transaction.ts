export interface Transaction {
  id: number;
  amount: number;
  description: string;
  type: string; // Backend returns as string
  date: string;
  createdAt: string;
}

export interface CreateTransactionDto {
  amount: number;
  description: string;
  type: TransactionType;
  date: string; // ISO date string format
}

export enum TransactionType {
  Income = 0,
  Expense = 1,
}

export interface BalanceResponse {
  balance: number;
}

export interface ApiErrorResponse {
  message: string;
  error?: string;
}