import { AxiosError } from 'axios';

export interface ApiError {
  message: string;
  status?: number;
  details?: string;
}

export const handleApiError = (error: unknown): ApiError => {
  console.log('Error details:', error);
  
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    const message = error.response?.data?.message || error.message;
    
    console.log('Axios error details:', {
      status,
      message,
      config: error.config,
      response: error.response?.data
    });
    
    switch (status) {
      case 400:
        return {
          message: 'リクエストが無効です。入力内容を確認してください。',
          status,
          details: message,
        };
      case 404:
        return {
          message: 'リソースが見つかりません。',
          status,
          details: message,
        };
      case 500:
        return {
          message: 'サーバーエラーが発生しました。しばらく時間をおいて再試行してください。',
          status,
          details: message,
        };
      default:
        return {
          message: 'ネットワークエラーが発生しました。接続を確認してください。',
          status,
          details: message,
        };
    }
  }
  
  return {
    message: '予期しないエラーが発生しました。',
    details: error instanceof Error ? error.message : String(error),
  };
};

export const showErrorMessage = (error: unknown): string => {
  const apiError = handleApiError(error);
  return apiError.message;
};