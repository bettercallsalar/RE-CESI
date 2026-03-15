import { FiDatabase, FiLock, FiMonitor, FiUserCheck } from "react-icons/fi";
import { InformationPage } from "@/shared/ui/site/InformationPage";

const summaryItems = [
  {
    icon: FiDatabase,
    label: "Donnees de compte",
    value: "Prenom, pseudo, email, date de naissance, biographie, departement"
  },
  {
    icon: FiUserCheck,
    label: "Contenus utilisateur",
    value: "Articles, events, commentaires, reactions, favoris et suivis selon l'usage"
  },
  {
    icon: FiMonitor,
    label: "Stockage navigateur",
    value: "localStorage pour le jeton et sessionStorage pour les messages temporaires"
  },
  {
    icon: FiLock,
    label: "Traceurs non essentiels",
    value: "Aucun cookie publicitaire ou analytique detecte dans le frontend actuel"
  }
];

const sections = [
  {
    title: "Donnees traitees par la plateforme",
    paragraphs: [
      "Le frontend permet la creation et la gestion d'un compte utilisateur. Les donnees saisies dans les formulaires visibles du depot incluent notamment le prenom, le nom d'utilisateur, l'adresse e-mail, la date de naissance, la biographie et le departement.",
      "Selon les fonctionnalites utilisees, la plateforme traite aussi les contenus produits par l'utilisateur, comme les articles, events, commentaires, reponses, reactions, favoris, mises de cote et suivis."
    ]
  },
  {
    title: "Finalites du traitement",
    paragraphs: [
      "Ces donnees sont necessaires a la creation du compte, a l'authentification, a la restauration de session, a la publication de ressources, a la moderation des espaces d'echange et au suivi des parcours utilisateurs, conformement aux objectifs du cahier des charges.",
      "Le cahier des charges mentionne egalement un espace de statistiques sur la consultation et la creation de ressources. Toute exploitation statistique doit respecter les principes de minimisation, de securisation et, si necessaire, d'anonymisation."
    ]
  },
  {
    title: "Cookies et stockage local",
    paragraphs: [
      "Dans l'etat actuel du frontend, aucun bandeau de consentement n'est necessaire pour des cookies marketing ou analytiques, car aucun traceur non essentiel n'a ete identifie dans le code client.",
      "En revanche, l'application utilise des mecanismes de stockage du navigateur pour faire fonctionner le service."
    ],
    bullets: [
      "localStorage conserve le jeton d'authentification afin de restaurer la session utilisateur",
      "sessionStorage conserve temporairement certains messages de confirmation ou d'erreur entre deux ecrans",
      "ces donnees locales peuvent etre effacees par la deconnexion, par la suppression manuelle du stockage navigateur ou par la fermeture du parcours concerne pour les messages temporaires"
    ]
  },
  {
    title: "Duree de conservation et suppression",
    paragraphs: [
      "Le frontend montre qu'un utilisateur peut modifier son profil et demander la suppression definitive de son compte. En l'etat du projet, les donnees de session stockees localement sont conservees tant qu'elles restent presentes dans le navigateur.",
      "Les durees de conservation serveur, les journaux techniques et les politiques d'archivage devront etre formalises avant tout usage en production reelle."
    ]
  },
  {
    title: "Vos droits",
    paragraphs: [
      "Conformement au RGPD, toute personne concernee doit pouvoir demander l'acces a ses donnees, leur rectification, leur effacement, la limitation du traitement, l'opposition au traitement lorsque cela s'applique, ainsi que la portabilite lorsqu'elle est pertinente.",
      "Pour cette maquette, les demandes doivent etre adressees via la page Contact. Les coordonnees de demonstration devront etre remplacees par un contact verifiable avant diffusion publique."
    ]
  },
  {
    title: "Securite et conformite",
    paragraphs: [
      "Le cahier des charges fixe comme exigences la conformite RGPD, le respect du RGAA, ainsi que l'anonymisation et le chiffrement des donnees sensibles. Ces principes doivent orienter la mise en production, les choix d'hebergement, les habilitations et la moderation."
    ]
  }
];

export function PrivacyCookiesPage() {
  return (
    <InformationPage
      description="Presentation des donnees personnelles manipulees par la maquette, des usages prevus et du stockage local utilise par le frontend."
      eyebrow="RGPD et traceurs"
      note="Cette page decrit le comportement actuellement observable dans le frontend du depot. Les mentions liees aux durees de conservation serveur, aux journaux et a l'hebergement devront etre precisees avant une mise en production."
      sections={sections}
      summaryItems={summaryItems}
      title="Donnees personnelles et cookies"
    />
  );
}
