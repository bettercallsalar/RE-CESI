import { FormControl, FormLabel, Input } from "@chakra-ui/react";

interface ResourceTitleFieldProps {
  value: string;
  onChange: (value: string) => void;
  isRequired?: boolean;
  label?: string;
  placeholder?: string;
}

export function ResourceTitleField({
  value,
  onChange,
  isRequired = true,
  label = "Titre",
  placeholder = "Titre de la ressource",
}: ResourceTitleFieldProps) {
  return (
    <FormControl isRequired={isRequired}>
      <FormLabel
        color="ink.800"
        fontSize={{ base: "15px", md: "16px" }}
        fontWeight="700">
        {label}
      </FormLabel>
      <Input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
      />
    </FormControl>
  );
}
