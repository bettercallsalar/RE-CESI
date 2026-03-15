import {
  Box,
  Button,
  Card,
  CardBody,
  Divider,
  FormControl,
  FormLabel,
  Grid,
  GridItem,
  Heading,
  Input,
  Select,
  Skeleton,
  Stack,
  Text,
  Textarea
} from "@chakra-ui/react";
import { useRegisterForm } from "@/features/auth/hooks/useRegisterForm";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function RegisterForm() {
  const {
    values,
    departments,
    message,
    isLoadingDepartments,
    isSubmitting,
    updateField,
    submit
  } = useRegisterForm();

  return (
    <Card
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      rounded={{ base: "12px", md: "16px" }}
      shadow="md"
    >
      <CardBody p={{ base: 7, md: 8 }} color="ink.700">
        <Stack gap={7}>
          <Box>
            <Heading color="brand.500" fontSize={{ base: "28px", md: "32px" }} lineHeight="1.15">
              Creer un compte
            </Heading>
            <Text fontSize={{ base: "16px", md: "17px" }} mt={3}>
              Renseignez vos informations pour creer un compte utilisateur sur la plateforme.
            </Text>
          </Box>

          <Divider borderColor="canvas.200" />

          <Stack
            as="form"
            gap={5}
            onSubmit={(event) => {
              event.preventDefault();
              void submit();
            }}
          >
            <Grid gap={5} templateColumns={{ base: "1fr", md: "1fr 1fr" }}>
              <GridItem>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Prenom
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="canvas.200"
                    value={values.firstName}
                    onChange={(event) => updateField("firstName", event.target.value)}
                    placeholder="Votre prenom"
                  />
                </FormControl>
              </GridItem>

              <GridItem>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Nom d'utilisateur
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="canvas.200"
                    value={values.username}
                    onChange={(event) => updateField("username", event.target.value)}
                    placeholder="Pseudo"
                  />
                </FormControl>
              </GridItem>

              <GridItem>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Adresse e-mail
                  </FormLabel>
                  <Input
                    autoComplete="email"
                    bg="white"
                    borderColor="canvas.200"
                    type="email"
                    value={values.email}
                    onChange={(event) => updateField("email", event.target.value)}
                    placeholder="nom@exemple.fr"
                  />
                </FormControl>
              </GridItem>

              <GridItem>
                <FormControl>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Date de naissance
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="canvas.200"
                    type="date"
                    value={values.birthDate}
                    onChange={(event) => updateField("birthDate", event.target.value)}
                  />
                </FormControl>
              </GridItem>

              <GridItem>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Mot de passe
                  </FormLabel>
                  <Input
                    autoComplete="new-password"
                    bg="white"
                    borderColor="canvas.200"
                    type="password"
                    value={values.password}
                    onChange={(event) => updateField("password", event.target.value)}
                    placeholder="Votre mot de passe"
                  />
                </FormControl>
              </GridItem>

              <GridItem>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Confirmer le mot de passe
                  </FormLabel>
                  <Input
                    autoComplete="new-password"
                    bg="white"
                    borderColor="canvas.200"
                    type="password"
                    value={values.confirmPassword}
                    onChange={(event) => updateField("confirmPassword", event.target.value)}
                    placeholder="Retapez votre mot de passe"
                  />
                </FormControl>
              </GridItem>

              <GridItem colSpan={{ base: 1, md: 2 }}>
                <FormControl isRequired>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Departement
                  </FormLabel>
                  {isLoadingDepartments ? (
                    <Skeleton height="48px" />
                  ) : (
                    <Select
                      bg="white"
                      borderColor="canvas.200"
                      placeholder="Choisir un departement"
                      value={values.idDepartment}
                      onChange={(event) =>
                        updateField("idDepartment", event.target.value ? Number(event.target.value) : "")
                      }
                    >
                      {departments.map((department) => (
                        <option key={department.idDepartment} value={department.idDepartment}>
                          {department.code} - {department.name}
                        </option>
                      ))}
                    </Select>
                  )}
                </FormControl>
              </GridItem>

              <GridItem colSpan={{ base: 1, md: 2 }}>
                <FormControl>
                  <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    Biographie
                  </FormLabel>
                  <Textarea
                    bg="white"
                    borderColor="canvas.200"
                    minH="140px"
                    placeholder="Presentez-vous en quelques lignes"
                    value={values.bio}
                    onChange={(event) => updateField("bio", event.target.value)}
                  />
                </FormControl>
              </GridItem>
            </Grid>

            {message ? (
              <MessageBanner message={message.message} title={message.title} tone={message.tone} />
            ) : null}

            <Stack
              align={{ base: "stretch", md: "center" }}
              direction={{ base: "column", md: "row" }}
              justify="space-between"
              spacing={4}
            >
              <Button as="a" href="/login" variant="outline">
                J'ai deja un compte
              </Button>
              <Button
                isLoading={isSubmitting}
                loadingText="Creation"
                minW={{ base: "100%", sm: "220px" }}
                type="submit"
              >
                Creer mon compte
              </Button>
            </Stack>
          </Stack>
        </Stack>
      </CardBody>
    </Card>
  );
}
