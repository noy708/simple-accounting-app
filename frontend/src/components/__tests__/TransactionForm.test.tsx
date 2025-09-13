import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { TransactionForm } from '../TransactionForm';
import { TransactionType } from '../../types/transaction';

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

// Mock the date picker components
jest.mock('@mui/x-date-pickers/DatePicker', () => ({
  DatePicker: ({ label, onChange, value, slotProps }: {
    label: string;
    onChange?: (date: Date | null) => void;
    value?: Date | null;
    slotProps?: { textField?: Record<string, unknown> };
  }) => (
    <input
      data-testid="date-picker"
      type="date"
      aria-label={label}
      value={value ? value.toISOString().split('T')[0] : ''}
      onChange={(e) => onChange && onChange(new Date(e.target.value))}
      {...slotProps?.textField}
    />
  ),
}));

jest.mock('@mui/x-date-pickers/LocalizationProvider', () => ({
  LocalizationProvider: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));

jest.mock('@mui/x-date-pickers/AdapterDateFns', () => ({
  AdapterDateFns: jest.fn(),
}));

describe('TransactionForm', () => {
  const mockOnSubmit = jest.fn();

  beforeEach(() => {
    mockOnSubmit.mockClear();
  });

  it('renders form title', () => {
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByText('新しい取引を追加')).toBeInTheDocument();
  });

  it('renders basic form elements', () => {
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText('金額')).toBeInTheDocument();
    expect(screen.getByLabelText('説明')).toBeInTheDocument();
    expect(screen.getAllByText('取引タイプ')).toHaveLength(2); // Label and legend
    expect(screen.getByTestId('date-picker')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /取引を追加/ })).toBeInTheDocument();
  });

  it('shows default expense type', () => {
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByText('支出')).toBeInTheDocument();
  });

  it('validates required fields on submit', async () => {
    const user = userEvent.setup();
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    const submitButton = screen.getByRole('button', { name: /取引を追加/ });
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText('金額は必須です')).toBeInTheDocument();
      expect(screen.getByText('説明は必須です')).toBeInTheDocument();
    });
    
    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('submits form with valid data', async () => {
    const user = userEvent.setup();
    mockOnSubmit.mockResolvedValue(undefined);
    
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    // Fill in the form
    await user.type(screen.getByLabelText('金額'), '1000');
    await user.type(screen.getByLabelText('説明'), 'Test transaction');
    
    // Submit form
    const submitButton = screen.getByRole('button', { name: /取引を追加/ });
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          amount: 1000,
          description: 'Test transaction',
          type: TransactionType.Expense, // Default type
        })
      );
    });
  });

  it('displays error message when submission fails', async () => {
    const user = userEvent.setup();
    const errorMessage = 'Network error';
    mockOnSubmit.mockRejectedValue(new Error(errorMessage));
    
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} />);
    
    // Fill and submit form
    await user.type(screen.getByLabelText('金額'), '500');
    await user.type(screen.getByLabelText('説明'), 'Test');
    
    const submitButton = screen.getByRole('button', { name: /取引を追加/ });
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it('shows loading state when loading prop is true', () => {
    renderWithTheme(<TransactionForm onSubmit={mockOnSubmit} loading={true} />);
    
    const submitButton = screen.getByRole('button', { name: /追加中/ });
    expect(submitButton).toBeDisabled();
    
    // Check that form fields are disabled
    expect(screen.getByLabelText('金額')).toBeDisabled();
    expect(screen.getByLabelText('説明')).toBeDisabled();
  });
});