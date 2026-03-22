import { Box, Button, FormControl, FormLabel, HStack, Input, Select, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useManageUsersPage } from "@/features/admin/hooks/useManageUsersPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getUserProfileHref } from "@/features/profile/lib/getUserProfileHref";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { PaginationControls } from "@/shared/ui/pagination/PaginationControls";

const STANDARD_USER_ROLE_ID = 1;

export function ManageUsersPage() {
  const { hasPermission, user: currentUser } = useAuth();
  const [roleSelections, setRoleSelections] = useState<Record<number, number>>({});
  const {
    users,
    roles,
    isLoading,
    isSubmitting,
    message,
    page,
    totalPages,
    totalCount,
    filters,
    updateFilter,
    applyFilters,
    resetFilters,
    goToPage,
    setUserBanStatus,
    setUserRole
  } = useManageUsersPage();
  const canBanUsers = hasPermission(PermissionNames.banUser);
  const roleNameById = new Map(roles.map((role) => [role.idRole, role.name]));

  useEffect(() => {
    setRoleSelections({});
  }, [users]);

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Gestion des utilisateurs
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Recherchez un compte, filtrez par role, mettez a jour le role d'un utilisateur et bannissez les comptes standards si necessaire.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {totalCount > 0 ? `${totalCount} utilisateur${totalCount > 1 ? "s" : ""} trouve${totalCount > 1 ? "s" : ""}.` : "Aucun utilisateur a afficher."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        <Stack
          as="form"
          bg="white"
          border="1px solid"
          borderColor="canvas.200"
          borderRadius="16px"
          p={{ base: 5, md: 6 }}
          spacing={5}
          onSubmit={(event) => {
            event.preventDefault();
            void applyFilters();
          }}
        >
          <Text color="ink.800" fontSize={{ base: "18px", md: "20px" }} fontWeight="700">
            Rechercher et filtrer
          </Text>

          <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={5}>
            <FormControl>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Recherche
              </FormLabel>
              <Input
                bg="white"
                borderColor="canvas.200"
                placeholder="Prenom, pseudo ou e-mail"
                value={filters.keyword}
                onChange={(event) => updateFilter("keyword", event.target.value)}
              />
            </FormControl>

            <FormControl>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Role
              </FormLabel>
              <Select
                bg="white"
                borderColor="canvas.200"
                placeholder="Tous les roles"
                value={filters.idRole}
                onChange={(event) => updateFilter("idRole", event.target.value ? Number(event.target.value) : "")}
              >
                {roles.map((role) => (
                  <option key={role.idRole} value={role.idRole}>
                    {role.name}
                  </option>
                ))}
              </Select>
            </FormControl>
          </SimpleGrid>

          <HStack spacing={3} wrap="wrap">
            <Button isDisabled={isLoading} type="submit">
              Rechercher
            </Button>
            <Button
              isDisabled={isLoading}
              onClick={() => {
                void resetFilters();
              }}
              variant="outline"
            >
              Reinitialiser
            </Button>
          </HStack>
        </Stack>

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="220px" key={index} />
            ))}
          </SimpleGrid>
        ) : (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {users.map((user) => (
              <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" key={user.idUser} minH="220px" p={{ base: 5, md: 6 }} spacing={5}>
                <Stack spacing={2}>
                  <HStack align="start" justify="space-between" spacing={4}>
                    <Stack spacing={0.5}>
                      <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                        {user.firstName}
                      </Text>
                      <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                        @{user.username}
                      </Text>
                    </Stack>
                    <Box bg="canvas.100" border="1px solid" borderColor="canvas.200" borderRadius="999px" px={3} py={1}>
                      <Text color="brand.500" fontSize={{ base: "13px", md: "14px" }} fontWeight="700">
                        {roleNameById.get(user.idRole) ?? `Role #${user.idRole}`}
                      </Text>
                    </Box>
                  </HStack>

                  <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                    {user.email}
                  </Text>
                </Stack>

                <Stack spacing={1}>
                  <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Departement
                  </Text>
                  <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                    {user.department.code} - {user.department.name}
                  </Text>
                </Stack>

                <HStack align="center" justify="space-between" spacing={4}>
                  <Stack spacing={1}>
                    <Text color={user.isVerified ? "#0F7B0F" : "#C53030"} fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      {user.isVerified ? "Compte verifie" : "Compte non verifie"}
                    </Text>
                    <Text color={user.isBanned ? "#C53030" : "#0F7B0F"} fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      {user.isBanned ? "Compte banni" : "Compte actif"}
                    </Text>
                  </Stack>
                </HStack>

                <Stack spacing={3}>
                  <FormControl>
                    <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                      Changer le role
                    </FormLabel>
                    <Select
                      bg="white"
                      borderColor="canvas.200"
                      isDisabled={currentUser?.idUser === user.idUser || roles.length === 0}
                      value={roleSelections[user.idUser] ?? user.idRole}
                      onChange={(event) =>
                        setRoleSelections((current) => ({
                          ...current,
                          [user.idUser]: Number(event.target.value)
                        }))
                      }
                    >
                      {roles.map((role) => (
                        <option key={role.idRole} value={role.idRole}>
                          {role.name}
                        </option>
                      ))}
                    </Select>
                  </FormControl>

                  {currentUser?.idUser === user.idUser ? (
                    <Text color="ink.500" fontSize={{ base: "13px", md: "14px" }}>
                      Votre propre role ne peut pas etre modifie depuis cette page.
                    </Text>
                  ) : null}

                  <HStack spacing={3} wrap="wrap">
                    <Button as="a" href={getUserProfileHref(user.idUser)} variant="outline">
                      Voir le profil
                    </Button>

                    <Button
                      isDisabled={isSubmitting || (roleSelections[user.idUser] ?? user.idRole) === user.idRole || currentUser?.idUser === user.idUser}
                      onClick={() => {
                        void setUserRole(user, roleSelections[user.idUser] ?? user.idRole);
                      }}
                    >
                      Mettre a jour le role
                    </Button>

                    {canBanUsers && user.idRole === STANDARD_USER_ROLE_ID ? (
                      <Button
                        _hover={user.isBanned ? { bg: "#276749" } : { bg: "#9B2C2C" }}
                        bg={user.isBanned ? "#2F855A" : "#C53030"}
                        color={user.isBanned ? "surface.onStrong" : "surface.onCritical"}
                        fontSize={{ base: "14px", md: "15px" }}
                        h="40px"
                        isDisabled={isSubmitting}
                        onClick={() => {
                          void setUserBanStatus(user, !user.isBanned);
                        }}
                        px={4}
                      >
                        {user.isBanned ? "Debannir" : "Bannir"}
                      </Button>
                    ) : null}
                  </HStack>
                </Stack>
              </Stack>
            ))}
          </SimpleGrid>
        )}

        <PaginationControls
          isLoading={isLoading}
          onNext={() => {
            void goToPage(page + 1);
          }}
          onPrevious={() => {
            void goToPage(page - 1);
          }}
          page={page}
          totalPages={totalPages}
        />
      </Stack>
    </SiteLayout>
  );
}
