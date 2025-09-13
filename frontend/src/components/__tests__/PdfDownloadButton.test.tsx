import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { PdfDownloadButton } from '../PdfDownloadButton';
import { transactionService } from '@/services/api';

// Mock the API service
jest.mock('@/services/api', () => ({
  transactionService: {
    downloadPdf: jest.fn(),
  },
}));

// Mock URL.createObjectURL and URL.revokeObjectURL
global.URL.createObjectURL = jest.fn(() => 'mock-url');
global.URL.revokeObjectURL = jest.fn();

describe('PdfDownloadButton', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders the PDF download button', () => {
    render(<PdfDownloadButton />);
    
    const button = screen.getByRole('button', { name: /PDF生成/i });
    expect(button).toBeInTheDocument();
  });

  it('shows loading state when downloading', async () => {
    const mockBlob = new Blob(['test'], { type: 'application/pdf' });
    (transactionService.downloadPdf as jest.Mock).mockResolvedValue(mockBlob);

    render(<PdfDownloadButton />);
    
    const button = screen.getByRole('button', { name: /PDF生成/i });
    fireEvent.click(button);

    // Should show loading state
    expect(screen.getByText('PDF生成中...')).toBeInTheDocument();
    expect(button).toBeDisabled();

    // Wait for the download to complete
    await waitFor(() => {
      expect(screen.getByText('PDF生成')).toBeInTheDocument();
    });
  });

  it('handles download success', async () => {
    const mockBlob = new Blob(['test'], { type: 'application/pdf' });
    (transactionService.downloadPdf as jest.Mock).mockResolvedValue(mockBlob);

    // Mock document.createElement and appendChild
    const mockLink = {
      href: '',
      download: '',
      click: jest.fn(),
    } as unknown as HTMLAnchorElement;
    jest.spyOn(document, 'createElement').mockReturnValue(mockLink);
    jest.spyOn(document.body, 'appendChild').mockImplementation(() => mockLink);
    jest.spyOn(document.body, 'removeChild').mockImplementation(() => mockLink);

    render(<PdfDownloadButton />);
    
    const button = screen.getByRole('button', { name: /PDF生成/i });
    fireEvent.click(button);

    await waitFor(() => {
      expect(transactionService.downloadPdf).toHaveBeenCalled();
      expect(mockLink.click).toHaveBeenCalled();
    });

    // Should show success message
    await waitFor(() => {
      expect(screen.getByText('PDFのダウンロードが完了しました')).toBeInTheDocument();
    });
  });

  it('handles download error', async () => {
    const errorMessage = 'Download failed';
    (transactionService.downloadPdf as jest.Mock).mockRejectedValue(new Error(errorMessage));

    render(<PdfDownloadButton />);
    
    const button = screen.getByRole('button', { name: /PDF生成/i });
    fireEvent.click(button);

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it('applies custom props correctly', () => {
    render(
      <PdfDownloadButton 
        variant="outlined" 
        size="small" 
        fullWidth 
      />
    );
    
    const button = screen.getByRole('button', { name: /PDF生成/i });
    expect(button).toHaveClass('MuiButton-outlined');
    expect(button).toHaveClass('MuiButton-sizeSmall');
  });
});