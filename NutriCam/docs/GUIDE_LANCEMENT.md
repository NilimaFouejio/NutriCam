# NutriCam : connecter le mobile au backend et lancer le projet sur votre ordinateur

## 0. Ce qu'il faut installer (une seule fois)
| Outil | Pourquoi |
|---|---|
| **Docker Desktop** | lance MariaDB et l'API sans rien installer d'autre |
| **.NET 9 SDK** + charge de travail MAUI (`dotnet workload install maui`) ou **Visual Studio 2022 17.12+** avec « Développement d'applications mobiles .NET » | compile l'appli |
| **Android Studio** (émulateur Android) ou le mode Windows | exécute l'appli |
| JDK 21 + Maven (facultatif) | seulement pour lancer les tests du backend sans Docker |

## 1. Lancer le backend
```bash
cd backend/nutricam-backend
copy .env.example .env          # Windows  (Linux/Mac : cp .env.example .env)
```
Ouvrez `.env` et remplacez les trois mots de passe / le secret :
```
MARIADB_ROOT_PASSWORD=un-mot-de-passe-root
DB_PASSWORD=un-mot-de-passe-base
NUTRICAM_JWT_SECRET=une-longue-phrase-secrete-d-au-moins-32-caracteres
```
Puis :
```bash
docker compose up --build
```
Au premier démarrage, Flyway exécute V1 à V5 (tables, données de démonstration, structures de génération, plan de 7 jours).
Vérification : ouvrez http://localhost:8080/actuator/health → `{"status":"UP"}`.
Si le démarrage échoue, lisez la dernière erreur Flyway/Spring dans la console et transmettez-la (voir `ETAT_AVANCEMENT_ET_RESTE_A_FAIRE.md`, § « À vérifier en premier »).

## 2. Connecter l'appli au backend
L'appli est déjà branchée sur l'API (`Api/ApiSettings.cs`). Seule l'adresse change selon l'endroit où elle tourne :

| Où tourne l'appli | Adresse utilisée | Que faire |
|---|---|---|
| Émulateur Android | `http://10.0.2.2:8080/api/v1/` (10.0.2.2 = votre ordinateur) | rien |
| Windows (cible `net9.0-windows…`) | `http://localhost:8080/api/v1/` | rien |
| Téléphone Android réel | `http://<IP de votre PC>:8080/api/v1/` | dans `ApiSettings.cs`, mettre `DevHost = "192.168.1.20"` (votre IP : `ipconfig`), PC et téléphone sur le même Wi-Fi, autoriser le port 8080 dans le pare-feu Windows |

Le manifeste Android autorise maintenant le HTTP non chiffré (`usesCleartextTraffic`) : **à retirer en production** (utiliser HTTPS).

## 3. Lancer l'appli
```bash
cd mobile/NutriCam
dotnet restore
dotnet build -t:Run -f net9.0-android                      # émulateur Android démarré
dotnet build -t:Run -f net9.0-windows10.0.19041.0          # Windows (mode développeur activé)
```
Ou, dans Visual Studio, ouvrez `NutriCam.csproj`, choisissez la plateforme dans la barre d'outils, puis **F5**.
(Si le projet ne s'ouvre pas tel quel, suivez `INTEGRATION.md` pour le copier dans un projet MAUI neuf.)

## 4. Vérifier le parcours
1. Accueil → créer un compte (le compte est créé sur le serveur).
2. Étape 1 : âge, taille, poids, sexe, **niveau d'activité (4 choix)** ; plus de choix du nombre de repas (3 repas par jour).
3. Étapes 2 à 4 : objectif, goûts, budget.
4. Récapitulatif → « Générer mon programme » : le serveur calcule la cible énergétique et compose les 7 jours.
5. Fermer et rouvrir l'appli : compte et programme conservés. Se connecter sur un autre appareil : le profil est restauré depuis le serveur.

Essai direct de l'API (sans l'appli) : voir `backend/nutricam-backend/README.md`, section « Essai rapide » (utiliser `"niveau_activite":"moderement_actif"`).

## 5. Les tests
```bash
cd backend/nutricam-backend
mvn test        # tests du moteur (sans Docker) + ParcoursDemoTest (Docker démarré obligatoire)
```
