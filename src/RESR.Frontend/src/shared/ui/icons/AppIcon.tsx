import { Box, Icon, type BoxProps } from "@chakra-ui/react";
import type { IconType } from "react-icons";

type AppIconSize = "sm" | "md" | "lg" | "xl";

interface AppIconProps extends Omit<BoxProps, "color"> {
  icon: IconType;
  size?: AppIconSize;
  color?: string;
  strokeWidth?: number;
  iconSize?: string;
}

const metrics: Record<AppIconSize, { container: string; icon: string }> = {
  sm: { container: "28px", icon: "18px" },
  md: { container: "32px", icon: "20px" },
  lg: { container: "36px", icon: "22px" },
  xl: { container: "40px", icon: "24px" }
};

export function AppIcon({
  icon,
  size = "md",
  color = "currentColor",
  strokeWidth = 1.9,
  iconSize,
  borderRadius = "10px",
  ...boxProps
}: AppIconProps) {
  const metric = metrics[size];

  return (
    <Box
      alignItems="center"
      borderRadius={borderRadius}
      color={color}
      display="inline-flex"
      flexShrink={0}
      h={metric.container}
      justifyContent="center"
      minW={metric.container}
      w={metric.container}
      {...boxProps}
    >
      <Icon as={icon} boxSize={iconSize ?? metric.icon} color={color} strokeWidth={strokeWidth} />
    </Box>
  );
}
