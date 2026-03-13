import {
  Alert,
  AlertIcon,
  Box,
  Button,
  Card,
  CardBody,
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
  Textarea,
} from "@chakra-ui/react";
import { useProfileForm } from "@/features/profile/hooks/useProfileForm";
import { DeleteAccountSection } from "@/features/profile/components/DeleteAccountSection";

export function ProfileForm() {
  const {
    user,
    values,
    departments,
    isLoadingDepartments,
    isSaving,
    isDeleting,
    hasChanges,
    saveMessage,
    error,
    deleteError,
    updateField,
    save,
    deleteAccount,
  } = useProfileForm();

  if (!user || !values) {
    return (
      <Stack spacing={4}>
        <Skeleton height="120px" />
        <Skeleton height="420px" />
      </Stack>
    );
  }

  return (
    <Stack spacing={8}>
      <Card
        bg="white"
        border="1px solid"
        borderColor="blackAlpha.100"
        rounded="16px"
        shadow="md">
        <CardBody p={{ base: 7, md: 8 }}>
          <Stack spacing={7}>
            <Box>
              <Heading
                color="brand.500"
                fontSize={{ base: "28px", md: "32px" }}>
                Mon profil
              </Heading>
              <Text
                color="ink.500"
                fontSize={{ base: "16px", md: "17px" }}
                mt={3}>
                Modifiez vos informations personnelles. Ces réglages sont
                disponibles pour votre compte utilisateur.
              </Text>
            </Box>

            <Grid gap={5} templateColumns={{ base: "1fr", md: "1fr 1fr" }}>
              <GridItem>
                <FormControl isRequired>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Prénom
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="blackAlpha.300"
                    color="ink.900"
                    value={values.firstName}
                    onChange={(event) =>
                      updateField("firstName", event.target.value)
                    }
                  />
                </FormControl>
              </GridItem>
              <GridItem>
                <FormControl isRequired>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Nom d'utilisateur
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="blackAlpha.300"
                    color="ink.900"
                    value={values.username}
                    onChange={(event) =>
                      updateField("username", event.target.value)
                    }
                  />
                </FormControl>
              </GridItem>
              <GridItem>
                <FormControl isRequired>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Adresse e-mail
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="blackAlpha.300"
                    color="ink.900"
                    type="email"
                    value={values.email}
                    onChange={(event) =>
                      updateField("email", event.target.value)
                    }
                  />
                </FormControl>
              </GridItem>
              <GridItem>
                <FormControl>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Date de naissance
                  </FormLabel>
                  <Input
                    bg="white"
                    borderColor="blackAlpha.300"
                    color="ink.900"
                    type="date"
                    value={values.birthDate}
                    onChange={(event) =>
                      updateField("birthDate", event.target.value)
                    }
                  />
                </FormControl>
              </GridItem>
              <GridItem colSpan={{ base: 1, md: 2 }}>
                <FormControl>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Département
                  </FormLabel>
                  {isLoadingDepartments ? (
                    <Skeleton height="48px" />
                  ) : (
                    <Select
                      bg="white"
                      borderColor="blackAlpha.300"
                      color="ink.900"
                      value={values.idDepartment}
                      onChange={(event) =>
                        updateField("idDepartment", Number(event.target.value))
                      }>
                      {departments.map((department) => (
                        <option
                          key={department.idDepartment}
                          value={department.idDepartment}>
                          {department.code} - {department.name}
                        </option>
                      ))}
                    </Select>
                  )}
                </FormControl>
              </GridItem>
              <GridItem colSpan={{ base: 1, md: 2 }}>
                <FormControl>
                  <FormLabel
                    color="ink.800"
                    fontSize={{ base: "15px", md: "16px" }}
                    fontWeight="700">
                    Biographie
                  </FormLabel>
                  <Textarea
                    bg="white"
                    borderColor="blackAlpha.300"
                    color="ink.900"
                    minH="160px"
                    placeholder="Présentez-vous en quelques lignes"
                    value={values.bio}
                    onChange={(event) => updateField("bio", event.target.value)}
                  />
                </FormControl>
              </GridItem>
            </Grid>

            <Stack spacing={4}>
              {saveMessage ? (
                <Alert borderRadius="8px" status="success">
                  <AlertIcon />
                  {saveMessage}
                </Alert>
              ) : null}

              {error ? (
                <Alert borderRadius="8px" status="error" color={"red"}>
                  <AlertIcon />
                  {error}
                </Alert>
              ) : null}
            </Stack>

            <Stack
              align={{ base: "stretch", md: "end" }}
              direction={{ base: "column", md: "row" }}
              spacing={4}>
              <Button
                isDisabled={!hasChanges}
                isLoading={isSaving}
                loadingText="Enregistrement"
                onClick={() => {
                  void save();
                }}>
                Enregistrer les modifications
              </Button>
            </Stack>
          </Stack>
        </CardBody>
      </Card>

      <DeleteAccountSection
        error={deleteError}
        isDeleting={isDeleting}
        onDelete={deleteAccount}
      />
    </Stack>
  );
}
