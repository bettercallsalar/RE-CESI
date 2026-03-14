import { FormControl, FormLabel, Icon, IconButton, Input, InputGroup, InputRightElement } from "@chakra-ui/react";
import { useRef } from "react";
import { FiCalendar } from "react-icons/fi";

interface DatePickerFieldProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  isRequired?: boolean;
  min?: string;
  max?: string;
  type?: "date" | "datetime-local";
}

export function DatePickerField({
  label,
  value,
  onChange,
  isRequired = false,
  min,
  max,
  type = "date"
}: DatePickerFieldProps) {
  const inputRef = useRef<HTMLInputElement | null>(null);

  function openPicker() {
    inputRef.current?.showPicker?.();
    inputRef.current?.focus();
  }

  return (
    <FormControl isRequired={isRequired}>
      <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
        {label}
      </FormLabel>
      <InputGroup>
        <Input
          max={max}
          min={min}
          onChange={(event) => onChange(event.target.value)}
          ref={inputRef}
          type={type}
          value={value}
        />
        <InputRightElement>
          <IconButton
            aria-label={`Choisir ${label.toLowerCase()}`}
            icon={<Icon as={FiCalendar} boxSize={4.5} />}
            onClick={openPicker}
            size="sm"
            variant="ghost"
          />
        </InputRightElement>
      </InputGroup>
    </FormControl>
  );
}
