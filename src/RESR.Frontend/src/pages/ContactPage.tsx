import { FiAlertCircle, FiGlobe, FiMail, FiPhone } from "react-icons/fi";
import { InformationPage } from "@/shared/ui/site/InformationPage";

const summaryItems = [
  {
    icon: FiPhone,
    label: "Standard ministeriel",
    value: "14 avenue Duquesne, 75350 Paris 07 SP · 01 40 56 60 00"
  },
  {
    icon: FiMail,
    label: "Saisine electronique",
    value: "sve.social-sante.gouv.fr"
  },
  {
    icon: FiGlobe,
    label: "Site et accessibilite",
    value: "solidarites.gouv.fr/contact-webmestre"
  },
  {
    icon: FiAlertCircle,
    label: "Donnees personnelles",
    value: "dgs-rgpd@sante.gouv.fr"
  }
];

const sections = [
  {
    title: "Pourquoi nous contacter",
    paragraphs: [
      "La page Contact centralise les points d'entree utiles pour un usager qui souhaite joindre l'administration, signaler une difficulte d'acces au site ou exercer des droits relatifs aux donnees personnelles.",
      "Les coordonnees affichees ci-dessus reprennent des contacts institutionnels officiels du ministere et de ses sites de reference. La maquette reste toutefois un projet pedagogique et ne constitue pas un service officiel de l'Etat."
    ],
    bullets: [
      "demande d'information generale aupres du ministere",
      "saisine administrative par voie electronique",
      "signalement d'un probleme technique ou d'accessibilite sur le site",
      "demande liee a l'exercice de droits sur les donnees personnelles"
    ]
  },
  {
    title: "Contacts institutionnels de reference",
    paragraphs: [
      "Les coordonnees retenues correspondent a des canaux publics officiellement exposes par les sites gouvernementaux lies aux solidarites et a la sante. Elles offrent un point d'appui plus realiste que les adresses fictives precedemment affichees."
    ],
    bullets: [
      "standard du ministere : 01 40 56 60 00",
      "saisine par voie electronique : sve.social-sante.gouv.fr",
      "formulaire webmestre et accessibilite : solidarites.gouv.fr/contact-webmestre",
      "contact donnees personnelles : dgs-rgpd@sante.gouv.fr"
    ]
  },
  {
    title: "Signalement et accessibilite",
    paragraphs: [
      "Le site solidarites.gouv.fr met a disposition un formulaire webmestre pour les observations portant sur l'accessibilite, la gestion administrative ou technique du site. C'est le canal institutionnel le plus proche d'un signalement technique ou d'un besoin d'assistance sur le web.",
      "Pour un signalement efficace, il reste utile de preciser l'URL concernee, la nature du probleme constate, le navigateur utilise et toute information permettant de reproduire la difficulte."
    ]
  },
  {
    title: "Accessibilite et protection des donnees",
    paragraphs: [
      "Les demandes relatives au RGAA, aux difficultes d'usage et a la protection des donnees doivent rester clairement identifiables. Le cahier des charges vise explicitement la conformite RGPD et l'amelioration de l'accessibilite.",
      "Pour les donnees personnelles, l'adresse institutionnelle mise en avant sur des pages officielles du ministere est dgs-rgpd@sante.gouv.fr. En cas de litige non resolu, une reclamation peut egalement etre adressee a la CNIL."
    ]
  }
];

export function ContactPage() {
  return (
    <InformationPage
      description="Coordonnees institutionnelles utiles pour joindre le ministere, utiliser les canaux publics de contact et identifier un point d'entree RGPD."
      eyebrow="Nous contacter"
      note="Cette page affiche des contacts institutionnels officiels en lien avec les sites gouvernementaux de reference. Leur presence n'a pas pour effet de transformer la maquette en service officiel du gouvernement."
      sections={sections}
      summaryItems={summaryItems}
      title="Contact"
    />
  );
}
