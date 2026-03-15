import { Box, Text } from "@chakra-ui/react";
import { FiEye, FiLayout, FiNavigation, FiShield } from "react-icons/fi";
import { InformationPage } from "@/shared/ui/site/InformationPage";
import { AccessibilityModeToggle } from "@/shared/ui/site/AccessibilityModeToggle";

const summaryItems = [
  {
    icon: FiEye,
    label: "Vision et lisibilite",
    value: "Mode contraste renforce disponible pour mieux distinguer textes, liens et zones interactives"
  },
  {
    icon: FiNavigation,
    label: "Navigation",
    value: "Parcours clavier, lien d'evitement et focus visibles sur les elements interactifs"
  },
  {
    icon: FiLayout,
    label: "Utilisation simple",
    value: "Interface responsive, structure claire et formulaires avec libelles explicites"
  },
  {
    icon: FiShield,
    label: "Engagement projet",
    value: "Le cahier des charges vise explicitement le respect du RGAA et une meilleure inclusion"
  }
];

const sections = [
  {
    title: "Un site pense pour rester facile a utiliser",
    paragraphs: [
      "Le frontend (RE)Sources Relationnelles doit rester simple a comprendre et a utiliser, y compris pour les personnes qui rencontrent des difficultes visuelles, cognitives, motrices ou de comprehension des interfaces numeriques.",
      "Dans cette maquette, cela se traduit par une navigation structurée, des zones cliquables visibles, des formulaires libelles, une mise en page lisible sur mobile comme sur ordinateur, ainsi qu'un lien d'evitement vers le contenu principal."
    ]
  },
  {
    title: "Ameliorations d'accessibilite deja presentes",
    paragraphs: [
      "Le site comporte deja plusieurs mecanismes utiles a l'accessibilite, en particulier pour les usages clavier ou les personnes ayant besoin de reperes visuels plus nets."
    ],
    bullets: [
      "focus visible sur les elements interactifs",
      "navigation coherente entre l'entete, le contenu principal et le pied de page",
      "libelles explicites sur les champs de formulaires importants",
      "mise en page responsive pour eviter les interfaces trop denses sur petit ecran"
    ]
  },
  {
    title: "Mode contraste renforce",
    paragraphs: [
      "Un toggle de contraste renforce a ete ajoute pour les personnes qui ont des difficultes a distinguer certaines couleurs ou qui ont besoin d'un affichage plus net. Ce mode renforce les contrastes, assombrit les textes utiles et rend les contours plus visibles.",
      "Le reglage reste memorise sur le navigateur afin d'eviter d'avoir a le reactiver a chaque visite."
    ]
  },
  {
    title: "Points a surveiller et amelioration continue",
    paragraphs: [
      "L'accessibilite ne se limite pas aux couleurs. Les prochaines etapes utiles seraient un audit plus large des messages d'erreur, des alternatives textuelles sur toutes les images de contenu, de la coherence des titres de page et des annonces dynamiques pour les lecteurs d'ecran.",
      "Toute difficulte d'usage, de lecture ou de comprehension doit pouvoir etre signalee via la page Contact."
    ]
  }
];

export function AccessibilityPage() {
  return (
    <InformationPage
      description="Engagement d'accessibilite de la maquette et fonctionnalites mises en place pour rendre le site plus lisible et plus simple a utiliser."
      eyebrow="Accessibilite"
      note="Le cahier des charges demande explicitement le respect du RGAA. Cette page explique l'intention du projet et les ajustements concrets deja integres dans le frontend."
      sections={sections}
      summaryItems={summaryItems}
      title="Accessibilite"
      topContent={(
        <Box
          bg="white"
          border="1px solid"
          borderColor="canvas.200"
          borderRadius="16px"
          px={{ base: 5, md: 6 }}
          py={{ base: 4, md: 5 }}
        >
          <AccessibilityModeToggle />
          <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} lineHeight="1.7" mt={4}>
            Activez ce mode si les couleurs actuelles vous semblent difficiles a distinguer ou si vous avez besoin
            d'un affichage plus contraste.
          </Text>
        </Box>
      )}
    />
  );
}
