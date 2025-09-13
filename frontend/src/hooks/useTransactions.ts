import { useState, useEffect, useCallback } from 'react';
import { Transaction, CreateTransactionDto } from '@/types/transaction';
import { transactionService } from '@/services/api';
import { handleApiError, ApiError } from '@/utils/errorHandler';

interface UseTransactionsReturn {
  transactions: Transaction[];
  balance: number;
  loading: boolean;
  creating: boolean;
  error: ApiError | null;
  createTransaction: (transaction: CreateTransactionDto) => Promise<void>;
  refreshTransactions: () => Promise<void>;
  refreshBalance: () => Promise<void>;
}

export const useTransactions = (): UseTransactionsReturn => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [balance, setBalance] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const [creating, setCreating] = useState<boolean>(false);
  const [error, setError] = useState<ApiError | null>(null);

  const refreshTransactions = useCallback(async () => {
    try {
      setError(null);
      const data = await transactionService.getTransactions();
      setTransactions(data);
    } catch (err) {
      const apiError = handleApiError(err);
      setError(apiError);
    }
  }, []);

  const refreshBalance = useCallback(async () => {
    try {
      setError(null);
      const balanceData = await transactionService.getBalance();
      setBalance(balanceData);
    } catch (err) {
      const apiError = handleApiError(err);
      setError(apiError);
    }
  }, []);

  const createTransaction = useCallback(async (transaction: CreateTransactionDto) => {
    setCreating(true);
    try {
      setError(null);
      const newTransaction = await transactionService.createTransaction(transaction);
      
      // Optimistically update the state for better UX
      setTransactions(prev => [newTransaction, ...prev]);
      
      // Then refresh data to ensure consistency
      await Promise.all([refreshTransactions(), refreshBalance()]);
    } catch (err) {
      const apiError = handleApiError(err);
      setError(apiError);
      throw apiError; // コンポーネント側でエラーハンドリングできるように再スロー
    } finally {
      setCreating(false);
    }
  }, [refreshTransactions, refreshBalance]);

  useEffect(() => {
    const loadInitialData = async () => {
      setLoading(true);
      try {
        await Promise.all([refreshTransactions(), refreshBalance()]);
      } finally {
        setLoading(false);
      }
    };

    loadInitialData();
  }, [refreshTransactions, refreshBalance]);

  return {
    transactions,
    balance,
    loading,
    creating,
    error,
    createTransaction,
    refreshTransactions,
    refreshBalance,
  };
};