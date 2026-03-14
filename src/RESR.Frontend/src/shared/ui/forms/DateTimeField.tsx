import { CalendarIcon } from "@chakra-ui/icons";
import { FormControl, FormLabel, IconButton, Input, InputGroup, InputRightElement } from "@chakra-ui/react";
import { useRef } from "react";

interface DateTimeFieldProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  isRequired?: boolean;
  min?: string;
  max?: string;
}

export function DateTimeField({ label, value, onChange, isRequired = false, min, max }: DateTimeFieldProps) {
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
          type="datetime-local"
          value={value}
        />
        <InputRightElement>
          <IconButton
            aria-label={`Choisir ${label.toLowerCase()}`}
            icon={<CalendarIcon />}
            onClick={openPicker}
            size="sm"
            variant="ghost"
          />
        </InputRightElement>
      </InputGroup>
    </FormControl>
  );
}
