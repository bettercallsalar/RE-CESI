import { ApiError } from "@/shared/api/httpClient";

const directTranslations: Record<string, string> = {
  "At least one field must be provided for update": "Au moins un champ doit etre fourni pour la mise a jour.",
  "Email format is invalid.": "Le format de l'adresse e-mail est invalide.",
  "Email already exists": "Cette adresse e-mail existe deja.",
  "Username already exists": "Ce nom d'utilisateur existe deja.",
  "Username already exists.": "Ce nom d'utilisateur existe deja.",
  "First name is required": "Le prenom est obligatoire.",
  "User account is deleted": "Le compte utilisateur est supprime.",
  "Invalid email or password": "Adresse e-mail ou mot de passe invalide.",
  "User email is not verified": "L'adresse e-mail du compte n'est pas verifiee.",
  "Missing or invalid Authorization header.": "En-tete d'autorisation manquant ou invalide.",
  "Invalid token or unauthorized access.": "Jeton invalide ou acces non autorise.",
  "Invalid token or missing subject claim.": "Jeton invalide ou identifiant utilisateur manquant.",
  "Title is required.": "Le titre est obligatoire.",
  "Content is required.": "Le contenu est obligatoire.",
  "Content cannot be empty.": "Le contenu ne peut pas etre vide.",
  "IdUser must be greater than 0.": "L'identifiant utilisateur doit etre superieur a 0.",
  "IdCategory must be greater than 0.": "La categorie doit etre superieure a 0.",
  "Visibility must be PUBLIC or PRIVATE.": "La visibilite doit etre PUBLIC ou PRIVATE.",
  "EndDate cannot be earlier than StartDate.": "La date de fin ne peut pas etre anterieure a la date de debut.",
  "EndDate must be later than StartDate.": "La date de fin doit etre strictement apres la date de debut.",
  "L'image par defaut selectionnee est invalide.": "L'image par defaut selectionnee est invalide.",
  "Aucune image n'a ete envoyee pour definir une image par defaut.": "Aucune image n'a ete envoyee pour definir une image par defaut.",
  "The request failed.": "La requete a echoue."
};

function translateKnownMessage(message: string) {
  if (directTranslations[message]) {
    return directTranslations[message];
  }

  if (/^Department (\d+) does not exist\.?$/.test(message)) {
    const match = message.match(/^Department (\d+) does not exist\.?$/);
    return `Le departement ${match?.[1]} n'existe pas.`;
  }

  if (/^Role (\d+) does not exist\.?$/.test(message)) {
    const match = message.match(/^Role (\d+) does not exist\.?$/);
    return `Le role ${match?.[1]} n'existe pas.`;
  }

  if (/^User (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^User (\d+) not found\.?$/);
    return `Utilisateur ${match?.[1]} introuvable.`;
  }

  if (/^Article resource (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Article resource (\d+) not found\.?$/);
    return `Article ${match?.[1]} introuvable.`;
  }

  if (/^Event resource (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Event resource (\d+) not found\.?$/);
    return `Evenement ${match?.[1]} introuvable.`;
  }

  return message;
}

export function getErrorMessage(error: unknown) {
  if (error instanceof ApiError) {
    return translateKnownMessage(error.message);
  }

  if (error instanceof Error) {
    return translateKnownMessage(error.message);
  }

  return "Une erreur est survenue.";
}
