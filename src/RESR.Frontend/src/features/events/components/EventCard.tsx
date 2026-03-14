import { Badge, Box, Button, Card, CardBody, HStack, Heading, Image, Stack, Text } from "@chakra-ui/react";
import { formatEventDateRange, formatEventPublishedDate, getEventExcerpt, getPrimaryEventImage } from "@/features/events/lib/eventDates";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import type { Event } from "@/shared/types/event";

export interface EventCardAction {
  label: string;
  href?: string;
  onClick?: () => void;
  variant?: "solid" | "outline";
  tone?: "default" | "danger" | "dangerSoft";
  isDisabled?: boolean;
}

interface EventCardProps {
  event: Event;
  categoryName?: string;
  compact?: boolean;
  href?: string;
  ctaLabel?: string;
  actions?: EventCardAction[];
  showStatusBadges?: boolean;
}

function getAuthorLabel(event: Event) {
  const username = event.author?.username?.trim();
  const firstName = event.author?.firstName?.trim();

  return username || firstName || `Utilisateur #${event.idUser}`;
}

export function EventCard({
  event,
  categoryName,
  compact = false,
  href = `/events/${event.idResource}`,
  ctaLabel = "Voir l'evenement",
  actions,
  showStatusBadges = false
}: EventCardProps) {
  const firstImage = getPrimaryEventImage(event);
  const coverImageSrc = firstImage ? getResourceFileUrl(firstImage.path) : "/article-placeholder.svg";
  const visibilityLabel = event.visibility === "PUBLIC" ? "Public" : "Prive";
  const approvalLabel = event.isApproved ? "Valide" : "En attente";
  const deletedLabel = event.deletedAt ? `Supprime le ${formatEventPublishedDate(event.deletedAt)}` : null;
  const isLinkedCard = !actions?.length;

  return (
    <Card
      as={isLinkedCard ? "a" : undefined}
      href={isLinkedCard ? href : undefined}
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      display="flex"
      flexDirection="column"
      h="100%"
      rounded={{ base: "12px", md: "16px" }}
      shadow="sm"
      transition="border-color 0.2s ease, transform 0.2s ease, box-shadow 0.2s ease"
      _hover={{ borderColor: "brand.500", boxShadow: "md", transform: "translateY(-2px)", textDecoration: "none" }}
    >
      <Box bg="canvas.200" h={compact ? "180px" : "220px"} overflow="hidden">
        <Image alt={event.title} h="100%" objectFit="cover" src={coverImageSrc} w="100%" />
      </Box>
      <CardBody display="flex" flex="1" flexDirection="column" p={{ base: 5, md: 6 }}>
        <Stack flex="1" spacing={compact ? 3 : 4}>
          <HStack flexWrap="wrap" spacing={3}>
            <Badge bg="canvas.200" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
              {formatEventPublishedDate(event.createdAt)}
            </Badge>
            {categoryName ? (
              <Badge bg="white" border="1px solid" borderColor="brand.500" color="brand.500" fontSize="12px" px={2.5} py={1} rounded="full">
                {categoryName}
              </Badge>
            ) : null}
            {event.department ? (
              <Badge bg="white" border="1px solid" borderColor="canvas.300" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
                {event.department.code} - {event.department.name}
              </Badge>
            ) : null}
            {showStatusBadges ? (
              <Badge bg={event.visibility === "PUBLIC" ? "brand.500" : "ink.800"} color="white" fontSize="12px" px={2.5} py={1} rounded="full">
                {visibilityLabel}
              </Badge>
            ) : null}
            {showStatusBadges ? (
              <Badge
                bg={event.isApproved ? "white" : "canvas.200"}
                border="1px solid"
                borderColor={event.isApproved ? "brand.500" : "canvas.300"}
                color={event.isApproved ? "brand.500" : "ink.800"}
                fontSize="12px"
                px={2.5}
                py={1}
                rounded="full"
              >
                {approvalLabel}
              </Badge>
            ) : null}
            {showStatusBadges && deletedLabel ? (
              <Badge bg="red.500" color="white" fontSize="12px" px={2.5} py={1} rounded="full">
                {deletedLabel}
              </Badge>
            ) : null}
          </HStack>

          <Heading color="ink.800" fontSize={compact ? { base: "18px", md: "20px" } : { base: "20px", md: "24px" }} lineHeight="1.25">
            {event.title}
          </Heading>

          {event.subtitle ? (
            <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
              {event.subtitle}
            </Text>
          ) : null}

          <Text color="ink.500" fontSize={{ base: "13px", md: "14px" }} fontWeight="600">
            Par {getAuthorLabel(event)}
          </Text>

          <Text color="ink.800" fontSize={{ base: "14px", md: "15px" }} fontWeight="600">
            {formatEventDateRange(event.startDate, event.endDate)}
          </Text>

          {event.address ? (
            <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
              {event.address}
            </Text>
          ) : null}

          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.65" noOfLines={compact ? 4 : 5}>
            {getEventExcerpt(event)}
          </Text>

          {actions?.length ? (
            <HStack align="stretch" mt="auto" pt={3} spacing={3} wrap="nowrap">
              {actions.map((action) => (
                <Button
                  key={`${action.label}-${action.href ?? "action"}`}
                  bg={action.tone === "danger" ? "red.500" : undefined}
                  borderColor={action.tone === "dangerSoft" ? "red.200" : undefined}
                  color={action.tone === "danger" ? "white" : action.tone === "dangerSoft" ? "red.300" : undefined}
                  flexShrink={0}
                  isDisabled={action.isDisabled}
                  onClick={() => {
                    if (action.onClick) {
                      action.onClick();
                      return;
                    }

                    if (action.href) {
                      navigateTo(action.href);
                    }
                  }}
                  size="sm"
                  variant={action.variant ?? "outline"}
                  _hover={
                    action.tone === "danger"
                      ? { bg: "red.600" }
                      : action.tone === "dangerSoft"
                        ? { bg: "red.50" }
                        : undefined
                  }
                  _disabled={
                    action.tone === "dangerSoft"
                      ? { bg: "red.100", borderColor: "red.200", color: "white", opacity: 1, cursor: "not-allowed" }
                      : undefined
                  }
                >
                  {action.label}
                </Button>
              ))}
            </HStack>
          ) : (
            <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700" mt="auto">
              {ctaLabel}
            </Text>
          )}
        </Stack>
      </CardBody>
    </Card>
  );
}
