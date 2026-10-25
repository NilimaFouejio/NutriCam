-- Ajouts au schéma NutriCam requis par le contrat nutricam-openapi.yaml.
-- Écrit pour MySQL 8.0.16+ / MariaDB 10.2+ (identifiants en VARCHAR(36), comme la table objectifs).
-- Les informations ci-dessous sont déjà saisies dans l'application mais absentes des tables.

ALTER TABLE profils ADD COLUMN sexe VARCHAR(10) NULL;
ALTER TABLE profils ADD CONSTRAINT chk_profils_sexe CHECK (sexe IN ('femme', 'homme'));
ALTER TABLE profils ADD COLUMN repas_par_jour INT NULL;
ALTER TABLE profils ADD CONSTRAINT chk_profils_repas CHECK (repas_par_jour BETWEEN 2 AND 5);

ALTER TABLE aliments_a_eviter ADD COLUMN motif VARCHAR(20) NOT NULL DEFAULT 'preference';
ALTER TABLE aliments_a_eviter ADD CONSTRAINT chk_evitement_motif
    CHECK (motif IN ('preference', 'allergie', 'intolerance', 'autre'));

ALTER TABLE recettes ADD COLUMN description TEXT NULL;
ALTER TABLE recettes ADD COLUMN preparation TEXT NULL;

ALTER TABLE repas_programme ADD COLUMN est_realise BOOLEAN NOT NULL DEFAULT FALSE;

-- Un seul objectif courant par profil : PUT /profils/moi met à jour la ligne existante.
-- (La table objectifs n'a pas de date : sans cette contrainte, « l'objectif courant » serait ambigu.)
ALTER TABLE objectifs ADD CONSTRAINT ux_objectifs_profil UNIQUE (id_profil);

-- Idempotence de POST /profils/moi/programmes (plusieurs NULL autorisés par l'index unique)
ALTER TABLE programmes_alimentaires ADD COLUMN cle_idempotence VARCHAR(36) NULL;
ALTER TABLE programmes_alimentaires ADD CONSTRAINT ux_programme_cle UNIQUE (id_profil, cle_idempotence);

-- Convention de valeurs pour aliments.categorie : 'legume', 'proteine', 'feculent', 'autre'.
-- Pour les UUID : les générer côté serveur (UUID() en MySQL, uuid en code) avant l'INSERT.
