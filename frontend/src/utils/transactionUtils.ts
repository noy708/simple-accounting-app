import { Transaction, TransactionType } from '@/types/transaction';

/**
 * 取引タイプを日本語に変換
 */
export const getTransactionTypeLabel = (type: string | TransactionType): string => {
  if (typeof type === 'string') {
    return type === 'Income' ? '収入' : '支出';
  }
  return type === TransactionType.Income ? '収入' : '支出';
};

/**
 * 取引タイプに応じた色を取得
 */
export const getTransactionTypeColor = (type: string | TransactionType): 'success' | 'error' => {
  if (typeof type === 'string') {
    return type === 'Income' ? 'success' : 'error';
  }
  return type === TransactionType.Income ? 'success' : 'error';
};

/**
 * 金額をフォーマット
 */
export const formatAmount = (amount: number): string => {
  return new Intl.NumberFormat('ja-JP', {
    style: 'currency',
    currency: 'JPY',
  }).format(amount);
};

/**
 * 日付をフォーマット
 */
export const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ja-JP', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).format(date);
};

/**
 * 日付時刻をフォーマット
 */
export const formatDateTime = (dateString: string): string => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ja-JP', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
};

/**
 * 取引リストを日付順（新しい順）でソート
 */
export const sortTransactionsByDate = (transactions: Transaction[]): Transaction[] => {
  return [...transactions].sort((a, b) => 
    new Date(b.date).getTime() - new Date(a.date).getTime()
  );
};

/**
 * 残高を計算
 */
export const calculateBalance = (transactions: Transaction[]): number => {
  return transactions.reduce((balance, transaction) => {
    const amount = transaction.amount;
    if (transaction.type === 'Income' || transaction.type === TransactionType.Income.toString()) {
      return balance + amount;
    } else {
      return balance - amount;
    }
  }, 0);
};