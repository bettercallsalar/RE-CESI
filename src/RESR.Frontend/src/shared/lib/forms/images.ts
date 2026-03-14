import type { ResourceFile } from "@/shared/types/article";

export const MAX_IMAGE_COUNT = 6;
export const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024;

export type ImageValidationError = "too_many" | "invalid_type" | "too_large";

export function getExistingDefaultImageSelection(
  defaultImageId: number | null,
  files: ResourceFile[]
): string {
  if (defaultImageId) {
    return `existing:${defaultImageId}`;
  }

  return files[0] ? `existing:${files[0].idFile}` : "";
}

export function getDefaultImageSelectionAfterImageChange(
  images: File[],
  fallbackSelection = ""
): string {
  return images.length > 0 ? "new:0" : fallbackSelection;
}

export function getDefaultImageIndex(selection: string): number | undefined {
  if (!selection.startsWith("new:")) {
    return undefined;
  }

  const parsedValue = Number(selection.slice(4));
  return Number.isInteger(parsedValue) && parsedValue >= 0 ? parsedValue : undefined;
}

export function getExistingDefaultImageId(selection: string): number | undefined {
  if (!selection.startsWith("existing:")) {
    return undefined;
  }

  const parsedValue = Number(selection.slice(9));
  return Number.isInteger(parsedValue) && parsedValue > 0 ? parsedValue : undefined;
}

export function validateImageFiles(images: File[]): ImageValidationError | null {
  if (images.length > MAX_IMAGE_COUNT) {
    return "too_many";
  }

  for (const image of images) {
    if (!image.type.startsWith("image/")) {
      return "invalid_type";
    }

    if (image.size > MAX_IMAGE_SIZE_BYTES) {
      return "too_large";
    }
  }

  return null;
}

export function getImageValidationMessage(error: ImageValidationError): string {
  switch (error) {
    case "too_many":
      return "Vous ne pouvez pas envoyer plus de 6 images.";
    case "invalid_type":
      return "Seules les images sont autorisees.";
    case "too_large":
      return "Chaque image doit faire moins de 5 Mo.";
  }
}
