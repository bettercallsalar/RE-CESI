export function formatPublicationDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "long"
  }).format(new Date(value));
}
