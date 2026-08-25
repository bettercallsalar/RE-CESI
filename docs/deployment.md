# Déploiement Azure de RE-CESI

## Architecture retenue

RE-CESI reprend la topologie éprouvée dans CESIZen : une VM Linux Azure économique exécute Docker Compose, avec un conteneur par responsabilité.

| Composant            | Conteneur         | Exposition               | Persistance                       |
| -------------------- | ----------------- | ------------------------ | --------------------------------- |
| Frontend React/Nginx | `recesi-frontend` | Port public 80           | Image immuable                    |
| API ASP.NET          | `recesi-api`      | Réseau Docker uniquement | Volume des fichiers envoyés       |
| Migrations Flyway    | `recesi-migrate`  | Aucune                   | Image immuable des migrations SQL |
| MySQL                | `recesi-mysql`    | Réseau Docker uniquement | Volume de données MySQL           |

Nginx sert l'application monopage et transmet `/api/*` et `/uploads/*` à l'API. Le frontend et le backend restent des images distinctes, comme dans CESIZen, tout en étant servis sous la même origine publique.

L'infrastructure utilise `polandcentral` et la taille `Standard_B2ls_v2`, identiques à CESIZen. Le disque système est un SSD standard pour limiter le coût de l'abonnement étudiant.

## Environnements

| Branche | Environnement       | État Terraform                  | Préfixe Azure | Déploiement |
| ------- | ------------------- | ------------------------------- | ------------- | ----------- |
| `main`  | Production          | `recesi-prod.terraform.tfstate` | `recesi-prod` | Autorisé    |
| `dev`   | Développement prévu | `recesi-dev.terraform.tfstate`  | `recesi-dev`  | Interdit    |

Les variables `dev.tfvars` permettent de démontrer la séparation des environnements et de produire un plan. Le workflow bloque structurellement l'`apply` de `dev`, et le workflow applicatif ne déploie que les images construites depuis `main`.

## Ressources Azure de production

Terraform crée :

- un groupe de ressources ;
- un réseau virtuel, un sous-réseau, une interface réseau et un NSG ;
- une IP publique statique avec le DNS `recesi.polandcentral.cloudapp.azure.com` ;
- une VM Ubuntu 22.04 avec authentification SSH par clé uniquement ;
- une extension Azure qui installe Docker et Docker Compose ;
- un budget annuel avec alertes à 25, 50, 75 et 90 %.

Seul le port HTTP 80 est public. SSH reste fermé tant que `admin_ssh_source_cidr` n'est pas défini ; GitHub Actions administre la VM avec Azure Run Command. MySQL et l'API ne publient aucun port sur Internet.

## État Terraform distant

Le backend est séparé des ressources applicatives :

```text
Resource group: recesi-tfstate-group
Storage account: recesitfstate418cec1d
Container: tfstate
```

Depuis la session Azure CLI déjà connectée :

```bash
./scripts/azure/bootstrap-terraform-backend.sh
```

Le script active TLS 1.2 minimum, désactive l'accès public aux blobs, désactive les clés partagées et active la rétention ainsi que le versionnement.

## Configuration GitHub

Créer les variables de dépôt suivantes :

```text
AZURE_INFRA_CLIENT_ID
AZURE_DEPLOY_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
TF_ADMIN_SSH_PUBLIC_KEY
TF_BUDGET_CONTACT_EMAILS
```

`TF_BUDGET_CONTACT_EMAILS` doit être une liste JSON Terraform, par exemple `["adresse@example.com"]` sur une seule ligne.

Créer deux environnements GitHub protégés par validation manuelle :

```text
infrastructure-production
application-production
```

Ajouter ces secrets dans `application-production` :

```text
MYSQL_PASSWORD
MYSQL_ROOT_PASSWORD
JWT_SECRET
```

`JWT_SECRET` doit contenir au moins 32 caractères aléatoires.

## Authentification Azure OIDC

Deux identités Azure sont recommandées afin de séparer l'infrastructure du déploiement applicatif :

- l'identité infrastructure possède `Contributor` sur l'abonnement et `Storage Blob Data Contributor` sur le compte d'état Terraform ;
- l'identité de déploiement possède `Virtual Machine Contributor` uniquement sur `recesi-prod-group` après sa création.

Ajouter les identifiants fédérés suivants aux applications Entra ID :

```text
# Identité infrastructure
repo:bettercallsalar/RE-CESI:ref:refs/heads/main
repo:bettercallsalar/RE-CESI:ref:refs/heads/dev
repo:bettercallsalar/RE-CESI:environment:infrastructure-production

# Identité de déploiement
repo:bettercallsalar/RE-CESI:environment:application-production
```

L'audience est `api://AzureADTokenExchange` et l'émetteur est `https://token.actions.githubusercontent.com`. Aucun secret client Azure n'est stocké dans GitHub.

## Pipeline plan/apply

Le workflow `Terraform Infrastructure` effectue systématiquement `fmt`, `init` et `validate` sur les pull requests qui modifient l'infrastructure.

Pour contrôler la production :

1. lancer manuellement le workflow depuis `main` avec `environment=prod` et `apply=false` ;
2. télécharger et lire `recesi-plan.txt` ;
3. vérifier que les ressources ciblent uniquement `recesi-prod-*` ;
4. relancer depuis le même commit avec `environment=prod` et `apply=true` ;
5. approuver l'environnement `infrastructure-production`.

Chaque exécution qui applique produit son propre plan puis applique exactement le fichier binaire généré dans cette exécution. Un plan `dev` peut être généré depuis `dev`, mais aucun job d'`apply` ne peut suivre.

## Pipeline applicatif

Un push sur `main` ou `dev` construit trois images immuables dans GHCR : API, frontend et migrations. Chaque image reçoit le SHA Git complet et un alias `<branche>-latest`.

Après une construction réussie de `main` uniquement :

1. GitHub s'authentifie dans Azure par OIDC ;
2. Azure Run Command envoie le Compose vérifié par SHA-256 à la VM ;
3. la VM récupère les images du commit ;
4. Flyway applique les migrations avant le démarrage de l'API ;
5. Docker Compose remplace les conteneurs ;
6. un smoke test vérifie les services et la réponse HTTP ;
7. les ressources Run Command temporaires sont supprimées.

L'environnement `application-production` doit imposer une approbation pour garder le déploiement de production manuel malgré l'enchaînement automatique des jobs.

## Première mise en production

1. Créer le backend Terraform avec le script local.
2. Créer les identités OIDC et les variables GitHub.
3. Fusionner l'infrastructure validée jusqu'à `main`.
4. Exécuter et examiner le plan Terraform de production.
5. Appliquer le plan de production après approbation.
6. Attribuer `Virtual Machine Contributor` à l'identité de déploiement sur `recesi-prod-group`.
7. Créer l'environnement et les secrets `application-production`.
8. Relancer `Build & Push Docker Images` depuis `main`.
9. Approuver le déploiement et vérifier le nom DNS retourné par Terraform.

## Retour arrière et maintenance

Pour revenir à une version précédente, relancer le déploiement avec le SHA validé des trois images. Les données MySQL et les fichiers envoyés sont conservés dans des volumes Docker. Une migration destructive nécessite une procédure de restauration MySQL testée avant le déploiement.

Les demandes d'évolution et incidents sont suivis dans GitHub Issues. Un incident de production suit le cycle : qualification, priorité, affectation, correction sur branche dédiée, revue, validation, déploiement puis compte rendu. Un incident de sécurité impose en plus la rotation immédiate des secrets concernés et la conservation des journaux Azure/GitHub nécessaires à l'analyse.

Cette première version expose HTTP uniquement, comme CESIZen. Avant une ouverture réelle au public, ajouter un nom de domaine et un certificat TLS géré, puis fermer le port 80 ou le réserver à la redirection HTTPS.
