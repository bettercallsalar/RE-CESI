import { Button, HStack, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useRolesManagementPage } from "@/features/admin/hooks/useRolesManagementPage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function RolesManagementPage() {
  const { roles, isLoading, message } = useRolesManagementPage();

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Administration des roles
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Espace reserve au SuperAdmin pour consulter les roles disponibles et ouvrir la gestion de leurs permissions.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {roles.length > 0 ? `${roles.length} role${roles.length > 1 ? "s" : ""} configure${roles.length > 1 ? "s" : ""} dans le backend.` : "Aucun role n'a ete recupere."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 3 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="220px" key={index} />
            ))}
          </SimpleGrid>
        ) : (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {roles.map((role) => (
              <Stack
                bg="white"
                border="1px solid"
                borderColor="canvas.200"
                borderRadius="16px"
                key={role.idRole}
                minH="220px"
                p={{ base: 5, md: 6 }}
                spacing={5}
              >
                <Stack spacing={2}>
                  <HStack align="start" justify="space-between" spacing={4}>
                    <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                      {role.name}
                    </Text>
                    <Text color="brand.500" fontSize={{ base: "13px", md: "14px" }} fontWeight="700">
                      #{role.idRole}
                    </Text>
                  </HStack>
                  <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} minH="48px">
                    {role.description || "Aucune description disponible pour ce role."}
                  </Text>
                </Stack>

                <Stack spacing={1}>
                  <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Permissions actuelles
                  </Text>
                  <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                    {role.permissions.length} permission{role.permissions.length > 1 ? "s" : ""} attribuee{role.permissions.length > 1 ? "s" : ""}.
                  </Text>
                </Stack>

                <Button alignSelf="start" as="a" href={`/admin/roles/${role.idRole}`}>
                  Gerer les permissions
                </Button>
              </Stack>
            ))}
          </SimpleGrid>
        )}
      </Stack>
    </SiteLayout>
  );
}
