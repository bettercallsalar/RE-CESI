import { FiAlertCircle, FiBookOpen, FiGlobe, FiShield } from "react-icons/fi";
import { InformationPage } from "@/shared/ui/site/InformationPage";

const summaryItems = [
  {
    icon: FiBookOpen,
    label: "Nature du service",
    value: "Maquette pedagogique du projet (RE)Sources Relationnelles"
  },
  {
    icon: FiGlobe,
    label: "Perimetre",
    value: "Front-office public et espace utilisateur pour articles, events et echanges"
  },
  {
    icon: FiShield,
    label: "Standards vises",
    value: "RGPD, RGAA, anonymisation et chiffrement des donnees sensibles"
  },
  {
    icon: FiAlertCircle,
    label: "Statut institutionnel",
    value: "Service non officiel, inspire d'un cas d'usage ministeriel fictif"
  }
];

const sections = [
  {
    title: "Editeur du site",
    paragraphs: [
      "Le frontend (RE)Sources Relationnelles est realise dans le cadre d'un projet pedagogique CESI. Le cahier des charges fourni precise que le sujet est une simulation de plateforme susceptible d'etre portee par le Ministere des Solidarites et de la Sante, mais que les documents et contenus du projet ne constituent pas un service officiel du ministere.",
      "En l'etat, cette application doit donc etre comprise comme une maquette fonctionnelle et une demonstration technique. Toute mise en production reelle suppose de completer l'identite de l'editeur, les coordonnees de publication et les mentions de responsabilite."
    ]
  },
  {
    title: "Direction de la publication",
    paragraphs: [
      "La direction de publication correspond a l'equipe projet et a ses encadrants pedagogiques pour la phase de demonstration. Les noms, qualites et coordonnees nominatives du responsable de publication doivent etre completes avant toute diffusion publique hors cadre scolaire."
    ]
  },
  {
    title: "Hebergement",
    paragraphs: [
      "Le depot actuel est prevu pour un usage de demonstration locale ou conteneurisee. Le service peut etre lance sur un environnement de developpement Docker, sans engagement d'hebergement permanent.",
      "Avant une mise en ligne publique, l'identite de l'hebergeur, son adresse postale et ses coordonnees de contact doivent etre ajoutees sur cette page."
    ]
  },
  {
    title: "Propriete intellectuelle",
    paragraphs: [
      "Les elements de code, maquettes, interfaces, bases de demonstration, textes et visuels livres dans ce projet sont proteges par le droit applicable a la propriete intellectuelle. Sauf mention contraire, ils sont utilises uniquement pour illustrer la realisation du projet.",
      "Toute reutilisation externe, diffusion integrale ou exploitation commerciale doit etre autorisee par les auteurs ou l'entite qui detient les droits sur les contenus concernes."
    ]
  },
  {
    title: "Accessibilite, donnees et responsabilite",
    paragraphs: [
      "Le cahier des charges impose le respect du RGPD, du RGAA, ainsi que l'anonymisation et le chiffrement des donnees sensibles. Ces exigences guident la conception de la solution, meme si la presente maquette reste un prototype en cours d'evolution.",
      "Les informations proposees sur la plateforme sont fournies a titre informatif dans le cadre du projet. Elles ne constituent ni un conseil medical, ni un engagement institutionnel, ni une publication officielle d'une administration."
    ]
  }
];

export function LegalNoticePage() {
  return (
    <InformationPage
      description="Informations d'edition et de responsabilite de la maquette frontend livree pour le projet (RE)Sources Relationnelles."
      eyebrow="Cadre legal"
      note="Le cahier des charges indique explicitement que le projet est une simulation pedagogique. Cette page reprend donc ce statut et distingue clairement la maquette du vrai site institutionnel qu'elle illustre."
      sections={sections}
      summaryItems={summaryItems}
      title="Mentions legales"
    />
  );
}
