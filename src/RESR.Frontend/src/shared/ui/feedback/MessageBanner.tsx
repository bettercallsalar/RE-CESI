import { CheckCircleIcon, InfoIcon, WarningIcon, WarningTwoIcon } from "@chakra-ui/icons";
import { Box, CloseButton, HStack, Icon, Stack, Text } from "@chakra-ui/react";
import type { ComponentType } from "react";
import type { MessageTone } from "@/shared/ui/feedback/message.types";

interface MessageBannerProps {
  tone: MessageTone;
  message: string;
  title?: string;
  onClose?: () => void;
}

const toneStyles: Record<MessageTone, {
  bg: string;
  border: string;
  iconBg: string;
  iconColor: string;
  titleColor: string;
  textColor: string;
  icon: ComponentType;
}> = {
  success: {
    bg: "#eaf7ef",
    border: "#9fd8b3",
    iconBg: "#2f855a",
    iconColor: "white",
    titleColor: "#22543d",
    textColor: "#22543d",
    icon: CheckCircleIcon
  },
  error: {
    bg: "#fdeeee",
    border: "#f1b5b5",
    iconBg: "#c53030",
    iconColor: "white",
    titleColor: "#742a2a",
    textColor: "#742a2a",
    icon: WarningIcon
  },
  warning: {
    bg: "#fff6e5",
    border: "#f2cf8b",
    iconBg: "#b7791f",
    iconColor: "white",
    titleColor: "#744210",
    textColor: "#744210",
    icon: WarningTwoIcon
  },
  info: {
    bg: "#edf4ff",
    border: "#a6c8ff",
    iconBg: "#2b6cb0",
    iconColor: "white",
    titleColor: "#1a365d",
    textColor: "#1a365d",
    icon: InfoIcon
  }
};

export function MessageBanner({ tone, message, title, onClose }: MessageBannerProps) {
  const style = toneStyles[tone];

  return (
    <HStack
      align="start"
      bg={style.bg}
      border="1px solid"
      borderColor={style.border}
      borderRadius="12px"
      px={{ base: 4, md: 5 }}
      py={{ base: 3.5, md: 4 }}
      spacing={4}
      width="100%"
    >
      <Box
        alignItems="center"
        bg={style.iconBg}
        borderRadius="999px"
        color={style.iconColor}
        display="inline-flex"
        flexShrink={0}
        h="36px"
        justifyContent="center"
        mt="2px"
        w="36px"
      >
        <Icon as={style.icon} boxSize={4.5} />
      </Box>

      <Stack flex="1" spacing={0.5}>
        {title ? (
          <Text color={style.titleColor} fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
            {title}
          </Text>
        ) : null}
        <Text color={style.textColor} fontSize={{ base: "15px", md: "16px" }} lineHeight="1.45">
          {message}
        </Text>
      </Stack>

      {onClose ? (
        <CloseButton
          alignSelf="start"
          color={style.textColor}
          onClick={onClose}
          size="md"
        />
      ) : null}
    </HStack>
  );
}
