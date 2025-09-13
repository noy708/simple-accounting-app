import axios, { AxiosResponse } from 'axios';
import { Transaction, CreateTransactionDto, BalanceResponse } from '@/types/transaction';
import { handleApiError } from '@/utils/errorHandler';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10秒のタイムアウト
});

// レスポンスインターセプターでエラーハンドリング
apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error) => {
    console.error('API Error:', handleApiError(error));
    return Promise.reject(error);
  }
);

export const transactionService = {
  /**
   * 全ての取引を取得
   */
  async getTransactions(): Promise<Transaction[]> {
    try {
      const response = await apiClient.get<Transaction[]>('/transactions');
      return response.data;
    } catch (error) {
      throw handleApiError(error);
    }
  },

  /**
   * 新しい取引を作成
   */
  async createTransaction(transaction: CreateTransactionDto): Promise<Transaction> {
    try {
      const response = await apiClient.post<Transaction>('/transactions', transaction);
      return response.data;
    } catch (error) {
      throw handleApiError(error);
    }
  },

  /**
   * 現在の残高を取得
   */
  async getBalance(): Promise<number> {
    try {
      const response = await apiClient.get<BalanceResponse>('/transactions/balance');
      return response.data.balance;
    } catch (error) {
      throw handleApiError(error);
    }
  },

  /**
   * 仕訳帳をPDF形式でダウンロード
   */
  async downloadPdf(): Promise<Blob> {
    try {
      console.log('PDF download request to:', `${API_BASE_URL}/transactions/pdf`);
      const response = await apiClient.get('/transactions/pdf', {
        responseType: 'blob',
        timeout: 30000, // PDF生成は時間がかかる可能性があるため30秒に設定
      });
      console.log('PDF download response:', response.status, response.headers);
      return response.data;
    } catch (error) {
      console.error('PDF download error details:', error);
      throw handleApiError(error);
    }
  },
};

export default apiClient;