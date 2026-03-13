import {
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
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function LoginForm() {
  const { values, message, isSubmitting, updateField, submit } = useLoginForm();

  return (
    <Card
      bg="white"
      border="1px solid"
      borderColor="blackAlpha.100"
      rounded={{ base: "12px", md: "16px" }}
      shadow="md">
      <CardBody p={{ base: 7, md: 8 }} color="ink.700">
        <Stack gap={7}>
          <Box>
            <Heading color="brand.500" fontSize={{ base: "28px", md: "32px" }} lineHeight="1.15">
              Connexion
            </Heading>
            <Text fontSize={{ base: "16px", md: "17px" }} mt={3}>
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
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Adresse e-mail
              </FormLabel>
              <Input
                autoComplete="email"
                bg="white"
                borderColor="blackAlpha.300"
                fontSize={{ base: "16px", md: "17px" }}
                type="email"
                value={values.email}
                onChange={(event) => updateField("email", event.target.value)}
                placeholder="nom@exemple.fr"
              />
            </FormControl>

            <FormControl isRequired>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Mot de passe
              </FormLabel>
              <Input
                autoComplete="current-password"
                bg="white"
                borderColor="blackAlpha.300"
                fontSize={{ base: "16px", md: "17px" }}
                type="password"
                value={values.password}
                onChange={(event) =>
                  updateField("password", event.target.value)
                }
                placeholder="Votre mot de passe"
              />
            </FormControl>

            {message ? (
              <MessageBanner message={message.message} title={message.title} tone={message.tone} />
            ) : null}

            <Button
              isLoading={isSubmitting}
              alignSelf="end"
              fontSize={{ base: "15px", md: "16px" }}
              h="48px"
              loadingText="Connexion"
              minW={{ base: "100%", sm: "180px" }}
              type="submit">
              Se connecter
            </Button>
          </Stack>
        </Stack>
      </CardBody>
    </Card>
  );
}
