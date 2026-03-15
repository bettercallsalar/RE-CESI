import { Grid, GridItem, Stack } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { LoginForm } from "@/features/auth/components/LoginForm";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function LoginPage() {
  const [flashMessage, setFlashMessage] = useState<FeedbackMessage | null>(
    null,
  );

  useEffect(() => {
    setFlashMessage(flashMessageStorage.take());
  }, []);

  return (
    <SiteLayout headerVariant="public">
      <Stack spacing={{ base: 6, md: 7 }}>
        {flashMessage ? (
          <MessageBanner
            message={flashMessage.message}
            onClose={() => setFlashMessage(null)}
            title={
              flashMessage.title ??
              (flashMessage.tone === "success" ? "Succes" : "Information")
            }
            tone={flashMessage.tone}
          />
        ) : null}

        <LoginForm />
      </Stack>
    </SiteLayout>
  );
}
