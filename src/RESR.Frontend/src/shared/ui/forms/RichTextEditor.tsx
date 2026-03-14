import { LinkIcon, createIcon } from "@chakra-ui/icons";
import {
  Box,
  Divider,
  HStack,
  IconButton,
  Stack,
  Text,
  Tooltip,
} from "@chakra-ui/react";
import { useEffect, useRef, useState, type ReactElement } from "react";
import {
  getArticleTextLength,
  hasMeaningfulArticleContent,
} from "@/features/articles/lib/articleContent";

interface RichTextEditorProps {
  label?: string;
  helperText?: string;
  minH?: { base: string; md?: string };
  placeholder?: string;
  value: string;
  onChange: (value: string) => void;
}

interface ToolbarAction {
  label: string;
  icon: ReactElement;
  command?: string;
  commandValue?: string;
  onTrigger?: () => void;
}

const BoldTextIcon = createIcon({
  displayName: "BoldTextIcon",
  viewBox: "0 0 24 24",
  path: (
    <path
      d="M8 5h6a4 4 0 0 1 0 8H8zm0 8h7a4 4 0 0 1 0 8H8z"
      fill="none"
      stroke="currentColor"
      strokeLinecap="round"
      strokeLinejoin="round"
      strokeWidth="2"
    />
  ),
});

const ItalicTextIcon = createIcon({
  displayName: "ItalicTextIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M10 5h8"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M6 19h8"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M14 5 10 19"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
    </>
  ),
});

const UnderlineTextIcon = createIcon({
  displayName: "UnderlineTextIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M8 5v6a4 4 0 0 0 8 0V5"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M6 19h12"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
    </>
  ),
});

const HeadingTextIcon = createIcon({
  displayName: "HeadingTextIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M5 6v12"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M13 6v12"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M5 12h8"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="M17 8h2l-1 8h2"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
    </>
  ),
});

const QuoteTextIcon = createIcon({
  displayName: "QuoteTextIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M8 9H5v5h4v-3H7c0-2 1-3 3-4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
      <path
        d="M17 9h-3v5h4v-3h-2c0-2 1-3 3-4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
    </>
  ),
});

const BulletListIcon = createIcon({
  displayName: "BulletListIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <circle cx="5" cy="7" r="1.5" fill="currentColor" />
      <circle cx="5" cy="12" r="1.5" fill="currentColor" />
      <circle cx="5" cy="17" r="1.5" fill="currentColor" />
      <path
        d="M9 7h10M9 12h10M9 17h10"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
    </>
  ),
});

const NumberListIcon = createIcon({
  displayName: "NumberListIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M4 7h2V5M6 5v4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
      <path
        d="M4 11h2l-2 3h2"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
      <path
        d="M4 19h2a1 1 0 0 0 0-2H4m2 0a1 1 0 0 1 0-2H4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
      <path
        d="M10 7h10M10 12h10M10 17h10"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
    </>
  ),
});

const ClearFormatIcon = createIcon({
  displayName: "ClearFormatIcon",
  viewBox: "0 0 24 24",
  path: (
    <>
      <path
        d="M6 18 14 6l4 6H10"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="2"
      />
      <path
        d="m15 15 4 4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
      <path
        d="m19 15-4 4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeWidth="2"
      />
    </>
  ),
});

function ToolbarButton({
  label,
  icon,
  onClick,
}: {
  label: string;
  icon: ReactElement;
  onClick: () => void;
}) {
  return (
    <Tooltip hasArrow label={label} openDelay={150}>
      <IconButton
        aria-label={label}
        bg="white"
        borderColor="canvas.200"
        color="ink.800"
        icon={icon}
        onClick={onClick}
        boxSize={{ base: "56px", md: "60px" }}
        minW="unset"
        type="button"
        variant="ghost"
        _hover={{ bg: "canvas.200" }}
      />
    </Tooltip>
  );
}

export function RichTextEditor({
  label,
  helperText,
  minH = { base: "180px", md: "240px" },
  placeholder = "Commencez à rédiger votre contenu",
  value,
  onChange,
}: RichTextEditorProps) {
  const editorRef = useRef<HTMLDivElement | null>(null);
  const [isFocused, setIsFocused] = useState(false);

  useEffect(() => {
    const editor = editorRef.current;

    if (!editor) {
      return;
    }

    if (editor.innerHTML !== value) {
      editor.innerHTML = value;
    }
  }, [value]);

  function syncEditorValue() {
    onChange(editorRef.current?.innerHTML ?? "");
  }

  function applyCommand(command: string, commandValue?: string) {
    editorRef.current?.focus();
    window.document.execCommand(command, false, commandValue);
    syncEditorValue();
  }

  function insertLink() {
    const url = window.prompt("Adresse du lien à insérer");

    if (!url) {
      return;
    }

    applyCommand("createLink", url);
  }

  const actions: ToolbarAction[] = [
    {
      label: "Mettre en gras",
      icon: <BoldTextIcon boxSize={6} />,
      command: "bold",
    },
    {
      label: "Mettre en italique",
      icon: <ItalicTextIcon boxSize={6} />,
      command: "italic",
    },
    {
      label: "Souligner",
      icon: <UnderlineTextIcon boxSize={6} />,
      command: "underline",
    },
    {
      label: "Titre",
      icon: <HeadingTextIcon boxSize={6} />,
      command: "formatBlock",
      commandValue: "h2",
    },
    {
      label: "Citation",
      icon: <QuoteTextIcon boxSize={6} />,
      command: "formatBlock",
      commandValue: "blockquote",
    },
    {
      label: "Liste à puces",
      icon: <BulletListIcon boxSize={6} />,
      command: "insertUnorderedList",
    },
    {
      label: "Liste numérotée",
      icon: <NumberListIcon boxSize={6} />,
      command: "insertOrderedList",
    },
    {
      label: "Insérer un lien",
      icon: <LinkIcon boxSize={6} />,
      onTrigger: insertLink,
    },
    {
      label: "Retirer la mise en forme",
      icon: <ClearFormatIcon boxSize={6} />,
      command: "removeFormat",
    },
  ];

  const hasContent = hasMeaningfulArticleContent(value);

  return (
    <Stack spacing={2}>
      {label ? (
        <Text
          color="ink.800"
          fontSize={{ base: "15px", md: "16px" }}
          fontWeight="700">
          {label}
        </Text>
      ) : null}

      {helperText ? (
        <Text
          color="ink.500"
          fontSize={{ base: "14px", md: "15px" }}
          lineHeight="1.6">
          {helperText}
        </Text>
      ) : null}

      <Box
        bg="white"
        border="1px solid"
        borderColor="canvas.200"
        overflow="hidden"
        rounded="16px">
        <HStack
          align="center"
          bg="white"
          borderBottom="1px solid"
          borderColor="canvas.200"
          flexWrap="wrap"
          px={{ base: 3, md: 4 }}
          py={{ base: 2.5, md: 3 }}
          spacing={2}>
          {actions.map((action, index) => (
            <Box alignItems="center" display="flex" key={action.label}>
              <ToolbarButton
                icon={action.icon}
                label={action.label}
                onClick={() => {
                  if (action.onTrigger) {
                    action.onTrigger();
                    return;
                  }

                  if (action.command) {
                    applyCommand(action.command, action.commandValue);
                  }
                }}
              />
              {index === 2 || index === 4 || index === 6 ? (
                <Divider
                  borderColor="canvas.200"
                  h="32px"
                  mx={{ base: 1.5, md: 2 }}
                  orientation="vertical"
                />
              ) : null}
            </Box>
          ))}
        </HStack>

        <Box position="relative">
          {!hasContent && !isFocused ? (
            <Text
              color="ink.500"
              fontSize={{ base: "15px", md: "16px" }}
              left={5}
              pointerEvents="none"
              position="absolute"
              top={4}>
              {placeholder}
            </Text>
          ) : null}

          <Box
            ref={editorRef}
            color="ink.800"
            contentEditable
            fontSize={{ base: "16px", md: "17px" }}
            minH={minH}
            onBlur={() => setIsFocused(false)}
            onFocus={() => setIsFocused(true)}
            onInput={syncEditorValue}
            px={{ base: 4, md: 4.5 }}
            py={{ base: 3.5, md: 4 }}
            role="textbox"
            suppressContentEditableWarning
            sx={{
              lineHeight: 1.7,
              outline: "none",
              whiteSpace: "pre-wrap",
              wordBreak: "break-word",
              "& h2": {
                fontSize: "1.35rem",
                fontWeight: 700,
                marginBlock: "0.5rem",
              },
              "& blockquote": {
                borderLeft: "4px solid #342B9A",
                marginBlock: "0.5rem",
                paddingInlineStart: "0.875rem",
              },
              "& ul, & ol": {
                paddingInlineStart: "1.35rem",
              },
              "& p": {
                marginBottom: "0.5rem",
              },
            }}
          />
        </Box>
      </Box>

      <Text color="ink.500" fontSize={{ base: "12px", md: "13px" }}>
        {getArticleTextLength(value)} caractères de texte saisis
      </Text>
    </Stack>
  );
}
