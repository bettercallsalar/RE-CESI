import { ChevronDownIcon, ChevronUpIcon, HamburgerIcon } from "@chakra-ui/icons";
import {
  Box,
  Button,
  Collapse,
  Flex,
  HStack,
  IconButton,
  Link,
  Menu,
  MenuButton,
  MenuItem,
  MenuList,
  Show,
  Stack,
  Text,
  useDisclosure
} from "@chakra-ui/react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { GovernmentBrand } from "@/shared/ui/site/GovernmentBrand";

interface SiteHeaderProps {
  variant?: "public" | "authenticated";
}

type NavGlyphType = "chart" | "grid" | "calendar" | "user";

interface NavLinkItem {
  label: string;
  glyph: NavGlyphType;
  href: string;
}

function NavGlyph({ type }: { type: NavGlyphType }) {
  if (type === "chart") {
    return (
      <Box display="inline-flex" gap="3px" h="18px" w="18px">
        <Box alignSelf="end" bg="brand.500" h="8px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="12px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="16px" w="3px" />
      </Box>
    );
  }

  if (type === "grid") {
    return (
      <Box border="1px solid" borderColor="brand.500" display="grid" gap="2px" gridTemplateColumns="repeat(2, 1fr)" h="18px" p="2px" w="18px">
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
      </Box>
    );
  }

  if (type === "calendar") {
    return (
      <Box border="1px solid" borderColor="brand.500" borderRadius="6px" h="18px" position="relative" w="18px">
        <Box bg="brand.500" h="4px" left="0" position="absolute" top="0" w="100%" />
        <Box bg="brand.500" borderRadius="999px" h="3px" left="4px" position="absolute" top="-1px" w="2px" />
        <Box bg="brand.500" borderRadius="999px" h="3px" position="absolute" right="4px" top="-1px" w="2px" />
        <Box bg="brand.500" h="2px" left="4px" position="absolute" top="8px" w="10px" />
        <Box bg="brand.500" h="2px" left="4px" position="absolute" top="12px" w="7px" />
      </Box>
    );
  }

  return (
    <Box border="1px solid" borderColor="brand.500" borderRadius="999px" h="18px" position="relative" w="18px">
      <Box bg="brand.500" borderRadius="999px" h="4px" left="6px" position="absolute" top="3px" w="4px" />
      <Box borderColor="brand.500" borderRadius="999px 999px 6px 6px" borderStyle="solid" borderWidth="1px 1px 0" h="6px" left="4px" position="absolute" top="8px" w="9px" />
    </Box>
  );
}

function HeaderLink({ label, glyph, href }: NavLinkItem) {
  return (
    <Link _hover={{ textDecoration: "none", color: "ink.800" }} color="brand.500" href={href}>
      <HStack minH="44px" px={1} spacing={2.5}>
        <Text fontSize={{ base: "15px", xl: "16px" }} fontWeight="600">
          {label}
        </Text>
        <NavGlyph type={glyph} />
      </HStack>
    </Link>
  );
}

function getUserLabel(firstName?: string, username?: string) {
  return firstName?.trim() || username?.trim() || "Mon compte";
}

function UserMenu({ label }: { label: string }) {
  const items: NavLinkItem[] = [
    { label: "Mon compte", glyph: "user", href: "/mon-compte" },
    { label: "Mes articles", glyph: "grid", href: "/mes-articles" },
    { label: "Mes events", glyph: "calendar", href: "/mes-events" }
  ];

  return (
    <Menu>
      <MenuButton
        as={Button}
        color="brand.500"
        fontSize={{ base: "15px", xl: "16px" }}
        fontWeight="600"
        px={1}
        rightIcon={<ChevronDownIcon boxSize={5} />}
        variant="ghost"
        _active={{ bg: "transparent" }}
        _hover={{ bg: "transparent", color: "ink.800" }}
      >
        <HStack spacing={2.5}>
          <Text>{label}</Text>
          <NavGlyph type="user" />
        </HStack>
      </MenuButton>
      <MenuList bg="white" borderColor="canvas.200" boxShadow="xl" minW="220px" p={2}>
        {items.map((item) => (
          <MenuItem
            as="a"
            bg="white"
            borderRadius="10px"
            color="brand.500"
            fontWeight="600"
            href={item.href}
            key={item.label}
            _focus={{ bg: "canvas.100", color: "ink.800" }}
            _hover={{ bg: "canvas.100", color: "ink.800" }}
          >
            <HStack spacing={3}>
              <NavGlyph type={item.glyph} />
              <Text>{item.label}</Text>
            </HStack>
          </MenuItem>
        ))}
      </MenuList>
    </Menu>
  );
}

function MobileNavigation({
  variant,
  userLabel
}: {
  variant: "public" | "authenticated";
  userLabel: string;
}) {
  const userDisclosure = useDisclosure();
  const publicItems: NavLinkItem[] = [
    { label: "Statistiques", glyph: "chart", href: "/" },
    { label: "Articles", glyph: "grid", href: "/articles" },
    { label: "Events", glyph: "calendar", href: "/events" },
    { label: "Mon compte", glyph: "user", href: "/login" }
  ];

  const userItems: NavLinkItem[] = [
    { label: "Mon compte", glyph: "user", href: "/mon-compte" },
    { label: "Mes articles", glyph: "grid", href: "/mes-articles" },
    { label: "Mes events", glyph: "calendar", href: "/mes-events" }
  ];
  const topLevelItems: NavLinkItem[] = variant === "authenticated"
    ? [
        { label: "Articles", glyph: "grid", href: "/articles" },
        { label: "Events", glyph: "calendar", href: "/events" }
      ]
    : publicItems;

  return (
    <Stack align={{ base: "stretch", sm: "flex-end" }} bg="white" border="1px solid" borderColor="canvas.200" mt={3} pb={3} pt={3} px={{ base: 3, sm: 4 }} spacing={2}>
      {topLevelItems.map((item) => (
        <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
      ))}

      {variant === "authenticated" ? (
        <Box w="100%">
          <Button
            color="brand.500"
            justifyContent="space-between"
            onClick={userDisclosure.onToggle}
            rightIcon={userDisclosure.isOpen ? <ChevronUpIcon boxSize={5} /> : <ChevronDownIcon boxSize={5} />}
            variant="ghost"
            w="100%"
            _hover={{ bg: "canvas.100" }}
          >
            <HStack spacing={2.5}>
              <Text fontSize="15px" fontWeight="600">
                {userLabel}
              </Text>
              <NavGlyph type="user" />
            </HStack>
          </Button>
          <Collapse in={userDisclosure.isOpen}>
            <Stack pl={4} pt={2} spacing={1}>
              {userItems.map((item) => (
                <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
              ))}
            </Stack>
          </Collapse>
        </Box>
      ) : null}
    </Stack>
  );
}

export function SiteHeader({ variant = "public" }: SiteHeaderProps) {
  const { isOpen, onToggle } = useDisclosure();
  const { user } = useAuth();
  const userLabel = getUserLabel(user?.firstName, user?.username);
  const publicItems: NavLinkItem[] = [
    { label: "Statistiques", glyph: "chart", href: "/" },
    { label: "Articles", glyph: "grid", href: "/articles" },
    { label: "Events", glyph: "calendar", href: "/events" },
    { label: "Mon compte", glyph: "user", href: "/login" }
  ];
  const authenticatedItems: NavLinkItem[] = [
    { label: "Articles", glyph: "grid", href: "/articles" },
    { label: "Events", glyph: "calendar", href: "/events" }
  ];

  return (
    <Box pb={{ base: 7, md: 8 }} pt={{ base: 5, md: 7 }}>
      <Flex align={{ base: "start", md: "center" }} gap={{ base: 3, md: 5 }} justify="space-between">
        <GovernmentBrand />

        <Stack align="center" flex="1" px={{ base: 2, sm: 6, md: 10 }} spacing={1}>
          <Text color="brand.500" fontSize={{ base: "18px", sm: "24px", md: "30px", xl: "34px" }} fontWeight="700" lineHeight="1.1" textAlign="center">
            (RE) Sources Relationnelles
          </Text>
        </Stack>

        <Box minW={{ base: "48px", lg: "420px" }}>
          <Show above="lg">
            <HStack justify="flex-end" spacing={4}>
              {(variant === "authenticated" ? authenticatedItems : publicItems).map((item) => (
                <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
              ))}
              {variant === "authenticated" ? <UserMenu label={userLabel} /> : null}
            </HStack>
          </Show>
          <Show below="lg">
            <Flex justify="flex-end">
              <IconButton
                aria-label="Ouvrir le menu"
                boxSize={{ base: "44px", md: "48px" }}
                color="brand.500"
                icon={<HamburgerIcon boxSize={7} />}
                minW="auto"
                onClick={onToggle}
                variant="ghost"
              />
            </Flex>
          </Show>
        </Box>
      </Flex>

      <Show below="lg">
        <Collapse animateOpacity in={isOpen}>
          <MobileNavigation userLabel={userLabel} variant={variant} />
        </Collapse>
      </Show>
    </Box>
  );
}
