import React, { useMemo } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Box,
  useTheme,
  Paper,
} from '@mui/material';
import {
  Receipt,
  TrendingUp,
  TrendingDown,
  InboxOutlined,
} from '@mui/icons-material';
import { Transaction, TransactionType } from '../types/transaction';

interface TransactionListProps {
  transactions: Transaction[];
  loading?: boolean;
}

export const TransactionList: React.FC<TransactionListProps> = ({
  transactions,
  loading = false,
}) => {
  const theme = useTheme();

  // Sort transactions by date (newest first)
  const sortedTransactions = useMemo(() => {
    return [...transactions].sort((a, b) => {
      const dateA = new Date(a.date);
      const dateB = new Date(b.date);
      return dateB.getTime() - dateA.getTime();
    });
  }, [transactions]);

  // Format date for display
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('ja-JP', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(date);
  };

  // Format amount with currency
  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('ja-JP', {
      style: 'currency',
      currency: 'JPY',
    }).format(amount);
  };

  // Get transaction type info
  const getTransactionTypeInfo = (type: string) => {
    // Backend returns type as string, need to convert
    const isIncome = type === 'Income' || parseInt(type) === TransactionType.Income;
    
    return {
      label: isIncome ? '収入' : '支出',
      color: isIncome ? 'success' : 'error',
      icon: isIncome ? <TrendingUp /> : <TrendingDown />,
      amountColor: isIncome ? theme.palette.success.main : theme.palette.error.main,
    };
  };

  // Empty state component
  const EmptyState = () => (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      py={6}
      textAlign="center"
    >
      <InboxOutlined
        sx={{
          fontSize: '4rem',
          color: theme.palette.text.disabled,
          mb: 2,
        }}
      />
      <Typography
        variant="h6"
        color="textSecondary"
        gutterBottom
      >
        取引がありません
      </Typography>
      <Typography
        variant="body2"
        color="textSecondary"
      >
        新しい取引を追加して開始しましょう
      </Typography>
    </Box>
  );

  // Loading state
  if (loading) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" component="h2" gutterBottom>
            取引履歴
          </Typography>
          <Box
            display="flex"
            justifyContent="center"
            alignItems="center"
            py={4}
          >
            <Typography color="textSecondary">
              読み込み中...
            </Typography>
          </Box>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardContent>
        <Box
          display="flex"
          alignItems="center"
          mb={2}
        >
          <Receipt
            sx={{
              mr: 1,
              color: theme.palette.primary.main,
            }}
          />
          <Typography variant="h6" component="h2">
            取引履歴
          </Typography>
          {transactions.length > 0 && (
            <Chip
              label={`${transactions.length}件`}
              size="small"
              sx={{ ml: 'auto' }}
            />
          )}
        </Box>

        {sortedTransactions.length === 0 ? (
          <EmptyState />
        ) : (
          <TableContainer component={Paper} elevation={0}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>日付</TableCell>
                  <TableCell>説明</TableCell>
                  <TableCell>タイプ</TableCell>
                  <TableCell align="right">金額</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {sortedTransactions.map((transaction) => {
                  const typeInfo = getTransactionTypeInfo(transaction.type);
                  
                  return (
                    <TableRow
                      key={transaction.id}
                      hover
                      sx={{
                        '&:last-child td, &:last-child th': { border: 0 },
                      }}
                    >
                      <TableCell>
                        <Typography variant="body2">
                          {formatDate(transaction.date)}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Typography
                          variant="body2"
                          sx={{
                            maxWidth: '200px',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap',
                          }}
                          title={transaction.description}
                        >
                          {transaction.description}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Chip
                          icon={typeInfo.icon}
                          label={typeInfo.label}
                          color={typeInfo.color as 'success' | 'error'}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      
                      <TableCell align="right">
                        <Typography
                          variant="body2"
                          sx={{
                            color: typeInfo.amountColor,
                            fontWeight: 'medium',
                          }}
                        >
                          {typeInfo.label === '収入' ? '+' : '-'}
                          {formatAmount(transaction.amount)}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </CardContent>
    </Card>
  );
};