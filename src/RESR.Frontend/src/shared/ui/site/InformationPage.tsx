import {
  Box,
  Card,
  CardBody,
  Heading,
  SimpleGrid,
  Stack,
  Text
} from "@chakra-ui/react";
import type { ReactNode } from "react";
import type { IconType } from "react-icons";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { AppIcon } from "@/shared/ui/icons/AppIcon";

interface InformationPageSummaryItem {
  icon: IconType;
  label: string;
  value: string;
}

interface InformationPageSection {
  title: string;
  paragraphs: string[];
  bullets?: string[];
}

interface InformationPageProps {
  eyebrow: string;
  title: string;
  description: string;
  summaryItems: InformationPageSummaryItem[];
  sections: InformationPageSection[];
  note?: string;
  topContent?: ReactNode;
}

export function InformationPage({
  description,
  eyebrow,
  note,
  sections,
  summaryItems,
  topContent,
  title
}: InformationPageProps) {
  const { status } = useAuth();
  const isAuthenticated = status === "authenticated";

  return (
    <SiteLayout
      headerVariant={isAuthenticated ? "authenticated" : "public"}
      intro={(
        <Stack align="center" spacing={3}>
          <Text
            color="brand.500"
            fontSize={{ base: "13px", md: "14px" }}
            fontWeight="700"
            letterSpacing="0.08em"
            textAlign="center"
            textTransform="uppercase"
          >
            {eyebrow}
          </Text>
          <Heading
            as="h1"
            color="ink.800"
            fontSize={{ base: "30px", md: "42px" }}
            lineHeight="1.1"
            maxW="900px"
            textAlign="center"
          >
            {title}
          </Heading>
          <Text
            color="ink.500"
            fontSize={{ base: "16px", md: "18px" }}
            maxW="860px"
            textAlign="center"
          >
            {description}
          </Text>
        </Stack>
      )}
    >
      <Stack spacing={{ base: 6, md: 8 }}>
        {note ? (
          <Box
            bg="white"
            border="1px solid"
            borderColor="canvas.200"
            borderRadius="16px"
            px={{ base: 5, md: 6 }}
            py={{ base: 4, md: 5 }}
          >
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} fontWeight="600">
              {note}
            </Text>
          </Box>
        ) : null}

        {topContent}

        <SimpleGrid columns={{ base: 1, md: 2 }} spacing={4}>
          {summaryItems.map((item) => (
            <Card
              bg="white"
              border="1px solid"
              borderColor="canvas.200"
              key={item.label}
              rounded="16px"
              shadow="sm"
            >
              <CardBody p={{ base: 5, md: 6 }}>
                <Stack align="start" spacing={3}>
                  <AppIcon
                    bg="rgba(52, 43, 154, 0.08)"
                    borderRadius="12px"
                    color="brand.500"
                    icon={item.icon}
                    size="lg"
                  />
                  <Box>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      {item.label}
                    </Text>
                    <Text color="ink.800" fontSize={{ base: "16px", md: "18px" }} fontWeight="600" mt={1}>
                      {item.value}
                    </Text>
                  </Box>
                </Stack>
              </CardBody>
            </Card>
          ))}
        </SimpleGrid>

        <Stack spacing={4}>
          {sections.map((section) => (
            <Card
              bg="white"
              border="1px solid"
              borderColor="canvas.200"
              key={section.title}
              rounded="16px"
              shadow="sm"
            >
              <CardBody p={{ base: 6, md: 7 }}>
                <Stack spacing={4}>
                  <Heading as="h2" color="ink.800" fontSize={{ base: "22px", md: "26px" }} lineHeight="1.2">
                    {section.title}
                  </Heading>

                  {section.paragraphs.map((paragraph) => (
                    <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} key={paragraph} lineHeight="1.75">
                      {paragraph}
                    </Text>
                  ))}

                  {section.bullets?.length ? (
                    <Stack as="ul" pl={5} spacing={2}>
                      {section.bullets.map((bullet) => (
                        <Text
                          as="li"
                          color="ink.500"
                          fontSize={{ base: "15px", md: "16px" }}
                          key={bullet}
                          lineHeight="1.7"
                        >
                          {bullet}
                        </Text>
                      ))}
                    </Stack>
                  ) : null}
                </Stack>
              </CardBody>
            </Card>
          ))}
        </Stack>
      </Stack>
    </SiteLayout>
  );
}
