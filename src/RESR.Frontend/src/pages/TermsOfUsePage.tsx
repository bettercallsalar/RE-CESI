import { FiFileText, FiMessageSquare, FiShield, FiUsers } from "react-icons/fi";
import { InformationPage } from "@/shared/ui/site/InformationPage";

const summaryItems = [
  {
    icon: FiFileText,
    label: "Objet",
    value: "Donner acces a des ressources, articles, events et outils d'echange"
  },
  {
    icon: FiUsers,
    label: "Acteurs",
    value: "Citoyen non connecte, citoyen connecte, moderateur, administrateur, super-administrateur"
  },
  {
    icon: FiMessageSquare,
    label: "Echanges",
    value: "Commentaires et reponses sur les ressources publiques, avec moderation"
  },
  {
    icon: FiShield,
    label: "Sanctions possibles",
    value: "Suspension, depublication ou suppression de contenus et de comptes"
  }
];

const sections = [
  {
    title: "Objet des conditions d'utilisation",
    paragraphs: [
      "Les presentes conditions generales d'utilisation definissent les regles d'acces et d'usage de la plateforme (RE)Sources Relationnelles. Le service a pour objectif de proposer des ressources et outils permettant de creer, renforcer et enrichir les relations entre citoyens.",
      "Conformement au cahier des charges, la solution comprend une vue publique, un espace utilisateur connecte et des fonctions d'administration et de moderation."
    ]
  },
  {
    title: "Acces au service et comptes",
    paragraphs: [
      "Une partie du service est accessible publiquement pour consulter les ressources. Les fonctionnalites personnalisees, comme la publication, les favoris, les mises de cote, les suivis ou les commentaires, supposent la creation d'un compte.",
      "L'utilisateur s'engage a fournir des informations exactes lors de l'inscription, a preserver la confidentialite de ses identifiants et a ne pas usurper l'identite d'un tiers."
    ]
  },
  {
    title: "Publication, partage et moderation",
    paragraphs: [
      "Le service permet la creation de ressources, l'ajout de commentaires, les reponses a des commentaires et, selon les roles, la validation ou la moderation des contenus publics.",
      "Tout contenu peut etre refuse, masque, suspendu ou supprime s'il contrevient a la loi, au bon fonctionnement du service, a la dignite des personnes ou aux presentes CGU."
    ],
    bullets: [
      "interdiction des contenus illicites, diffamatoires, injurieux, discriminatoires ou violents",
      "interdiction de publier des donnees personnelles de tiers sans base legitime",
      "interdiction de contourner les mecanismes de moderation, de securite ou de gestion des roles"
    ]
  },
  {
    title: "Engagements des utilisateurs",
    paragraphs: [
      "Chaque utilisateur est responsable des informations, ressources et messages qu'il diffuse. Il s'engage a utiliser la plateforme de bonne foi, dans le respect des autres usagers et des finalites du projet.",
      "Les contenus publies doivent rester pertinents au regard de la thematique relationnelle, sociale ou de prevention mise en avant par la plateforme."
    ]
  },
  {
    title: "Propriete intellectuelle et reutilisation",
    paragraphs: [
      "L'utilisateur garantit disposer des droits necessaires sur les contenus qu'il publie. Il autorise la plateforme a les afficher, les moderer et les rendre accessibles selon les niveaux de visibilite prevus par le service.",
      "La reutilisation des contenus du site en dehors du service reste soumise aux droits de leurs auteurs et a la politique de diffusion definie par l'editeur."
    ]
  },
  {
    title: "Suspension, suppression et evolution du service",
    paragraphs: [
      "L'editeur peut suspendre un compte, limiter certaines fonctionnalites ou retirer un contenu en cas d'abus, de non-respect des CGU, de risque de securite ou d'obligation legale.",
      "La maquette et ses fonctionnalites peuvent evoluer au fil du projet. Toute version de production devra afficher une version datee de ces CGU et informer les utilisateurs en cas de modification substantielle."
    ]
  },
  {
    title: "Donnees personnelles et responsabilite",
    paragraphs: [
      "L'utilisation du service implique un traitement de donnees personnelles de compte et de contenus utilisateur. Les details sont presentes dans la page Donnees personnelles et cookies.",
      "La presente maquette etant un projet pedagogique, elle ne constitue pas un service public officiel. L'editeur ne saurait etre tenu responsable d'un usage qui depasserait le cadre de demonstration prevu."
    ]
  }
];

export function TermsOfUsePage() {
  return (
    <InformationPage
      description="Regles d'acces et d'usage de la plateforme, dans le cadre de la maquette pedagogique (RE)Sources Relationnelles."
      eyebrow="Utilisation du service"
      note="Le contenu de ces CGU est aligne sur les fonctionnalites et roles decrits dans le cahier des charges. Il devra etre complete par l'editeur final en cas de mise en production."
      sections={sections}
      summaryItems={summaryItems}
      title="Conditions generales d'utilisation"
    />
  );
}
