import {
  Box,
  Button,
  Collapse,
  Flex,
  HStack,
  Icon,
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
    <Link _hover={{ color: "ink.800", textDecoration: "none" }} color="brand.500" href={href}>
      <HStack align="center" minH="44px" px={1} spacing={2.5}>
        <Icon as={icon} boxSize={{ base: 5.5, xl: 6 }} color="brand.500" flexShrink={0} strokeWidth={1.75} />
        <Text fontSize={{ base: "15px", xl: "16px" }} fontWeight="600">
          {label}
        </Text>
      </HStack>
    </Link>
  );
}

function getUserLabel(firstName?: string, username?: string) {
  return firstName?.trim() || username?.trim() || "Mon compte";
}

function UserMenu({ isSuperAdmin, label }: { isSuperAdmin: boolean; label: string }) {
  const items: NavLinkItem[] = isSuperAdmin
    ? [
        { label: "Administration", icon: FiShield, href: "/admin/roles" },
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
        alignItems="center"
        _active={{ bg: "transparent" }}
        _hover={{ bg: "transparent", color: "ink.800" }}
        color="brand.500"
        fontSize={{ base: "15px", xl: "16px" }}
        fontWeight="600"
        h="44px"
        lineHeight="1"
        minH="44px"
        minW="auto"
        px={1}
        rightIcon={<Icon as={FiChevronDown} boxSize={{ base: 5, xl: 5.5 }} color="brand.500" strokeWidth={2} />}
        variant="unstyled"
      >
        <HStack align="center" color="brand.500" spacing={2.5}>
          <Icon as={FiUser} boxSize={{ base: 5.5, xl: 6 }} color="brand.500" strokeWidth={1.75} />
          <Text color="brand.500">{label}</Text>
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
            <HStack align="center" spacing={3}>
              <Icon as={item.icon} boxSize="20px" color="brand.500" flexShrink={0} strokeWidth={1.9} />
              <Text>{item.label}</Text>
            </HStack>
          </MenuItem>
        ))}
      </MenuList>
    </Menu>
  );
}

function MobileNavigation({
  isSuperAdmin,
  variant,
  userLabel
}: {
  isSuperAdmin: boolean;
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
  const userItems: NavLinkItem[] = isSuperAdmin
    ? [
        { label: "Administration", icon: FiShield, href: "/admin/roles" },
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
    <Stack align={{ base: "stretch", sm: "flex-end" }} bg="white" border="1px solid" borderColor="canvas.200" mt={3} pb={3} pt={3} px={{ base: 3, sm: 4 }} spacing={2}>
      {topLevelItems.map((item) => (
        <HeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
      ))}

      {variant === "authenticated" ? (
        <Box w="100%">
          <Button
            _hover={{ bg: "canvas.100" }}
            color="brand.500"
            justifyContent="space-between"
            onClick={userDisclosure.onToggle}
            rightIcon={<Icon as={userDisclosure.isOpen ? FiChevronUp : FiChevronDown} boxSize={5} strokeWidth={2} />}
            variant="ghost"
            w="100%"
          >
            <HStack spacing={2.5}>
              <Icon as={FiUser} boxSize={5.5} color="brand.500" strokeWidth={1.75} />
              <Text fontSize="15px" fontWeight="600">
                {userLabel}
              </Text>
            </HStack>
          </Button>
          <Collapse in={userDisclosure.isOpen}>
            <Stack pl={4} pt={2} spacing={1}>
              {userItems.map((item) => (
                <HeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
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
  const { isSuperAdmin, user } = useAuth();
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
                <HeaderLink href={item.href} icon={item.icon} key={item.label} label={item.label} />
              ))}
              {variant === "authenticated" ? <UserMenu isSuperAdmin={isSuperAdmin} label={userLabel} /> : null}
            </HStack>
          </Show>
          <Show below="lg">
            <Flex justify="flex-end">
              <IconButton
                aria-label="Ouvrir le menu"
                boxSize={{ base: "44px", md: "48px" }}
                color="brand.500"
                icon={<Icon as={FiMenu} boxSize={6} strokeWidth={2} />}
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
          <MobileNavigation isSuperAdmin={isSuperAdmin} userLabel={userLabel} variant={variant} />
        </Collapse>
      </Show>
    </Box>
  );
}
