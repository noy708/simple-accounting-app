'use client';

import React, { useState, useCallback, useEffect } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Container,
  Box,
  useTheme,
  useMediaQuery,
  Snackbar,
  Alert,
  Fade,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
} from '@mui/material';
import Grid from '@mui/material/Grid';
import { AccountBalance, Help } from '@mui/icons-material';
import { BalanceDisplay, TransactionForm, TransactionList, PdfDownloadButton } from '@/components';
import { useTransactions } from '@/hooks/useTransactions';
import { CreateTransactionDto } from '@/types/transaction';
import { showErrorMessage } from '@/utils/errorHandler';

interface NotificationState {
  open: boolean;
  message: string;
  severity: 'success' | 'error' | 'warning' | 'info';
}

export default function Dashboard() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const {
    transactions,
    balance,
    loading,
    creating,
    error,
    createTransaction,
  } = useTransactions();

  // Help dialog state
  const [helpOpen, setHelpOpen] = useState(false);

  // Notification state management
  const [notification, setNotification] = useState<NotificationState>({
    open: false,
    message: '',
    severity: 'info',
  });

  // Handle transaction creation with notifications
  const handleCreateTransaction = useCallback(async (transactionData: CreateTransactionDto) => {
    try {
      await createTransaction(transactionData);

      // Show success notification
      setNotification({
        open: true,
        message: '取引が正常に追加されました',
        severity: 'success',
      });
    } catch (error) {
      // Show error notification
      setNotification({
        open: true,
        message: showErrorMessage(error),
        severity: 'error',
      });
    }
  }, [createTransaction]);

  // Handle notification close
  const handleCloseNotification = useCallback((event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') {
      return;
    }
    setNotification(prev => ({ ...prev, open: false }));
  }, []);

  // Show error notification when global error occurs
  useEffect(() => {
    if (error) {
      setNotification({
        open: true,
        message: error.message,
        severity: 'error',
      });
    }
  }, [error]);

  // Manual refresh handlers - removed unused handleRefreshData

  return (
    <Box sx={{ flexGrow: 1, minHeight: '100vh', backgroundColor: theme.palette.grey[50] }}>
      {/* AppBar Header */}
      <AppBar position="static" elevation={2}>
        <Toolbar>
          <AccountBalance sx={{ mr: 2 }} />
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            シンプル会計アプリ
          </Typography>
          {!isMobile && !loading && (
            <Fade in={!loading}>
              <Typography variant="body2" sx={{ opacity: 0.8, mr: 2 }}>
                現在の残高: ¥{balance.toLocaleString('ja-JP')}
              </Typography>
            </Fade>
          )}
          <PdfDownloadButton
            variant="outlined"
            size="small"
            sx={{
              color: 'white',
              borderColor: 'white',
              '&:hover': {
                borderColor: 'rgba(255, 255, 255, 0.7)',
                backgroundColor: 'rgba(255, 255, 255, 0.1)'
              }
            }}
          />
          <Tooltip title="ヘルプ">
            <IconButton
              color="inherit"
              onClick={() => setHelpOpen(true)}
              sx={{ ml: 1 }}
            >
              <Help />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </AppBar>

      {/* Main Content */}
      <Container maxWidth="xl" sx={{ mt: 3, mb: 4 }}>
        <Grid container spacing={3}>
          {/* Balance Display - Full width on mobile, half on desktop */}
          <Grid size={{ xs: 12, md: 6 }}>
            <BalanceDisplay balance={balance} loading={loading} />
          </Grid>

          {/* Transaction Form - Full width on mobile, half on desktop */}
          <Grid size={{ xs: 12, md: 6 }}>
            <TransactionForm
              onSubmit={handleCreateTransaction}
              loading={creating}
            />
          </Grid>

          {/* PDF Download Button - Mobile only */}
          {isMobile && (
            <Grid size={{ xs: 12 }}>
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
                <PdfDownloadButton
                  variant="contained"
                  size="medium"
                  fullWidth
                />
              </Box>
            </Grid>
          )}

          {/* Transaction List - Full width */}
          <Grid size={{ xs: 12 }}>
            <TransactionList
              transactions={transactions}
              loading={loading}
            />
          </Grid>
        </Grid>
      </Container>

      {/* Help Dialog */}
      <Dialog
        open={helpOpen}
        onClose={() => setHelpOpen(false)}
        aria-labelledby="help-dialog-title"
      >
        <DialogTitle id="help-dialog-title">
          ヘルプ
        </DialogTitle>
        <DialogContent>
          <DialogContentText paragraph>
            このアプリケーションは、日々の取引を記録し、収支を管理するためのシンプル会計アプリです。
          </DialogContentText>
          <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
            主な機能:
          </Typography>
          <Box component="ul" sx={{ pl: 2, m: 0 }}>
            <li>
              <Typography variant="body2">
                <strong>取引の追加:</strong> 日付、説明、金額、カテゴリを入力して取引を記録します。
              </Typography>
            </li>
            <li>
              <Typography variant="body2">
                <strong>残高表示:</strong> 現在の総残高をリアルタイムで確認できます。
              </Typography>
            </li>
            <li>
              <Typography variant="body2">
                <strong>PDF出力:</strong> 取引履歴をPDF形式でダウンロードして保存できます。
              </Typography>
            </li>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHelpOpen(false)} autoFocus>
            閉じる
          </Button>
        </DialogActions>
      </Dialog>

      {/* Notification Snackbar */}
      <Snackbar
        open={notification.open}
        autoHideDuration={6000}
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          onClose={handleCloseNotification}
          severity={notification.severity}
          variant="filled"
          sx={{ width: '100%' }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}