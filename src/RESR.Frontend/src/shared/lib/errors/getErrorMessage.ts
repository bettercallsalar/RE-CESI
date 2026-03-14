import { ApiError } from "@/shared/api/httpClient";

const directTranslations: Record<string, string> = {
  "At least one field must be provided for update": "Au moins un champ doit etre fourni pour la mise a jour.",
  "Email format is invalid.": "Le format de l'adresse e-mail est invalide.",
  "Email already exists": "Cette adresse e-mail existe deja.",
  "Username already exists": "Ce nom d'utilisateur existe deja.",
  "Username already exists.": "Ce nom d'utilisateur existe deja.",
  "First name is required": "Le prenom est obligatoire.",
  "User account is deleted": "Le compte utilisateur est supprime.",
  "User account is banned": "Le compte utilisateur est banni.",
  "Invalid email or password": "Adresse e-mail ou mot de passe invalide.",
  "User email is not verified": "L'adresse e-mail du compte n'est pas verifiee.",
  "Missing or invalid Authorization header.": "En-tete d'autorisation manquant ou invalide.",
  "Invalid token or unauthorized access.": "Jeton invalide ou acces non autorise.",
  "Invalid token or missing subject claim.": "Jeton invalide ou identifiant utilisateur manquant.",
  "A user cannot follow themselves": "Vous ne pouvez pas vous suivre vous-meme.",
  "User already reacted to this resource": "Vous avez deja une reaction sur cette ressource.",
  "Title is required.": "Le titre est obligatoire.",
  "The request payload is invalid.": "Les donnees envoyees sont invalides.",
  "The Content field is required.": "Le contenu est obligatoire.",
  "The field Content must be a string or array type with a maximum length of '2000'.": "Le contenu ne doit pas depasser 2000 caracteres.",
  "Content is required.": "Le contenu est obligatoire.",
  "Content is required": "Le contenu est obligatoire.",
  "Content cannot be empty.": "Le contenu ne peut pas etre vide.",
  "Comment is deleted": "Le commentaire est supprime.",
  "Comment is already deleted": "Le commentaire est deja supprime.",
  "Cannot reply to a deleted comment": "Impossible de repondre a un commentaire supprime.",
  "Parent comment must belong to the same resource": "La reponse doit appartenir a la meme ressource.",
  "You are not allowed to delete this comment": "Vous n'etes pas autorise a supprimer ce commentaire.",
  "IdUser must be greater than 0.": "L'identifiant utilisateur doit etre superieur a 0.",
  "IdUser must be greater than 0": "L'identifiant utilisateur doit etre superieur a 0.",
  "IdResource must be greater than 0": "L'identifiant de la ressource doit etre superieur a 0.",
  "IdComment must be greater than 0": "L'identifiant du commentaire doit etre superieur a 0.",
  "IdParentComment must be greater than 0": "L'identifiant du commentaire parent doit etre superieur a 0.",
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

  if (/^Permission (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Permission (\d+) not found\.?$/);
    return `La permission ${match?.[1]} est introuvable.`;
  }

  if (/^Comment (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Comment (\d+) not found\.?$/);
    return `Le commentaire ${match?.[1]} est introuvable.`;
  }

  if (/^Permission (\d+) is already assigned to role (\d+)\.?$/.test(message)) {
    const match = message.match(/^Permission (\d+) is already assigned to role (\d+)\.?$/);
    return `La permission ${match?.[1]} est deja attribuee au role ${match?.[2]}.`;
  }

  if (/^Permission (\d+) is not assigned to role (\d+)\.?$/.test(message)) {
    const match = message.match(/^Permission (\d+) is not assigned to role (\d+)\.?$/);
    return `La permission ${match?.[1]} n'est pas attribuee au role ${match?.[2]}.`;
  }

  if (/^User (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^User (\d+) not found\.?$/);
    return `Utilisateur ${match?.[1]} introuvable.`;
  }

  if (/^Follow (\d+)->(\d+) already exists\.?$/.test(message)) {
    return "Vous suivez deja cet utilisateur.";
  }

  if (/^Follow (\d+)->(\d+) not found\.?$/.test(message)) {
    return "Vous ne suivez pas ou plus cet utilisateur.";
  }

  if (/^Article resource (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Article resource (\d+) not found\.?$/);
    return `Article ${match?.[1]} introuvable.`;
  }

  if (/^Event resource (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Event resource (\d+) not found\.?$/);
    return `Evenement ${match?.[1]} introuvable.`;
  }

  if (/^Resource (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Resource (\d+) not found\.?$/);
    return `La ressource ${match?.[1]} est introuvable.`;
  }

  if (/^Reaction (\d+) not found\.?$/.test(message)) {
    const match = message.match(/^Reaction (\d+) not found\.?$/);
    return `La reaction ${match?.[1]} est introuvable.`;
  }

  return message;
}

export function getErrorMessage(error: unknown) {
  if (error instanceof ApiError) {
    if (error.status === 403) {
      return "Acces refuse.";
    }

    return translateKnownMessage(error.message);
  }

  if (error instanceof Error) {
    return translateKnownMessage(error.message);
  }

  return "Une erreur est survenue.";
}
