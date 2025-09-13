import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { BalanceDisplay } from '../BalanceDisplay';

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

describe('BalanceDisplay', () => {
  it('renders balance display with title', () => {
    renderWithTheme(<BalanceDisplay balance={1000} />);
    
    expect(screen.getByText('現在の残高')).toBeInTheDocument();
  });

  it('displays positive balance correctly', () => {
    renderWithTheme(<BalanceDisplay balance={1500.50} />);
    
    expect(screen.getByText(/￥1,501/)).toBeInTheDocument(); // Rounded to nearest yen
    expect(screen.getByText('黒字です')).toBeInTheDocument();
  });

  it('displays negative balance correctly', () => {
    renderWithTheme(<BalanceDisplay balance={-500} />);
    
    expect(screen.getByText(/-￥500/)).toBeInTheDocument();
    expect(screen.getByText('赤字です')).toBeInTheDocument();
  });

  it('displays zero balance correctly', () => {
    renderWithTheme(<BalanceDisplay balance={0} />);
    
    expect(screen.getByText(/￥0/)).toBeInTheDocument();
    expect(screen.getByText('収支は均衡しています')).toBeInTheDocument();
  });

  it('shows loading state when loading prop is true', () => {
    renderWithTheme(<BalanceDisplay balance={1000} loading={true} />);
    
    expect(screen.getByText('読み込み中...')).toBeInTheDocument();
    expect(screen.queryByText('¥1,000')).not.toBeInTheDocument();
    expect(screen.queryByText('黒字です')).not.toBeInTheDocument();
  });

  it('renders account balance icon', () => {
    renderWithTheme(<BalanceDisplay balance={1000} />);
    
    // Check if the AccountBalance icon is rendered (it should be in the DOM)
    const icon = document.querySelector('[data-testid="AccountBalanceIcon"]');
    expect(icon || screen.getByText('現在の残高').previousElementSibling).toBeTruthy();
  });

  it('formats large numbers correctly', () => {
    renderWithTheme(<BalanceDisplay balance={1234567} />);
    
    expect(screen.getByText(/￥1,234,567/)).toBeInTheDocument();
  });

  it('formats decimal numbers correctly', () => {
    renderWithTheme(<BalanceDisplay balance={999.99} />);
    
    expect(screen.getByText(/￥1,000/)).toBeInTheDocument(); // Rounded to nearest yen
  });
});