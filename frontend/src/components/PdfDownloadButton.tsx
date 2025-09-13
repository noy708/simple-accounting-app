'use client';

import React, { useState } from 'react';
import { Button, CircularProgress, Alert, Snackbar, SxProps, Theme } from '@mui/material';
import { Download as DownloadIcon } from '@mui/icons-material';
import { transactionService } from '@/services/api';

interface PdfDownloadButtonProps {
  variant?: 'contained' | 'outlined' | 'text';
  size?: 'small' | 'medium' | 'large';
  fullWidth?: boolean;
  sx?: SxProps<Theme>;
}

export const PdfDownloadButton: React.FC<PdfDownloadButtonProps> = ({
  variant = 'contained',
  size = 'medium',
  fullWidth = false,
  sx,
}) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleDownload = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const pdfBlob = await transactionService.downloadPdf();
      
      // ファイル名を生成
      const fileName = `仕訳帳_${new Date().toISOString().split('T')[0]}.pdf`;
      
      // ダウンロード用のリンクを作成
      const url = window.URL.createObjectURL(pdfBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      
      // ダウンロードを実行
      document.body.appendChild(link);
      link.click();
      
      // クリーンアップ
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      setSuccess(true);
    } catch (error) {
      console.error('PDF download error:', error);
      
      // より詳細なエラー情報を表示
      let errorMessage = 'PDFのダウンロード中にエラーが発生しました';
      
      if (error instanceof Error) {
        errorMessage = error.message;
      }
      
      console.log('Setting error message:', errorMessage);
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCloseError = () => {
    setError(null);
  };

  const handleCloseSuccess = () => {
    setSuccess(false);
  };

  return (
    <>
      <Button
        variant={variant}
        size={size}
        fullWidth={fullWidth}
        onClick={handleDownload}
        disabled={isLoading}
        startIcon={
          isLoading ? (
            <CircularProgress size={20} color="inherit" />
          ) : (
            <DownloadIcon />
          )
        }
        sx={{
          minWidth: '140px',
          '&.Mui-disabled': {
            backgroundColor: variant === 'contained' ? 'rgba(0, 0, 0, 0.12)' : 'transparent',
          },
          ...sx,
        }}
      >
        {isLoading ? 'PDF生成中...' : 'PDF生成'}
      </Button>

      {/* エラー通知 */}
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={handleCloseError}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert 
          onClose={handleCloseError} 
          severity="error" 
          sx={{ width: '100%' }}
        >
          {error}
        </Alert>
      </Snackbar>

      {/* 成功通知 */}
      <Snackbar
        open={success}
        autoHideDuration={3000}
        onClose={handleCloseSuccess}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert 
          onClose={handleCloseSuccess} 
          severity="success" 
          sx={{ width: '100%' }}
        >
          PDFのダウンロードが完了しました
        </Alert>
      </Snackbar>
    </>
  );
};

export default PdfDownloadButton;