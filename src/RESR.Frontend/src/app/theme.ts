import { extendTheme } from "@chakra-ui/react";

export const theme = extendTheme({
  fonts: {
    heading: "'Avenir Next', 'Gill Sans', sans-serif",
    body: "'Avenir Next', 'Trebuchet MS', sans-serif"
  },
  colors: {
    brand: {
      50: "#f4f4ff",
      100: "#dcdafc",
      200: "#bcb7f8",
      300: "#9992f1",
      400: "#726ce2",
      500: "#3d33a8",
      600: "#2f2784",
      700: "#221d60",
      800: "#14123d",
      900: "#08071b"
    },
    ink: {
      50: "#f6f6f8",
      100: "#dfdfe4",
      200: "#c2c4ca",
      300: "#9d9fa8",
      400: "#717480",
      500: "#4b4f5d",
      600: "#333745",
      700: "#232733",
      800: "#151821",
      900: "#090b10"
    },
    canvas: {
      50: "#f7f6f4",
      100: "#ece9e4",
      200: "#dddad3"
    }
  },
  styles: {
    global: {
      body: {
        bg: "canvas.50",
        color: "ink.900"
      },
      "*:focus-visible": {
        outline: "3px solid #726ce2",
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
            bg: "brand.200",
            color: "white",
            opacity: 1
          },
          _hover: {
            bg: "brand.600"
          }
        },
        ghost: {
          color: "brand.500"
        },
        outline: {
          borderColor: "brand.500",
          color: "brand.500"
        }
      }
    },
    Card: {
      baseStyle: {
        container: {
          borderRadius: "8px"
        }
      }
    },
    Input: {
      baseStyle: {
        field: {
          bg: "white",
          borderColor: "blackAlpha.300",
          borderRadius: "4px",
          color: "ink.900",
          minH: "48px",
          fontSize: "16px",
          _placeholder: {
            color: "ink.400"
          },
          _hover: {
            borderColor: "ink.400"
          }
        }
      }
    },
    Select: {
      baseStyle: {
        field: {
          bg: "white",
          borderColor: "blackAlpha.300",
          borderRadius: "4px",
          color: "ink.900",
          minH: "48px",
          fontSize: "16px",
          _hover: {
            borderColor: "ink.400"
          }
        },
        icon: {
          color: "ink.700"
        }
      }
    },
    Textarea: {
      baseStyle: {
        borderRadius: "4px",
        bg: "white",
        borderColor: "blackAlpha.300",
        color: "ink.900",
        fontSize: "16px",
        _placeholder: {
          color: "ink.400"
        },
        _hover: {
          borderColor: "ink.400"
        }
      }
    }
  }
});
