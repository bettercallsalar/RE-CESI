import { Box, Button, HStack, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useManageUsersPage } from "@/features/admin/hooks/useManageUsersPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getUserProfileHref } from "@/features/profile/lib/getUserProfileHref";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function ManageUsersPage() {
  const { hasPermission } = useAuth();
  const { users, isLoading, isSubmitting, message, page, totalPages, totalCount, goToPage, setUserBanStatus } = useManageUsersPage();
  const canBanUsers = hasPermission(PermissionNames.banUser);

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Gestion des utilisateurs
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Cette page liste uniquement les comptes avec le role User. Les comptes admins et superadmins ne sont pas exposes ici.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {totalCount > 0 ? `${totalCount} utilisateur${totalCount > 1 ? "s" : ""} standard${totalCount > 1 ? "s" : ""} trouve${totalCount > 1 ? "s" : ""}.` : "Aucun utilisateur standard a afficher."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

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
                        User
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

                  <HStack spacing={3} wrap="wrap">
                    <Button as="a" href={getUserProfileHref(user.idUser)} variant="outline">
                      Voir le profil
                    </Button>

                    {canBanUsers ? (
                      <Button
                        _hover={user.isBanned ? { bg: "#276749" } : { bg: "#9B2C2C" }}
                        bg={user.isBanned ? "#2F855A" : "#C53030"}
                        color="white"
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
                </HStack>
              </Stack>
            ))}
          </SimpleGrid>
        )}

        <HStack justify="space-between" spacing={4}>
          <Button
            isDisabled={page <= 1 || isLoading}
            onClick={() => {
              void goToPage(page - 1);
            }}
            variant="outline"
          >
            Page precedente
          </Button>
          <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
            Page {page} {totalPages > 0 ? `sur ${totalPages}` : ""}
          </Text>
          <Button
            isDisabled={isLoading || totalPages === 0 || page >= totalPages}
            onClick={() => {
              void goToPage(page + 1);
            }}
            variant="outline"
          >
            Page suivante
          </Button>
        </HStack>
      </Stack>
    </SiteLayout>
  );
}
