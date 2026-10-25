# Catalogue des règles métier (génération du programme)

Sources : `Fiches_pratiques_3_profils_dietetiques_25_55_20.docx` (« Fiches »), `plan_nutritionnel_hebdomadaire_3_profils_modifie.docx` (« Plan »), image « 5.2 Estimer la dépense énergétique totale » (« Tableau 5.2 »).
Statut : **E** = écrite dans un document, **D** = déduite d'une donnée, **H** = hypothèse à valider par le responsable métier.
Les numéros R-xx / H-xx sont repris dans les commentaires du code (`generation/`) et dans les tests.

## Énergie
| N° | Règle | Source | Statut |
|---|---|---|---|
| R-01 | MR homme = 10·P + 6,25·T − 5·A + 5 ; MR femme = 10·P + 6,25·T − 5·A − 161 (P kg, T cm, A ans, kcal/j). Non arrondi. | Fiches (Mifflin–St Jeor) | E |
| R-02 | BET = MR × facteur d'activité. | Fiches, Tableau 5.2 | E |
| H-01 | Facteurs du Tableau 5.2 donnés en fourchettes ; facteur retenu = milieu : très sédentaire 1,2 ; légèrement actif 1,35 ; modérément actif 1,55 ; très actif 1,8. | Tableau 5.2 | H (1,35 et 1,55 coïncident avec les fiches 1 et 2) |
| R-03 | Cible = BET × (1 + ajustement), arrondie au pas configuré (HALF_EVEN). | Fiches | D |
| H-02 | Ajustement : perte −17 %, prise +11 %, maintien 0 % (déduits des exemples 340 kcal / 1 991 et 250 kcal / 2 246). Aucune règle générale n'est écrite. Configurable (`nutricam.generation.ajustement-*`). | Fiches | H |
| H-03 | Pas d'arrondi de la cible journalière : 50 kcal (configurable). | — | H |
| R-04 | Cible d'un repas = cible journalière × 25 % / 55 % / 20 % ; la somme des pourcentages est contrôlée (= 100). | Fiches | E |
| R-05 | Cibles de repas affichées dans les documents : arrondi HALF_EVEN à l'entier, écart reporté sur le déjeuner (412,5→412 ; 907,5→908). Le calcul utilise les cibles exactes, non arrondies. | Fiches/Plan | D |

## Composition et quantités
| N° | Règle | Source | Statut |
|---|---|---|---|
| R-06 | 3 repas par jour (petit-déjeuner, déjeuner, dîner), sans collation, 7 jours. Le nombre de repas n'est plus choisi par l'utilisateur. | Fiches, demande | E |
| R-07 | Petit-déjeuner : jus de légumes (perte, maintien = profils 1 et 3), smoothie haute énergie (prise = profil 2). L'objectif est le critère d'association profil ↔ stratégie. | Plan | E + H (association par objectif) |
| R-08 | Déjeuner et dîner : jamais de jus ni de smoothie. | Plan | E |
| R-09 | Dîner : aucun féculent d'accompagnement. | Plan | E |
| R-10 | Une recette est adaptée à la cible par mise à l'échelle **proportionnelle** de tous ses ingrédients : facteur = cible du repas ÷ énergie de la variante de l'objectif. Limites du facteur : 0,5 à 2,0, sinon `ECHEC_VALIDATION_PROGRAMME`. Limites : huile, œufs entiers, etc. ne sont pas des quantités indivisibles dans ce modèle. | Plan (variantes) | H-06 |
| R-11 | Quantités arrondies à 0,1 g (HALF_EVEN). | — | H |
| R-12 | Énergie d'une ligne = quantité arrondie × densité (kcal/g), arrondie à 0,01 kcal ; repas = somme des lignes ; jour = somme des 3 repas. | Plan, énoncé | E/D |
| R-13 | Les menus des 7 jours viennent du plan de référence (mêmes plats pour les 3 objectifs, quantités propres à chaque profil). Aucune recette n'est inventée. | Plan | E |

## Exclusions et prudence
| N° | Règle | Statut |
|---|---|---|
| R-20 | Allergie ou intolérance : exclusion stricte, y compris ingrédients indirects (liste explicite par terme proposé par l'appli). Si une recette du plan est concernée, aucun programme (`AUCUN_PROGRAMME_COMPATIBLE`) : pas de recette de remplacement inventée. « À éviter » (préférence, autre) : avertissement. | H (liste d'ingrédients indirects à valider) |
| H-04 | Perte de poids refusée si IMC < 18,5 ; prise de poids avec avertissement si IMC < 18,5 (prudence des Fiches). | H |
| H-05 | Programme réservé aux adultes (≥ 18 ans) : les documents ne couvrent que des adultes. | H |
| R-21 | Budget, aliments préférés et disponibles : enregistrés, mais aucune règle des documents ne les utilise. | — |

## Contrôles automatiques
| N° | Contrôle (code) | Type |
|---|---|---|
| R-30 | `TROIS_REPAS_PAR_JOUR` | bloquant |
| R-31/32 | `QUANTITES_POSITIVES`, `DENSITES_VALIDES` | bloquant |
| R-33/34/35 | `ENERGIE_INGREDIENTS`, `SOUS_TOTAUX_REPAS`, `TOTAUX_JOURNALIERS` | bloquant |
| R-36 | `REGLE_PETIT_DEJEUNER`, `REGLE_PAS_JUS_DEJEUNER_DINER`, `REGLE_DINER_SANS_FECULENT` | bloquant |
| R-37 | `ECART_CIBLE_REPAS` (tolérance 2 %) | avertissement |
| R-38 | `ECART_CIBLE_JOUR` (tolérance 1 %) | avertissement |
| R-39 | `REPARTITION_100_POURCENT`, `ALLERGIES_EXCLUES`, `HYPOTHESE_AJUSTEMENT_OBJECTIF` | info / avertissement |
