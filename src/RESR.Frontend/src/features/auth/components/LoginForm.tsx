import {
  Alert,
  AlertIcon,
  Box,
  Button,
  Card,
  CardBody,
  Divider,
  FormControl,
  FormLabel,
  Heading,
  Input,
  Stack,
  Text,
} from "@chakra-ui/react";
import { useLoginForm } from "@/features/auth/hooks/useLoginForm";

export function LoginForm() {
  const { values, error, isSubmitting, updateField, submit } = useLoginForm();

  return (
    <Card
      bg="white"
      border="1px solid"
      borderColor="blackAlpha.100"
      shadow="sm">
      <CardBody p={{ base: 6, md: 7 }} color="ink.700">
        <Stack gap={6}>
          <Box>
            <Heading color="brand.500" fontSize={{ base: "20px", md: "22px" }}>
              Connexion
            </Heading>
            <Text fontSize="12px" mt={2}>
              Accédez à votre espace sur la plateforme avec les identifiants de l'API existante.
            </Text>
          </Box>

          <Divider borderColor="blackAlpha.200" />

          <Stack
            as="form"
            gap={4}
            onSubmit={(event) => {
              event.preventDefault();
              void submit();
            }}>
            <FormControl isRequired>
              <FormLabel color="ink.800" fontSize="12px" fontWeight="600">
                Adresse e-mail
              </FormLabel>
              <Input
                autoComplete="email"
                bg="white"
                borderColor="blackAlpha.300"
                type="email"
                value={values.email}
                onChange={(event) => updateField("email", event.target.value)}
                placeholder="nom@exemple.fr"
              />
            </FormControl>

            <FormControl isRequired>
              <FormLabel color="ink.800" fontSize="12px" fontWeight="600">
                Mot de passe
              </FormLabel>
              <Input
                autoComplete="current-password"
                bg="white"
                borderColor="blackAlpha.300"
                type="password"
                value={values.password}
                onChange={(event) =>
                  updateField("password", event.target.value)
                }
                placeholder="Votre mot de passe"
              />
            </FormControl>

            {error ? (
              <Alert borderRadius="4px" status="error">
                <AlertIcon />
                {error}
              </Alert>
            ) : null}

            <Button
              isLoading={isSubmitting}
              alignSelf="end"
              fontSize="11px"
              h="28px"
              loadingText="Connexion"
              minW="110px"
              type="submit">
              Se connecter
            </Button>
          </Stack>
        </Stack>
      </CardBody>
    </Card>
  );
}
