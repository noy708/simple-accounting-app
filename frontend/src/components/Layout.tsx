import React from 'react';
import { AppBar, Toolbar, Typography, Container, Box } from '@mui/material';

interface LayoutProps {
  children: React.ReactNode;
  title?: string;
  showAppBar?: boolean;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | false;
}

export const Layout: React.FC<LayoutProps> = ({ 
  children, 
  title = 'シンプル会計アプリ',
  showAppBar = true,
  maxWidth = 'lg'
}) => {
  return (
    <Box sx={{ flexGrow: 1 }}>
      {showAppBar && (
        <AppBar position="static" elevation={2}>
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              {title}
            </Typography>
          </Toolbar>
        </AppBar>
      )}
      <Container maxWidth={maxWidth} sx={{ mt: showAppBar ? 4 : 0, mb: 4 }}>
        {children}
      </Container>
    </Box>
  );
};