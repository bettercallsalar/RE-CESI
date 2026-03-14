import {
  Box,
  Divider,
  HStack,
  IconButton,
  Stack,
  Text,
  Tooltip,
} from "@chakra-ui/react";
import { useEffect, useRef, useState } from "react";
import type { IconType } from "react-icons";
import { LuBold, LuEraser, LuHeading2, LuItalic, LuLink, LuList, LuListOrdered, LuTextQuote, LuUnderline } from "react-icons/lu";
import {
  getArticleTextLength,
  hasMeaningfulArticleContent,
} from "@/features/articles/lib/articleContent";
import { AppIcon } from "@/shared/ui/icons/AppIcon";

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
  icon: IconType;
  command?: string;
  commandValue?: string;
  onTrigger?: () => void;
}

function ToolbarButton({
  label,
  icon,
  onClick,
}: {
  label: string;
  icon: IconType;
  onClick: () => void;
}) {
  return (
    <Tooltip hasArrow label={label} openDelay={150}>
      <IconButton
        aria-label={label}
        bg="white"
        borderColor="canvas.200"
        boxSize={{ base: "56px", md: "60px" }}
        color="ink.800"
        icon={<AppIcon color="ink.800" icon={icon} size="lg" />}
        minW="unset"
        onClick={onClick}
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
      icon: LuBold,
      command: "bold",
    },
    {
      label: "Mettre en italique",
      icon: LuItalic,
      command: "italic",
    },
    {
      label: "Souligner",
      icon: LuUnderline,
      command: "underline",
    },
    {
      label: "Titre",
      icon: LuHeading2,
      command: "formatBlock",
      commandValue: "h2",
    },
    {
      label: "Citation",
      icon: LuTextQuote,
      command: "formatBlock",
      commandValue: "blockquote",
    },
    {
      label: "Liste à puces",
      icon: LuList,
      command: "insertUnorderedList",
    },
    {
      label: "Liste numérotée",
      icon: LuListOrdered,
      command: "insertOrderedList",
    },
    {
      label: "Insérer un lien",
      icon: LuLink,
      onTrigger: insertLink,
    },
    {
      label: "Retirer la mise en forme",
      icon: LuEraser,
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
