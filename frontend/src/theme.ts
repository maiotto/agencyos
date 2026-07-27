import { createTheme } from '@mui/material/styles'

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#0F4C5C',
      dark: '#0A3440',
      light: '#1A6B7C',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#E36414',
      contrastText: '#FFFFFF',
    },
    background: {
      default: '#E8EEF1',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#132226',
      secondary: '#3F5258',
    },
    success: {
      main: '#2A6F4E',
    },
    warning: {
      main: '#B86E00',
    },
    error: {
      main: '#9B2C2C',
    },
    divider: 'rgba(15, 76, 92, 0.12)',
  },
  typography: {
    fontFamily: '"IBM Plex Sans", "Segoe UI", sans-serif',
    h1: {
      fontFamily: '"IBM Plex Serif", Georgia, serif',
      fontWeight: 600,
    },
    h2: {
      fontFamily: '"IBM Plex Serif", Georgia, serif',
      fontWeight: 600,
    },
    h3: {
      fontFamily: '"IBM Plex Serif", Georgia, serif',
      fontWeight: 600,
    },
    h4: {
      fontFamily: '"IBM Plex Serif", Georgia, serif',
      fontWeight: 600,
    },
    h5: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 600,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 10,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        },
      },
    },
    MuiPaper: {
      defaultProps: {
        elevation: 0,
      },
      styleOverrides: {
        root: {
          border: '1px solid rgba(15, 76, 92, 0.12)',
        },
      },
    },
  },
})

/** Seeded default Company id (AgencyOSCompanies.DefaultCompanyId, US-402). Used as a fallback when
 * no Company has been selected in the current browser session. */
export const DEFAULT_COMPANY_ID = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa'
