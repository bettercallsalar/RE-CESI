import { extendTheme } from "@chakra-ui/react";

export const theme = extendTheme({
  fonts: {
    heading: "'Avenir Next', 'Gill Sans', sans-serif",
    body: "'Avenir Next', 'Trebuchet MS', sans-serif"
  },
  colors: {
    brand: {
      50: "#342B9A",
      100: "#342B9A",
      200: "#342B9A",
      300: "#342B9A",
      400: "#342B9A",
      500: "#342B9A",
      600: "#342B9A",
      700: "#342B9A",
      800: "#342B9A",
      900: "#342B9A"
    },
    ink: {
      50: "#2C2C2C",
      100: "#2C2C2C",
      200: "#2C2C2C",
      300: "#2C2C2C",
      400: "#2C2C2C",
      500: "#2C2C2C",
      600: "#2C2C2C",
      700: "#2C2C2C",
      800: "#2C2C2C",
      900: "#2C2C2C"
    },
    canvas: {
      50: "#FFFFFF",
      100: "#FFFFFF",
      200: "#D7D7D7"
    }
  },
  styles: {
    global: {
      body: {
        bg: "canvas.50",
        color: "ink.800"
      },
      "*:focus-visible": {
        outline: "3px solid #342B9A",
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
          color: "white",
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
            bg: "white"
          }
        },
        outline: {
          borderColor: "brand.500",
          color: "brand.500",
          bg: "white",
          _hover: {
            bg: "white"
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
            boxShadow: "0 0 0 1px #342B9A"
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
            boxShadow: "0 0 0 1px #342B9A"
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
          boxShadow: "0 0 0 1px #342B9A"
        }
      }
    }
  }
});
