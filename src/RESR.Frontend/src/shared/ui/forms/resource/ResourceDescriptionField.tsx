import { FormControl, FormLabel, Text, Textarea } from "@chakra-ui/react";

interface ResourceDescriptionFieldProps {
  value: string;
  onChange: (value: string) => void;
  label?: string;
  placeholder?: string;
  maxLength?: number;
}

export function ResourceDescriptionField({
  value,
  onChange,
  label = "Description",
  placeholder = "Résumé visible dans les listes publiques",
  maxLength = 5000,
}: ResourceDescriptionFieldProps) {
  return (
    <FormControl>
      <FormLabel
        color="ink.800"
        fontSize={{ base: "15px", md: "16px" }}
        fontWeight="700">
        {label}
      </FormLabel>
      <Textarea
        minH="120px"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
      />
      <Text
        color="ink.500"
        fontSize={{ base: "13px", md: "14px" }}
        mt={2}
        textAlign="right">
        {value.length} / {maxLength} caractères
      </Text>
    </FormControl>
  );
}
