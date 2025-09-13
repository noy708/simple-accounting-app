import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { TransactionList } from '../TransactionList';
import { Transaction, TransactionType } from '../../types/transaction';

// Create a theme for testing
const theme = createTheme();

// Helper function to render component with theme
const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

// Mock transaction data
const mockTransactions: Transaction[] = [
  {
    id: 1,
    amount: 1000,
    description: 'Salary',
    type: 'Income',
    date: '2025-01-01',
    createdAt: '2025-01-01T00:00:00Z',
  },
  {
    id: 2,
    amount: 500,
    description: 'Groceries',
    type: 'Expense',
    date: '2025-01-02',
    createdAt: '2025-01-02T00:00:00Z',
  },
  {
    id: 3,
    amount: 200,
    description: 'Coffee',
    type: 'Expense',
    date: '2024-12-31',
    createdAt: '2024-12-31T00:00:00Z',
  },
];

describe('TransactionList', () => {
  it('renders transaction list title', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('取引履歴')).toBeInTheDocument();
  });

  it('displays transaction count', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('3件')).toBeInTheDocument();
  });

  it('renders table headers', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('日付')).toBeInTheDocument();
    expect(screen.getByText('説明')).toBeInTheDocument();
    expect(screen.getByText('タイプ')).toBeInTheDocument();
    expect(screen.getByText('金額')).toBeInTheDocument();
  });

  it('displays transactions in correct order (newest first)', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    const rows = screen.getAllByRole('row');
    // Skip header row (index 0)
    const firstDataRow = rows[1];
    const secondDataRow = rows[2];
    const thirdDataRow = rows[3];
    
    // Check that transactions are ordered by date (newest first)
    expect(firstDataRow).toHaveTextContent('Groceries'); // 2025-01-02
    expect(secondDataRow).toHaveTextContent('Salary'); // 2025-01-01
    expect(thirdDataRow).toHaveTextContent('Coffee'); // 2024-12-31
  });

  it('displays income transactions correctly', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('Salary')).toBeInTheDocument();
    expect(screen.getByText('収入')).toBeInTheDocument();
    expect(screen.getByText(/￥1,000/)).toBeInTheDocument();
  });

  it('displays expense transactions correctly', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('Groceries')).toBeInTheDocument();
    expect(screen.getAllByText('支出')).toHaveLength(2); // Two expense transactions
    expect(screen.getByText(/￥500/)).toBeInTheDocument();
    expect(screen.getByText(/￥200/)).toBeInTheDocument();
  });

  it('formats dates correctly', () => {
    renderWithTheme(<TransactionList transactions={mockTransactions} />);
    
    expect(screen.getByText('2025/01/01')).toBeInTheDocument();
    expect(screen.getByText('2025/01/02')).toBeInTheDocument();
    expect(screen.getByText('2024/12/31')).toBeInTheDocument();
  });

  it('shows empty state when no transactions', () => {
    renderWithTheme(<TransactionList transactions={[]} />);
    
    expect(screen.getByText('取引がありません')).toBeInTheDocument();
    expect(screen.getByText('新しい取引を追加して開始しましょう')).toBeInTheDocument();
  });

  it('shows loading state when loading prop is true', () => {
    renderWithTheme(<TransactionList transactions={[]} loading={true} />);
    
    expect(screen.getByText('読み込み中...')).toBeInTheDocument();
    expect(screen.queryByText('取引がありません')).not.toBeInTheDocument();
  });

  it('handles long descriptions with ellipsis', () => {
    const longDescriptionTransaction: Transaction = {
      id: 4,
      amount: 100,
      description: 'This is a very long description that should be truncated with ellipsis when displayed in the table',
      type: 'Expense',
      date: '2025-01-01',
      createdAt: '2025-01-01T00:00:00Z',
    };

    renderWithTheme(<TransactionList transactions={[longDescriptionTransaction]} />);
    
    const descriptionCell = screen.getByText(longDescriptionTransaction.description);
    expect(descriptionCell).toBeInTheDocument();
    expect(descriptionCell).toHaveAttribute('title', longDescriptionTransaction.description);
  });

  it('handles numeric transaction types correctly', () => {
    const numericTypeTransactions: Transaction[] = [
      {
        id: 5,
        amount: 300,
        description: 'Numeric Income',
        type: TransactionType.Income.toString(), // "0"
        date: '2025-01-01',
        createdAt: '2025-01-01T00:00:00Z',
      },
      {
        id: 6,
        amount: 150,
        description: 'Numeric Expense',
        type: TransactionType.Expense.toString(), // "1"
        date: '2025-01-01',
        createdAt: '2025-01-01T00:00:00Z',
      },
    ];

    renderWithTheme(<TransactionList transactions={numericTypeTransactions} />);
    
    expect(screen.getByText('Numeric Income')).toBeInTheDocument();
    expect(screen.getByText('Numeric Expense')).toBeInTheDocument();
    // Check for formatted amounts (they might be split across elements)
    expect(screen.getByText(/￥300/)).toBeInTheDocument();
    expect(screen.getByText(/￥150/)).toBeInTheDocument();
  });
});