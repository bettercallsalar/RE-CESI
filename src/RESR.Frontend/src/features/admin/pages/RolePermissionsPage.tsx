import {
  Button,
  HStack,
  Skeleton,
  Stack,
  Text
} from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useRolePermissionsPage } from "@/features/admin/hooks/useRolePermissionsPage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface RolePermissionsPageProps {
  idRole: number;
}

export function RolePermissionsPage({ idRole }: RolePermissionsPageProps) {
  const {
    role,
    allPermissions,
    isLoading,
    isSubmitting,
    message,
    activatePermission,
    deactivatePermission
  } = useRolePermissionsPage(idRole);
  const assignedPermissionIds = new Set(role?.permissions.map((permission) => permission.idPermission) ?? []);

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Gestion des permissions
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez les permissions d'un role, ajoutez-en une nouvelle ou retirez une permission existante.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin/roles" variant="outline">
          Retour aux roles
        </Button>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <Stack spacing={6}>
            <Skeleton borderRadius="16px" height="160px" />
            <Skeleton borderRadius="16px" height="420px" />
          </Stack>
        ) : role ? (
          <Stack spacing={{ base: 6, md: 7 }}>
            <Stack
              bg="white"
              border="1px solid"
              borderColor="canvas.200"
              borderRadius="16px"
              p={{ base: 5, md: 6 }}
              spacing={3}
            >
              <HStack align="start" justify="space-between" spacing={4}>
                <Stack spacing={1}>
                  <Text color="ink.800" fontSize={{ base: "24px", md: "28px" }} fontWeight="700">
                    {role.name}
                  </Text>
                  <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                    {role.description || "Aucune description disponible pour ce role."}
                  </Text>
                </Stack>
                <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Role #{role.idRole}
                </Text>
              </HStack>
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                {role.permissions.length} permission{role.permissions.length > 1 ? "s" : ""} actuellement attribuee{role.permissions.length > 1 ? "s" : ""}.
              </Text>
            </Stack>

            <Stack
              bg="white"
              border="1px solid"
              borderColor="canvas.200"
              borderRadius="16px"
              p={{ base: 5, md: 6 }}
              spacing={5}
            >
              <Stack spacing={1}>
                <Text color="ink.800" fontSize={{ base: "18px", md: "20px" }} fontWeight="700">
                  Toutes les permissions
                </Text>
                <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                  Chaque permission peut etre activee ou desactivee directement depuis cette liste.
                </Text>
              </Stack>

              <Stack spacing={3}>
                {allPermissions.map((permission) => {
                  const isActive = assignedPermissionIds.has(permission.idPermission);

                  return (
                    <Stack
                      border="1px solid"
                      borderColor={isActive ? "#C6F6D5" : "canvas.200"}
                      borderRadius="12px"
                      key={permission.idPermission}
                      px={4}
                      py={3.5}
                      spacing={2}
                    >
                      <Stack
                        align={{ base: "stretch", lg: "center" }}
                        direction={{ base: "column", lg: "row" }}
                        justify="space-between"
                        spacing={{ base: 3, lg: 4 }}
                      >
                        <Stack flex="1" spacing={1}>
                          <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                            {permission.name}
                          </Text>
                          <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                            {permission.description || "Aucune description disponible pour cette permission."}
                          </Text>
                        </Stack>

                        <Stack
                          align={{ base: "stretch", sm: "center" }}
                          direction={{ base: "column", sm: "row" }}
                          flexShrink={0}
                          justify={{ base: "flex-start", lg: "flex-end" }}
                          spacing={3}
                        >
                          <Text
                            color={isActive ? "#0F7B0F" : "#C53030"}
                            fontSize={{ base: "13px", md: "14px" }}
                            fontWeight="700"
                            minW={{ base: "auto", sm: "72px" }}
                            textAlign={{ base: "left", sm: "right" }}
                          >
                            {isActive ? "Active" : "Inactive"}
                          </Text>

                          <HStack justify={{ base: "flex-start", sm: "flex-end" }} spacing={2.5}>
                            <Button
                              _disabled={{ bg: "#C6F6D5", color: "#276749", cursor: "not-allowed", opacity: 1 }}
                              _hover={{ bg: "#0C6A0C" }}
                              bg="#0F7B0F"
                              color="surface.onStrong"
                              fontSize={{ base: "14px", md: "15px" }}
                              h="40px"
                              isDisabled={isActive || isSubmitting}
                              onClick={() => {
                                void activatePermission(permission.idPermission);
                              }}
                              px={4}
                            >
                              Activer
                            </Button>
                            <Button
                              _disabled={{ bg: "#FED7D7", color: "#9B2C2C", cursor: "not-allowed", opacity: 1 }}
                              _hover={{ bg: "#9B2C2C" }}
                              bg="#C53030"
                              color="surface.onCritical"
                              fontSize={{ base: "14px", md: "15px" }}
                              h="40px"
                              isDisabled={!isActive || isSubmitting}
                              onClick={() => {
                                void deactivatePermission(permission.idPermission);
                              }}
                              px={4}
                            >
                              Desactiver
                            </Button>
                          </HStack>
                        </Stack>
                      </Stack>
                    </Stack>
                  );
                })}
              </Stack>
            </Stack>
          </Stack>
        ) : null}
      </Stack>
    </SiteLayout>
  );
}
