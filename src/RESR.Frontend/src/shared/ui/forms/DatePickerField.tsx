import {
  FormControl,
  FormLabel,
  IconButton,
  Input,
  InputGroup,
  InputLeftElement,
} from "@chakra-ui/react";
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
  type = "date",
}: DatePickerFieldProps) {
  const inputRef = useRef<HTMLInputElement | null>(null);

  function openPicker() {
    inputRef.current?.showPicker?.();
    inputRef.current?.focus();
  }

  return (
    <FormControl isRequired={isRequired}>
      <FormLabel
        color="ink.800"
        fontSize={{ base: "15px", md: "16px" }}
        fontWeight="700">
        {label}
      </FormLabel>
      <InputGroup>
        <InputLeftElement h="100%" pointerEvents="auto" width="72px">
          <IconButton
            aria-label={`Choisir ${label.toLowerCase()}`}
            bg="canvas.100"
            border="1px solid"
            borderColor="canvas.200"
            borderRadius="12px"
            boxSize="44px"
            color="brand.500"
            icon={<FiCalendar size="22px" strokeWidth={1.75} />}
            minW="44px"
            onClick={openPicker}
            size="md"
            variant="ghost"
            _active={{ bg: "canvas.200" }}
            _hover={{ bg: "canvas.200" }}
          />
        </InputLeftElement>
        <Input
          max={max}
          min={min}
          onChange={(event) => onChange(event.target.value)}
          pl="72px"
          ref={inputRef}
          sx={{
            "&::-webkit-calendar-picker-indicator": {
              display: "none",
              opacity: 0,
            },
          }}
          type={type}
          value={value}
        />
      </InputGroup>
    </FormControl>
  );
}
