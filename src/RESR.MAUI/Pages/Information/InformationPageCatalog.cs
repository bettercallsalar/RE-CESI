namespace RESR.MAUI.Pages.Information;

internal static class InformationPageCatalog
{
    public static InformationPageDefinition GetPage(string? key)
    {
        return key switch
        {
            "contact" => ContactPage,
            "donnees-personnelles-cookies" => PrivacyCookiesPage,
            "conditions-generales-utilisation" => TermsOfUsePage,
            _ => LegalNoticePage
        };
    }

    private static readonly InformationPageDefinition LegalNoticePage = new(
        "mentions-legales",
        "Mentions legales",
        "Informations relatives a l'edition, a l'exploitation et au cadre general du service (RE) Sources Relationnelles.",
        "15/03/2026",
        [
            new InformationSectionDefinition(
                "Editeur du service",
                [
                    "Le service numerique (RE) Sources Relationnelles est exploite pour le compte du ministere des Solidarites et de la Sante.",
                    "Il a pour finalite de mettre a disposition des usagers un espace de consultation, de partage et d'interaction autour de ressources relationnelles."
                ]),
            new InformationSectionDefinition(
                "Direction de publication et exploitation",
                [
                    "La publication, l'administration fonctionnelle et l'exploitation technique du service sont assurees sous l'autorite de l'editeur et, le cas echeant, de ses prestataires habilites.",
                    "Les references operationnelles utiles a la publication, a l'hebergement et au support du service sont gerees dans le cadre de la documentation d'exploitation."
                ]),
            new InformationSectionDefinition(
                "Hebergement et securite",
                [
                    "Le service est heberge sur une infrastructure securisee repondant aux exigences techniques, contractuelles et reglementaires applicables.",
                    "L'exploitation integre des mecanismes de supervision, de journalisation, de sauvegarde et de maintien en condition de securite adaptes a la nature des traitements realises."
                ]),
            new InformationSectionDefinition(
                "Propriete intellectuelle",
                [
                    "La structure generale du service, les contenus publies, les elements graphiques, les marques, les illustrations, les composants logiciels et, plus generalement, les elements accessibles depuis la plateforme sont proteges par les regles applicables en matiere de propriete intellectuelle.",
                    "Sauf mention contraire, toute reproduction, representation, adaptation ou reutilisation, totale ou partielle, sans autorisation prealable, expresse et ecrite, est interdite."
                ]),
            new InformationSectionDefinition(
                "Responsabilite",
                [
                    "L'editeur met en oeuvre les moyens raisonnables pour assurer l'exactitude des informations diffusees et la disponibilite du service.",
                    "Il ne saurait toutefois garantir l'absence totale d'erreur, d'interruption, d'indisponibilite temporaire ou de defaut de fonctionnement, notamment lors des operations de maintenance, de mise a jour ou en cas d'evenement exterieur."
                ]),
            new InformationSectionDefinition(
                "Accessibilite",
                [
                    "Le service est concu pour respecter les exigences d'accessibilite applicables, notamment les principes du RGAA et les bonnes pratiques de conception inclusive.",
                    "Les signalements relatifs a l'accessibilite peuvent etre transmis par le canal de contact du service afin de permettre leur prise en compte et leur traitement."
                ])
        ]);

    private static readonly InformationPageDefinition ContactPage = new(
        "contact",
        "Contact",
        "Informations de contact, d'assistance et d'orientation des demandes relatives au service.",
        "15/03/2026",
        [
            new InformationSectionDefinition(
                "Objet de la page",
                [
                    "Cette page a vocation a orienter les usagers vers le bon circuit de prise en charge selon la nature de leur demande.",
                    "Les sollicitations adressees au service sont traitees par les equipes habilitees chargees du support, de la moderation, de l'exploitation ou de la protection des donnees."
                ]),
            new InformationSectionDefinition(
                "Demandes prises en charge",
                [
                    "Le service prend en charge les demandes liees a son fonctionnement, a l'utilisation du compte, a la moderation des contenus, a l'accessibilite et aux donnees personnelles."
                ],
                [
                    "assistance a l'utilisation du service et du compte utilisateur",
                    "signalement d'un contenu, d'un commentaire ou d'un comportement non conforme",
                    "question relative a l'accessibilite numerique ou a l'ergonomie du service",
                    "demande portant sur les donnees personnelles ou l'exercice de vos droits"
                ]),
            new InformationSectionDefinition(
                "Modalites de traitement",
                [
                    "Les demandes sont analysees selon leur objet et orientees vers l'equipe competente afin d'assurer une reponse adaptee et proportionnee.",
                    "En cas de sujet technique ou fonctionnel complexe, le delai de traitement peut etre ajuste afin de permettre les verifications necessaires."
                ]),
            new InformationSectionDefinition(
                "Protection des donnees et accessibilite",
                [
                    "Les demandes relatives aux donnees personnelles sont traitees dans le respect de la reglementation applicable et selon les procedures internes du service.",
                    "Les retours portant sur l'accessibilite, la comprehension des interfaces ou les difficultes d'usage sont pris en compte dans la demarche d'amelioration continue du service."
                ])
        ]);

    private static readonly InformationPageDefinition PrivacyCookiesPage = new(
        "donnees-personnelles-cookies",
        "Donnees personnelles et cookies",
        "Informations relatives aux traitements de donnees personnelles, a la securite du service et a l'usage des cookies et traceurs.",
        "15/03/2026",
        [
            new InformationSectionDefinition(
                "Principes generaux",
                [
                    "Le service met en oeuvre des traitements de donnees personnelles strictement necessaires a son fonctionnement, a la gestion des comptes utilisateurs, a la publication des contenus et a la securisation des usages.",
                    "Les traitements sont concus dans une logique de minimisation des donnees, de proportionnalite et de protection de la vie privee."
                ]),
            new InformationSectionDefinition(
                "Finalites des traitements",
                [
                    "Les donnees peuvent etre traitees pour permettre l'acces au service, la gestion du profil utilisateur, la publication de ressources, la moderation, la gestion des commentaires, des favoris, des reactions et le suivi de l'activite du service."
                ],
                [
                    "creer, administrer et securiser un compte utilisateur",
                    "publier, consulter et moderer des ressources et des commentaires",
                    "personnaliser l'experience de consultation via les favoris, reactions et listes de suivi",
                    "produire des indicateurs utiles a l'exploitation et a l'amelioration du service"
                ]),
            new InformationSectionDefinition(
                "Categories de donnees",
                [
                    "Selon les usages, le service peut traiter des donnees d'identification, des donnees de profil, des contenus publies par les utilisateurs, des informations de navigation necessaires a la session ainsi que des donnees techniques liees a la securite et a l'exploitation.",
                    "Aucune donnee bancaire n'entre dans le perimetre fonctionnel du service."
                ]),
            new InformationSectionDefinition(
                "Securite",
                [
                    "Le service met en oeuvre des mesures techniques et organisationnelles visant a garantir la confidentialite, l'integrite, la disponibilite et la tracabilite des donnees traitees.",
                    "Les mots de passe ne sont pas conserves en clair et font l'objet de mecanismes de protection adaptes avant leur stockage.",
                    "Les acces aux donnees sont limites aux seules personnes habilitees dans le cadre de leurs missions."
                ]),
            new InformationSectionDefinition(
                "Conservation, destinataires et transferts",
                [
                    "Les donnees sont conservees pour une duree proportionnee aux finalites poursuivies, aux contraintes de securite, aux obligations legales et aux besoins de preuve ou de gestion du service.",
                    "Les donnees peuvent etre communiquees aux personnels habilites, aux outils techniques necessaires au fonctionnement du service et, le cas echeant, aux prestataires autorises intervenant dans un cadre contractuel securise.",
                    "Tout transfert hors de l'Union europeenne, s'il devait etre mis en oeuvre, ferait l'objet des garanties appropriees prevues par la reglementation applicable."
                ]),
            new InformationSectionDefinition(
                "Droits des personnes",
                [
                    "Conformement a la reglementation applicable, vous pouvez demander l'acces, la rectification, l'effacement, la limitation ou, selon les cas, l'opposition au traitement de vos donnees personnelles.",
                    "Vous pouvez egalement introduire une reclamation aupres de la CNIL si vous estimez que vos droits ne sont pas respectes."
                ]),
            new InformationSectionDefinition(
                "Gestion des incidents",
                [
                    "En cas de violation de donnees personnelles susceptible d'engendrer un risque pour les droits et libertes des personnes, le service applique les procedures de gestion d'incident prevues par la reglementation.",
                    "Lorsque cela est requis, la violation est notifiee a la CNIL dans les meilleurs delais et, si possible, dans un delai de 72 heures apres en avoir pris connaissance. Si le risque est eleve, les personnes concernees sont egalement informees sans delai inutile."
                ]),
            new InformationSectionDefinition(
                "Cookies et traceurs",
                [
                    "Le service peut utiliser des cookies ou des mecanismes de stockage local strictement necessaires a son fonctionnement, a l'authentification et au maintien de la session utilisateur.",
                    "Tout traceur non strictement necessaire, notamment de mesure d'audience, de personnalisation avancee ou de partage vers des services tiers, est soumis aux regles applicables et, lorsqu'il y a lieu, au recueil prealable du consentement."
                ])
        ]);

    private static readonly InformationPageDefinition TermsOfUsePage = new(
        "conditions-generales-utilisation",
        "Conditions generales d'utilisation",
        "Conditions applicables a l'utilisation du service, aux comptes utilisateurs et aux contenus publies.",
        "15/03/2026",
        [
            new InformationSectionDefinition(
                "Objet",
                [
                    "Les presentes conditions generales d'utilisation ont pour objet de definir les modalites d'acces et d'usage du service (RE) Sources Relationnelles.",
                    "Toute utilisation du service implique l'acceptation des regles ci-dessous, dans le respect du cadre legal et reglementaire applicable."
                ]),
            new InformationSectionDefinition(
                "Acces au service",
                [
                    "Le service distingue des fonctionnalites accessibles sans authentification et des fonctionnalites reservees aux utilisateurs disposant d'un compte et, le cas echeant, d'un role specifique.",
                    "La consultation, la recherche et le filtrage des ressources publiques peuvent etre ouverts au public. La publication de contenus, la gestion du profil, les reactions, les favoris, les commentaires et les fonctions de moderation sont reserves aux utilisateurs autorises."
                ]),
            new InformationSectionDefinition(
                "Compte utilisateur",
                [
                    "L'utilisateur s'engage a fournir des informations exactes, a jour et completes lors de la creation et de la gestion de son compte.",
                    "Il est responsable de la confidentialite de ses identifiants et de toute utilisation effectuee depuis son compte, sauf preuve d'un usage frauduleux ne lui etant pas imputable."
                ]),
            new InformationSectionDefinition(
                "Engagements des utilisateurs",
                [
                    "L'utilisateur s'engage a utiliser le service de bonne foi, dans le respect de sa finalite, des lois en vigueur, des droits des tiers et des regles elementaires de civilite numerique.",
                    "Les contenus diffuses via le service doivent etre loyaux, licites, pertinents et compatibles avec l'objet de la plateforme."
                ],
                [
                    "ne pas publier de contenus illicites, violents, diffamatoires ou discriminatoires",
                    "ne pas usurper l'identite d'un tiers",
                    "ne pas perturber le fonctionnement normal du service",
                    "respecter les decisions de moderation et les restrictions liees a son role"
                ]),
            new InformationSectionDefinition(
                "Moderation et gestion des contenus",
                [
                    "L'auteur d'un contenu reste responsable des informations qu'il publie. Selon les droits associes a son role, il peut gerer ses propres ressources et commentaires dans les limites offertes par le service.",
                    "Le service peut mettre en oeuvre des operations de moderation, de validation, de retrait, de suspension ou de suppression lorsqu'un contenu est contraire aux regles applicables, a la finalite du service ou aux obligations de securite."
                ]),
            new InformationSectionDefinition(
                "Disponibilite du service",
                [
                    "L'editeur s'efforce d'assurer un niveau de disponibilite et de performance compatible avec les besoins du service, sans garantie d'absence totale d'interruption ou d'anomalie.",
                    "Le service peut etre suspendu, limite ou adapte temporairement pour des raisons de maintenance, de securite, d'evolution technique ou de conformite."
                ]),
            new InformationSectionDefinition(
                "Evolution des conditions",
                [
                    "Les presentes conditions peuvent etre modifiees a tout moment afin de tenir compte des evolutions du service, de la reglementation ou des contraintes d'exploitation.",
                    "La version en vigueur est celle publiee sur le service a la date de consultation."
                ]),
            new InformationSectionDefinition(
                "Donnees personnelles et droit applicable",
                [
                    "L'utilisation du service implique le traitement de certaines donnees personnelles dans les conditions decrites sur la page Donnees personnelles et cookies.",
                    "Les presentes conditions sont soumises au droit francais."
                ])
        ]);
}

internal sealed record InformationPageDefinition(
    string Key,
    string Title,
    string Description,
    string UpdatedAt,
    IReadOnlyList<InformationSectionDefinition> Sections);

internal sealed record InformationSectionDefinition(
    string Title,
    IReadOnlyList<string> Paragraphs,
    IReadOnlyList<string>? Bullets = null);
