import { Button, HStack, SimpleGrid, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";

export function AdminDashboardPage() {
  const { hasPermission, isSuperAdmin } = useAuth();
  const canApproveArticles = hasPermission(PermissionNames.approveArticle);
  const canApproveEvents = hasPermission(PermissionNames.approveEvent);
  const canManageUsers = hasPermission(PermissionNames.manageUsers);
  const canApproveResources = canApproveArticles || canApproveEvents;

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Tableau de bord administration
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Accedez aux espaces de gestion autorises par les permissions presentes dans votre token.
          </Text>
        </>
      }
    >
      <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
        {canApproveResources ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" minH="220px" p={{ base: 5, md: 6 }} spacing={5}>
            <Stack spacing={2}>
              <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                Articles et evenements a approuver
              </Text>
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                Ouvrez la page articles ou evenements selon vos permissions, puis approuvez ou desapprouvez chaque article ou evenement depuis son detail.
              </Text>
            </Stack>
            <HStack flexWrap="wrap" justify="space-between" spacing={4}>
              <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                {canApproveArticles && canApproveEvents
                  ? "Permissions ApproveArticle et/ou ApproveEvent"
                  : canApproveArticles
                    ? "Permission ApproveArticle requise"
                    : "Permission ApproveEvent requise"}
              </Text>
              <HStack flexWrap="wrap" justify="flex-end" spacing={3}>
                {canApproveArticles ? (
                  <Button as="a" href="/admin/articles/pending">
                    Articles
                  </Button>
                ) : null}
                {canApproveEvents ? (
                  <Button as="a" href="/admin/events/pending">
                    Evenements
                  </Button>
                ) : null}
              </HStack>
            </HStack>
          </Stack>
        ) : null}

        {isSuperAdmin ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" minH="220px" p={{ base: 5, md: 6 }} spacing={5}>
            <Stack spacing={2}>
              <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                Controle des roles
              </Text>
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                Consultez tous les roles et modifiez leurs permissions. Cette section reste reservee au SuperAdmin.
              </Text>
            </Stack>
            <HStack justify="space-between" spacing={4}>
              <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                SuperAdmin uniquement
              </Text>
              <Button as="a" href="/admin/roles">
                Ouvrir
              </Button>
            </HStack>
          </Stack>
        ) : null}

        {canManageUsers ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" minH="220px" p={{ base: 5, md: 6 }} spacing={5}>
            <Stack spacing={2}>
              <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                Gestion des utilisateurs
              </Text>
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                Listez uniquement les comptes avec le role User et gerez-les depuis un endpoint backend securise.
              </Text>
            </Stack>
            <HStack justify="space-between" spacing={4}>
              <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                Permission ManageUsers requise
              </Text>
              <Button as="a" href="/admin/users">
                Ouvrir
              </Button>
            </HStack>
          </Stack>
        ) : null}
      </SimpleGrid>
    </SiteLayout>
  );
}
