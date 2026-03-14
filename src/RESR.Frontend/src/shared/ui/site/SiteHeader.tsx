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
import type { IconType } from "react-icons";
import { FiCalendar, FiChevronDown, FiChevronUp, FiFileText, FiHome, FiMenu, FiShield, FiUser } from "react-icons/fi";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { AppIcon } from "@/shared/ui/icons/AppIcon";
import { GovernmentBrand } from "@/shared/ui/site/GovernmentBrand";

interface SiteHeaderProps {
  variant?: "public" | "authenticated";
}

interface NavLinkItem {
  label: string;
  icon: IconType;
  href: string;
}

function HeaderLink({ label, icon, href }: NavLinkItem) {
  return (
    <Link _hover={{ color: "ink.800", textDecoration: "none" }} color="brand.500" display="inline-flex" href={href} verticalAlign="middle">
      <HStack align="center" minH="44px" px={1} spacing={1.5}>
        <AppIcon color="brand.500" icon={icon} size="lg" />
        <Text fontSize={{ base: "15px", xl: "16px" }} fontWeight="600">
          {label}
        </Text>
      </HStack>
    </Link>
  );
}

function MobileHeaderLink({ label, icon, href }: NavLinkItem) {
  return (
    <Link
      _hover={{ bg: "canvas.100", color: "ink.800", textDecoration: "none" }}
      borderRadius="12px"
      color="brand.500"
      display="block"
      href={href}
      px={3}
      py={2.5}
      w="100%"
    >
      <HStack align="center" minH="24px" spacing={2}>
        <AppIcon color="brand.500" icon={icon} size="lg" />
        <Text fontSize="15px" fontWeight="600" lineHeight="1.3">
          {label}
        </Text>
      </HStack>
    </Link>
  );
}

function getUserLabel(firstName?: string, username?: string) {
  return firstName?.trim() || username?.trim() || "Mon compte";
}

function UserMenu({ canAccessAdminDashboard, label }: { canAccessAdminDashboard: boolean; label: string }) {
  const items: NavLinkItem[] = canAccessAdminDashboard
    ? [
        { label: "Administration", icon: FiShield, href: "/admin" },
        { label: "Mon compte", icon: FiUser, href: "/mon-compte" },
        { label: "Mes articles", icon: FiFileText, href: "/mes-articles" },
        { label: "Mes events", icon: FiCalendar, href: "/mes-events" }
      ]
    : [
        { label: "Mon compte", icon: FiUser, href: "/mon-compte" },
        { label: "Mes articles", icon: FiFileText, href: "/mes-articles" },
        { label: "Mes events", icon: FiCalendar, href: "/mes-events" }
      ];

  return (
    <Menu>
      <MenuButton
        as={Button}
        _active={{ bg: "transparent" }}
        _hover={{ bg: "transparent", color: "ink.800" }}
        alignItems="center"
        color="brand.500"
        display="inline-flex"
        fontSize={{ base: "15px", xl: "16px" }}
        fontWeight="600"
        h="44px"
        justifyContent="center"
        lineHeight="1"
        minH="44px"
        minW="auto"
        px={1}
        variant="unstyled"
        verticalAlign="middle"
      >
        <HStack align="center" color="brand.500" h="44px" spacing={1.5}>
          <AppIcon color="brand.500" icon={FiUser} size="lg" />
          <Text color="brand.500">{label}</Text>
          <AppIcon color="brand.500" icon={FiChevronDown} size="sm" strokeWidth={2} />
        </HStack>
      </MenuButton>
      <MenuList bg="white" borderColor="canvas.200" boxShadow="xl" minW="220px" p={2}>
        {items.map((item) => (
          <MenuItem
            as="a"
            _focus={{ bg: "canvas.100", color: "ink.800" }}
            _hover={{ bg: "canvas.100", color: "ink.800" }}
            alignItems="center"
            bg="white"
            borderRadius="10px"
            color="brand.500"
            fontSize="16px"
            fontWeight="600"
            href={item.href}
            key={item.label}
            minH="52px"
          >
            <HStack align="center" spacing={2}>
              <AppIcon color="brand.500" icon={item.icon} size="lg" />
              <Text>{item.label}</Text>
            </HStack>
          </MenuItem>
        ))}
      </MenuList>
    </Menu>
  );
}

function MobileNavigation({
  canAccessAdminDashboard,
  variant,
  userLabel
}: {
  canAccessAdminDashboard: boolean;
  variant: "public" | "authenticated";
  userLabel: string;
}) {
  const userDisclosure = useDisclosure();
  const publicItems: NavLinkItem[] = [
    { label: "Accueil", icon: FiHome, href: "/" },
    { label: "Articles", icon: FiFileText, href: "/articles" },
    { label: "Events", icon: FiCalendar, href: "/events" },
    { label: "Mon compte", icon: FiUser, href: "/login" }
  ];
  const userItems: NavLinkItem[] = canAccessAdminDashboard
    ? [
        { label: "Administration", icon: FiShield, href: "/admin" },
        { label: "Mon compte", icon: FiUser, href: "/mon-compte" },
        { label: "Mes articles", icon: FiFileText, href: "/mes-articles" },
        { label: "Mes events", icon: FiCalendar, href: "/mes-events" }
      ]
    : [
        { label: "Mon compte", icon: FiUser, href: "/mon-compte" },
        { label: "Mes articles", icon: FiFileText, href: "/mes-articles" },
        { label: "Mes events", icon: FiCalendar, href: "/mes-events" }
      ];
  const authenticatedItems: NavLinkItem[] = [
    { label: "Accueil", icon: FiHome, href: "/" },
    { label: "Articles", icon: FiFileText, href: "/articles" },
    { label: "Events", icon: FiCalendar, href: "/events" }
  ];
  const topLevelItems: NavLinkItem[] = variant === "authenticated"
    ? authenticatedItems
    : publicItems;

  return (
    <Stack
      align="stretch"
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      borderRadius="16px"
      boxShadow="sm"
      maxW={{ base: "100%", sm: "420px" }}
      ml="auto"
      mt={3}
      px={{ base: 2.5, sm: 3 }}
      py={2.5}
      spacing={1.5}
      w="100%"
    >
      {topLevelItems.map((item) => (
        <MobileHeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
      ))}

      {variant === "authenticated" ? (
        <Box borderTop="1px solid" borderColor="canvas.200" mt={1} pt={2} w="100%">
          <Button
            _hover={{ bg: "canvas.100" }}
            borderRadius="12px"
            color="brand.500"
            h="48px"
            onClick={userDisclosure.onToggle}
            px={3}
            variant="ghost"
            w="100%"
          >
            <HStack justify="space-between" spacing={3} w="100%">
              <HStack minW={0} spacing={2}>
                <AppIcon color="brand.500" icon={FiUser} size="lg" />
                <Text fontSize="15px" fontWeight="600" lineHeight="1.3" noOfLines={1}>
                  {userLabel}
                </Text>
              </HStack>
              <AppIcon color="brand.500" icon={userDisclosure.isOpen ? FiChevronUp : FiChevronDown} size="sm" strokeWidth={2} />
            </HStack>
          </Button>
          <Collapse in={userDisclosure.isOpen}>
            <Stack pt={1.5} spacing={1}>
              {userItems.map((item) => (
                <MobileHeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
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
  const { canAccessAdminDashboard, user } = useAuth();
  const userLabel = getUserLabel(user?.firstName, user?.username);
  const publicItems: NavLinkItem[] = [
    { label: "Accueil", icon: FiHome, href: "/" },
    { label: "Articles", icon: FiFileText, href: "/articles" },
    { label: "Events", icon: FiCalendar, href: "/events" },
    { label: "Mon compte", icon: FiUser, href: "/login" }
  ];
  const authenticatedItems: NavLinkItem[] = [
    { label: "Accueil", icon: FiHome, href: "/" },
    { label: "Articles", icon: FiFileText, href: "/articles" },
    { label: "Events", icon: FiCalendar, href: "/events" }
  ];

  return (
    <Box pb={{ base: 7, md: 8 }} pt={{ base: 5, md: 7 }}>
      <Flex align={{ base: "start", md: "center" }} gap={{ base: 2.5, md: 5 }} justify="space-between">
        <GovernmentBrand />

        <Stack align="center" flex="1" px={{ base: 1, sm: 4, md: 10 }} spacing={1}>
          <Text color="brand.500" fontSize={{ base: "16px", sm: "22px", md: "30px", xl: "34px" }} fontWeight="700" lineHeight="1.1" textAlign="center">
            (RE) Sources Relationnelles
          </Text>
        </Stack>

        <Box minW={{ base: "44px", lg: "420px" }}>
          <Show above="lg">
            <HStack align="center" justify="flex-end" spacing={4}>
              {(variant === "authenticated" ? authenticatedItems : publicItems).map((item) => (
                <HeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
              ))}
              {variant === "authenticated" ? <UserMenu canAccessAdminDashboard={canAccessAdminDashboard} label={userLabel} /> : null}
            </HStack>
          </Show>
          <Show below="lg">
            <Flex justify="flex-end">
              <IconButton
                aria-label="Ouvrir le menu"
                borderRadius="12px"
                boxSize={{ base: "42px", md: "46px" }}
                color="brand.500"
                icon={<AppIcon color="brand.500" icon={FiMenu} size="xl" strokeWidth={2} />}
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
          <MobileNavigation canAccessAdminDashboard={canAccessAdminDashboard} userLabel={userLabel} variant={variant} />
        </Collapse>
      </Show>
    </Box>
  );
}
