import { Box, SimpleGrid, Text } from "@chakra-ui/react";
import { EventCard } from "@/features/events/components/EventCard";
import type { EventCardAction } from "@/features/events/components/EventCard";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";

interface EventsGridProps {
  events: Event[];
  categories: Category[];
  emptyLabel: string;
  compact?: boolean;
  resolveHref?: (event: Event) => string;
  ctaLabel?: string;
  resolveActions?: (event: Event) => EventCardAction[];
  showStatusBadges?: boolean;
}

export function EventsGrid({
  events,
  categories,
  emptyLabel,
  compact = false,
  resolveHref,
  ctaLabel,
  resolveActions,
  showStatusBadges = false
}: EventsGridProps) {
  function getCategoryName(idCategory: number) {
    return categories.find((category) => category.idCategory === idCategory)?.name;
  }

  if (events.length === 0) {
    return (
      <Box bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" px={{ base: 5, md: 6 }} py={{ base: 6, md: 7 }}>
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {emptyLabel}
        </Text>
      </Box>
    );
  }

  return (
    <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
      {events.map((event) => (
        <EventCard
          actions={resolveActions?.(event)}
          categoryName={getCategoryName(event.idCategory)}
          compact={compact}
          ctaLabel={ctaLabel}
          event={event}
          href={resolveHref?.(event)}
          key={event.idResource}
          showStatusBadges={showStatusBadges}
        />
      ))}
    </SimpleGrid>
  );
}
