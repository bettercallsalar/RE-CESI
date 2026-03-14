import { Box, CloseButton, HStack, Icon, Stack, Text } from "@chakra-ui/react";
import type { IconType } from "react-icons";
import { FiAlertCircle, FiAlertTriangle, FiCheckCircle, FiInfo } from "react-icons/fi";
import type { MessageTone } from "@/shared/ui/feedback/message.types";

interface MessageBannerProps {
  tone: MessageTone;
  message: string;
  title?: string;
  onClose?: () => void;
}

const toneStyles: Record<
  MessageTone,
  {
    bg: string;
    border: string;
    iconBg: string;
    iconColor: string;
    titleColor: string;
    textColor: string;
    icon: IconType;
  }
> = {
  success: {
    bg: "#FFFFFF",
    border: "#342B9A",
    iconBg: "#342B9A",
    iconColor: "#FFFFFF",
    titleColor: "#2C2C2C",
    textColor: "#2C2C2C",
    icon: FiCheckCircle,
  },
  error: {
    bg: "#342B9A",
    border: "#342B9A",
    iconBg: "#FFFFFF",
    iconColor: "#342B9A",
    titleColor: "#FFFFFF",
    textColor: "#FFFFFF",
    icon: FiAlertCircle,
  },
  warning: {
    bg: "#FFFFFF",
    border: "#2C2C2C",
    iconBg: "#2C2C2C",
    iconColor: "#FFFFFF",
    titleColor: "#2C2C2C",
    textColor: "#2C2C2C",
    icon: FiAlertTriangle,
  },
  info: {
    bg: "#FFFFFF",
    border: "#2C2C2C",
    iconBg: "#2C2C2C",
    iconColor: "#FFFFFF",
    titleColor: "#2C2C2C",
    textColor: "#2C2C2C",
    icon: FiInfo,
  },
};

export function MessageBanner({
  tone,
  message,
  title,
  onClose,
}: MessageBannerProps) {
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
      width="100%">
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
        w="36px">
        <Icon as={style.icon} boxSize={5} />
      </Box>

      <Stack flex="1" spacing={0.5}>
        {title ? (
          <Text
            color={style.titleColor}
            fontSize={{ base: "15px", md: "16px" }}
            fontWeight="700">
            {title}
          </Text>
        ) : null}
        <Text
          color={style.textColor}
          fontSize={{ base: "15px", md: "16px" }}
          lineHeight="1.45">
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
