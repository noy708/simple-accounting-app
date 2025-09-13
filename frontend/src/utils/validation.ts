import * as yup from 'yup';
import { TransactionType } from '@/types/transaction';

/**
 * 取引作成フォームのバリデーションスキーマ
 */
export const createTransactionSchema = yup.object({
  amount: yup
    .number()
    .required('金額は必須です')
    .positive('金額は正の数値である必要があります')
    .max(999999999, '金額が大きすぎます'),
  description: yup
    .string()
    .required('説明は必須です')
    .max(200, '説明は200文字以内で入力してください')
    .trim(),
  type: yup
    .number()
    .oneOf([TransactionType.Income, TransactionType.Expense], '有効な取引タイプを選択してください')
    .required('取引タイプは必須です'),
  date: yup
    .date()
    .required('日付は必須です')
    .max(new Date(), '未来の日付は選択できません'),
});

/**
 * フォームデータの型
 */
export type CreateTransactionFormData = yup.InferType<typeof createTransactionSchema>;

/**
 * バリデーションエラーメッセージを取得
 */
export const getValidationErrorMessage = (error: yup.ValidationError): string => {
  return error.errors[0] || 'バリデーションエラーが発生しました';
};