import { extendTheme } from "@chakra-ui/react";

export const theme = extendTheme({
  fonts: {
    heading: "'Avenir Next', 'Gill Sans', sans-serif",
    body: "'Avenir Next', 'Trebuchet MS', sans-serif"
  },
  colors: {
    white: "var(--color-surface-base)",
    brand: {
      50: "var(--color-brand-500)",
      100: "var(--color-brand-500)",
      200: "var(--color-brand-500)",
      300: "var(--color-brand-500)",
      400: "var(--color-brand-500)",
      500: "var(--color-brand-500)",
      600: "var(--color-brand-500)",
      700: "var(--color-brand-500)",
      800: "var(--color-brand-500)",
      900: "var(--color-brand-500)"
    },
    ink: {
      50: "var(--color-ink-500)",
      100: "var(--color-ink-500)",
      200: "var(--color-ink-500)",
      300: "var(--color-ink-500)",
      400: "var(--color-ink-500)",
      500: "var(--color-ink-500)",
      600: "var(--color-ink-600)",
      700: "var(--color-ink-700)",
      800: "var(--color-ink-800)",
      900: "var(--color-ink-900)"
    },
    canvas: {
      50: "var(--color-canvas-50)",
      100: "var(--color-canvas-100)",
      200: "var(--color-canvas-200)",
      300: "var(--color-canvas-300)"
    },
    surface: {
      base: "var(--color-surface-base)",
      muted: "var(--color-surface-muted)",
      strong: "var(--color-surface-strong)",
      onAccent: "var(--color-on-accent)",
      onStrong: "var(--color-on-strong)",
      onCritical: "var(--color-on-critical)"
    }
  },
  styles: {
    global: {
      body: {
        bg: "canvas.50",
        color: "ink.800"
      },
      "*:focus-visible": {
        outline: "3px solid var(--color-focus-ring)",
        outlineOffset: "2px"
      }
    }
  },
  components: {
    Button: {
      baseStyle: {
        borderRadius: "2px",
        fontWeight: "600",
        minH: "44px",
        px: 5
      },
      defaultProps: {
        colorScheme: "brand"
      },
      variants: {
        solid: {
          bg: "brand.500",
          color: "surface.onAccent",
          _disabled: {
            bg: "canvas.200",
            color: "ink.800",
            opacity: 1
          },
          _hover: {
            bg: "brand.500",
            opacity: 0.92
          }
        },
        ghost: {
          color: "brand.500",
          _hover: {
            bg: "canvas.100"
          }
        },
        outline: {
          borderColor: "brand.500",
          color: "brand.500",
          bg: "white",
          _hover: {
            bg: "canvas.100"
          }
        }
      }
    },
    Card: {
      baseStyle: {
        container: {
          borderRadius: "8px",
          bg: "white",
          borderColor: "canvas.200"
        }
      }
    },
    Input: {
      baseStyle: {
        field: {
          bg: "white",
          borderColor: "canvas.200",
          borderRadius: "4px",
          color: "ink.800",
          minH: "48px",
          fontSize: "16px",
          _placeholder: {
            color: "ink.800",
            opacity: 0.7
          },
          _hover: {
            borderColor: "ink.800"
          },
          _focusVisible: {
            borderColor: "brand.500",
            boxShadow: "0 0 0 1px var(--color-brand-500)"
          }
        }
      }
    },
    Select: {
      baseStyle: {
        field: {
          bg: "white",
          borderColor: "canvas.200",
          borderRadius: "4px",
          color: "ink.800",
          minH: "48px",
          fontSize: "16px",
          _hover: {
            borderColor: "ink.800"
          },
          _focusVisible: {
            borderColor: "brand.500",
            boxShadow: "0 0 0 1px var(--color-brand-500)"
          }
        },
        icon: {
          color: "ink.800"
        }
      }
    },
    Textarea: {
      baseStyle: {
        borderRadius: "4px",
        bg: "white",
        borderColor: "canvas.200",
        color: "ink.800",
        fontSize: "16px",
        _placeholder: {
          color: "ink.800",
          opacity: 0.7
        },
        _hover: {
          borderColor: "ink.800"
        },
        _focusVisible: {
          borderColor: "brand.500",
          boxShadow: "0 0 0 1px var(--color-brand-500)"
        }
      }
    }
  }
});
