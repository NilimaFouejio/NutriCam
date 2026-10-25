# Rapport d'exécution des tests

Exécutés le 25/10/2026 dans l'environnement de travail (Java 21, sans Maven ni Docker) : les trois classes de tests du moteur
(`CalculEnergetiqueTest`, `CorrespondanceExclusionsTest`, `GenerateurProgrammeTest`) ont été compilées avec le moteur et lancées
par un lanceur minimal reproduisant `@Test` et `Assertions` de JUnit 5 (les fichiers de test sont ceux livrés, sans modification).

**Résultat : 31 tests réussis, 0 échoué.**

| Classe | Tests | Contenu |
|---|---|---|
| CalculEnergetiqueTest | 10 | MR profil 1 = 1 475,25 ; IMC ; facteurs 1,2/1,35/1,55/1,8 ; cible 1 650 ; cible imposée conservée ; répartition arrondie 412/908/330, 625/1375/500, 467/1027/374 ; somme ≠ 100 refusée ; sexe manquant ; **anomalies A-03 et A-04 documentées** |
| CorrespondanceExclusionsTest | 6 | œufs, lait de coco ≠ lait, arachides/pâte d'arachide, viande rouge/bœuf, fruits de mer, termes libres |
| GenerateurProgrammeTest | 15 | 3 profils de référence × 7 jours × 3 repas ; totaux à ±1 % de la cible ; petit-déjeuner (jus/smoothie) ; pas de jus ni smoothie au déjeuner/dîner ; dîner sans féculent ; sous-totaux cohérents ; reproductibilité ; facteur d'échelle ≤ ±2 % du document ; allergie stricte ; aliment à éviter = avertissement ; mineur ; IMC bas ; objectif impossible ; utilisateur générique ; **anomalie A-01 documentée** |

**Non exécutés** : `ParcoursDemoTest` (Docker/Testcontainers), compilation Maven du backend complet, migrations Flyway V3–V5 sur MariaDB, compilation du mobile.
Pour tout relancer : `cd backend/nutricam-backend && mvn test` (Docker démarré).
