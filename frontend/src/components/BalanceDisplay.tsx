import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  useTheme,
} from '@mui/material';
import { AccountBalance } from '@mui/icons-material';

interface BalanceDisplayProps {
  balance: number;
  loading?: boolean;
}

export const BalanceDisplay: React.FC<BalanceDisplayProps> = ({
  balance,
  loading = false,
}) => {
  const theme = useTheme();

  // Determine color based on balance value
  const getBalanceColor = (amount: number) => {
    if (amount > 0) return theme.palette.success.main;
    if (amount < 0) return theme.palette.error.main;
    return theme.palette.text.primary;
  };

  // Format balance with currency symbol
  const formatBalance = (amount: number) => {
    return new Intl.NumberFormat('ja-JP', {
      style: 'currency',
      currency: 'JPY',
    }).format(amount);
  };

  return (
    <Card>
      <CardContent>
        <Box
          display="flex"
          alignItems="center"
          justifyContent="center"
          flexDirection="column"
          textAlign="center"
        >
          <Box
            display="flex"
            alignItems="center"
            mb={1}
          >
            <AccountBalance
              sx={{
                mr: 1,
                color: theme.palette.primary.main,
                fontSize: '1.5rem',
              }}
            />
            <Typography
              variant="h6"
              component="h2"
              color="textSecondary"
            >
              現在の残高
            </Typography>
          </Box>

          {loading ? (
            <Typography
              variant="h4"
              component="div"
              color="textSecondary"
            >
              読み込み中...
            </Typography>
          ) : (
            <Typography
              variant="h4"
              component="div"
              sx={{
                color: getBalanceColor(balance),
                fontWeight: 'bold',
                transition: 'color 0.3s ease',
              }}
            >
              {formatBalance(balance)}
            </Typography>
          )}

          {!loading && (
            <Typography
              variant="body2"
              color="textSecondary"
              sx={{ mt: 1 }}
            >
              {balance > 0 && '黒字です'}
              {balance < 0 && '赤字です'}
              {balance === 0 && '収支は均衡しています'}
            </Typography>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};