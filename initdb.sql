-- Adminer 5.4.1 PostgreSQL 18.0 dump

DROP DATABASE IF EXISTS "hizkitza";
CREATE DATABASE "hizkitza";
\connect "hizkitza";

CREATE TYPE "erabiltzailemota" AS ENUM ('admin', 'user');

DROP TABLE IF EXISTS "Erabiltzaileak";
DROP SEQUENCE IF EXISTS "Erabiltzaileak_id_seq";
CREATE SEQUENCE "Erabiltzaileak_id_seq" INCREMENT 1 MINVALUE 1 MAXVALUE 2147483647 START 23 CACHE 1;

CREATE TABLE "public"."Erabiltzaileak" (
    "id" integer DEFAULT nextval('"Erabiltzaileak_id_seq"') NOT NULL,
    "izena" text NOT NULL,
    "pasahitza" text NOT NULL,
    "mota" erabiltzailemota NOT NULL,
    "sorkuntza_data" date NOT NULL,
    CONSTRAINT "Erabiltzaileak_pkey" PRIMARY KEY ("id")
)
WITH (oids = false);

INSERT INTO "Erabiltzaileak" ("id", "izena", "pasahitza", "mota", "sorkuntza_data") VALUES
(1,	'admin',	'admin',	'admin',	'2026-01-20'),
(2,	'user',	'user',	'user',	'2026-01-20'),
(5,	'Maria',	'1234',	'user',	'2025-12-23'),
(6,	'Markel',	'1234',	'user',	'2025-12-01'),
(7,	'Leire',	'1234',	'user',	'2025-12-20'),
(8,	'Iker',	'1234',	'user',	'2025-12-20'),
(9,	'Naiara',	'1234',	'user',	'2025-12-03'),
(10,	'Gorka',	'1234',	'user',	'2025-12-06'),
(11,	'Maite',	'1234',	'user',	'2025-12-21'),
(12,	'Ander',	'1234',	'user',	'2025-12-11'),
(13,	'Uxue',	'1234',	'user',	'2025-12-12'),
(14,	'Unai',	'1234',	'user',	'2025-12-18'),
(15,	'Ainhoa',	'1234',	'user',	'2025-12-18'),
(16,	'Eneko',	'1234',	'user',	'2025-12-14'),
(17,	'Miren',	'1234',	'user',	'2025-12-12'),
(18,	'Xabier',	'1234',	'user',	'2025-12-15'),
(19,	'Itziar',	'1234',	'user',	'2025-12-28'),
(20,	'Asier',	'1234',	'user',	'2025-12-12'),
(21,	'Nerea',	'1234',	'user',	'2025-12-08'),
(22,	'Irati',	'1234',	'user',	'2025-12-18'),
(3,	'Ane',	'admin',	'user',	'2025-12-20'),
(4,	'Jon',	'admin',	'user',	'2025-12-18');

DROP TABLE IF EXISTS "ErabiltzaileakStats";
CREATE TABLE "public"."ErabiltzaileakStats" (
    "erabiltzaile_id" integer NOT NULL,
    "azken_itxura" character(1),
    "azken_kolorea" text,
    "partida_sartu_n" integer DEFAULT '0' NOT NULL,
    "partida_t_max" time without time zone DEFAULT '00:00:00' NOT NULL,
    "azken_sartu_partida" integer
)
WITH (oids = false);

INSERT INTO "ErabiltzaileakStats" ("erabiltzaile_id", "azken_itxura", "azken_kolorea", "partida_sartu_n", "partida_t_max", "azken_sartu_partida") VALUES
(3,	'C',	'berdea',	12,	'02:15:20',	13),
(4,	'@',	'horia',	5,	'03:15:40',	5),
(5,	'Q',	'morea',	7,	'01:40:20',	18),
(6,	'G',	'horia',	3,	'01:45:35',	8),
(7,	'@',	'berdea',	6,	'01:20:50',	9),
(8,	'C',	'laranja',	4,	'02:30:00',	10),
(9,	'Q',	'arrosa',	9,	'03:00:15',	11),
(10,	'@',	'morea',	2,	'01:10:30',	12),
(11,	'G',	'arrosa',	1,	'04:20:00',	14),
(12,	'@',	'urdina',	5,	'01:35:45',	15),
(13,	'C',	'urdina',	3,	'00:50:10',	16),
(14,	'Q',	'berdea',	7,	'02:55:30',	17),
(15,	'@',	'gorria',	4,	'02:25:15',	19),
(16,	'G',	'horia',	3,	'01:15:00',	20),
(17,	'@',	'gorria',	6,	'03:10:40',	21),
(18,	'C',	'berdea',	8,	'02:40:25',	22),
(19,	'Q',	'morea',	5,	'01:55:10',	23),
(20,	'@',	'laranja',	10,	'04:30:00',	24);

DROP TABLE IF EXISTS "PartidakStats";
DROP SEQUENCE IF EXISTS "ErabiltzailePartidak_partida_id_seq";
CREATE SEQUENCE "ErabiltzailePartidak_partida_id_seq" INCREMENT 1 MINVALUE 1 MAXVALUE 2147483647 START 27 CACHE 1;

CREATE TABLE "public"."PartidakStats" (
    "partida_id" integer DEFAULT nextval('"ErabiltzailePartidak_partida_id_seq"') NOT NULL,
    "erabiltzaile_id" integer NOT NULL,
    "izena" text NOT NULL,
    "iraupena" time without time zone DEFAULT '00:00:00',
    "erabiltzaile_max" integer DEFAULT '0' NOT NULL,
    "mapa" text NOT NULL,
    "sorkuntza_data" date NOT NULL,
    CONSTRAINT "ErabiltzailePartidak_pkey" PRIMARY KEY ("partida_id")
)
WITH (oids = false);

INSERT INTO "PartidakStats" ("partida_id", "erabiltzaile_id", "izena", "iraupena", "erabiltzaile_max", "mapa", "sorkuntza_data") VALUES
(2,	4,	'Dungeon Master',	'01:15:30',	4,	'castillo',	'2026-01-20'),
(3,	10,	'Dragon Slayer',	'02:45:15',	6,	'montañas',	'2026-01-18'),
(4,	3,	'Forest Run',	'00:45:20',	3,	'bosque',	'2026-01-23'),
(5,	7,	'Night Raid',	'01:30:00',	5,	'ciudad',	'2026-01-01'),
(6,	4,	'Desert Storm',	'03:15:40',	8,	'desierto',	'2026-01-20'),
(7,	5,	'Ice Cave',	'00:55:10',	4,	'hielo',	'2026-01-20'),
(8,	9,	'Volcano Escape',	'02:10:25',	7,	'volcán',	'2026-01-10'),
(9,	6,	'Sky Fortress',	'01:45:35',	6,	'cielo',	'2026-01-21'),
(10,	7,	'Water Temple',	'01:20:50',	5,	'agua',	'2026-01-02'),
(11,	8,	'Ancient Ruins',	'02:30:00',	8,	'ruinas',	'2026-01-03'),
(12,	9,	'Cyber City',	'03:00:15',	10,	'futurista',	'2026-01-03'),
(13,	10,	'Haunted Mansion',	'01:10:30',	4,	'mansión',	'2026-01-06'),
(14,	3,	'Jungle Adventure',	'02:15:20',	6,	'jungla',	'2026-01-06'),
(15,	11,	'Space Station',	'04:20:00',	12,	'espacio',	'2026-01-09'),
(16,	12,	'Pirate Bay',	'01:35:45',	5,	'isla',	'2026-01-18'),
(17,	13,	'Mountain Peak',	'00:50:10',	3,	'cumbre',	'2026-01-21'),
(18,	14,	'Underground Lab',	'02:55:30',	8,	'laboratorio',	'2026-01-20'),
(19,	5,	'Crystal Cavern',	'01:40:20',	6,	'cuevas',	'2026-01-18'),
(20,	15,	'Fire Temple',	'02:25:15',	7,	'templo',	'2026-01-17'),
(21,	16,	'Wind Valley',	'01:15:00',	4,	'valle',	'2026-01-11'),
(22,	17,	'Thunder Island',	'03:10:40',	9,	'isla_tormenta',	'2026-01-11'),
(23,	18,	'Golden Palace',	'02:40:25',	8,	'palacio',	'2026-01-12'),
(24,	19,	'Shadow Realm',	'01:55:10',	6,	'sombra',	'2026-01-14'),
(25,	20,	'Moon Base',	'04:30:00',	15,	'luna',	'2026-01-15'),
(26,	6,	'Final Battle',	'05:15:20',	16,	'final_boss',	'2026-01-20');

ALTER TABLE ONLY "public"."ErabiltzaileakStats" ADD CONSTRAINT "ErabiltzaileStats_erabiltzaile_id_fkey" FOREIGN KEY (erabiltzaile_id) REFERENCES "Erabiltzaileak"(id) ON UPDATE CASCADE ON DELETE CASCADE NOT DEFERRABLE;
ALTER TABLE ONLY "public"."ErabiltzaileakStats" ADD CONSTRAINT "ErabiltzaileakStats_azken_sartu_partida_fkey" FOREIGN KEY (azken_sartu_partida) REFERENCES "PartidakStats"(partida_id) ON UPDATE CASCADE ON DELETE RESTRICT NOT DEFERRABLE;

ALTER TABLE ONLY "public"."PartidakStats" ADD CONSTRAINT "ErabiltzailePartidak_erabiltzaile_id_fkey" FOREIGN KEY (erabiltzaile_id) REFERENCES "Erabiltzaileak"(id) ON UPDATE CASCADE ON DELETE CASCADE NOT DEFERRABLE;

-- 2026-01-26 09:55:22 UTC