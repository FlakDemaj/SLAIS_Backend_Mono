# Fortschritt Phase 2 — Verwaltung der Fallvorlagen (Admin-API)

Spec: `SPEC-phase2.md`. Branch `room701/night-shift-phase2` auf Phase 1. Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Version, Zustandsregeln, Reopen, EnsureEditable, Text.Update, Domain-Tests | done | | |
| WP2+3 | Application (Fehlercodes, DTOs, Mapper, acht Use Cases, TemplateId beim Start, Uebersetzungsparser) + Infrastructure (Migration, Mapping, GetByKeyAsync, translate.md, PromptBuilder) | done | | |
| WP4 | Presentation (Controller, Routings) + Integration-Tests + FakeLlmClient-Zweig | done | | |
| WP5 | Gate, README, Sol-Adversarial, Draft-PR gegen Phase 1, Stack, curl-Check | done | | |

## Runden
- 2026-09-25 14:55 · WP1 · terra · Review ok (Version++, Zustandsregeln, Reopen, EnsureEditable, Text.Update, 6 Tests). Wie in Phase 1: `Version` per Konvention gemappt -> Mapping + Migration V2026_09_04 vorgezogen. Zweiter Stolperstein: Finder-Duplikate `… 2.sql` im Build-Output (nicht im Quellbaum) liessen Evolve doppelte Versionen sehen -> `*/bin/*/Persistence/Migrations` geleert. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 75. Entscheidung: done.
- 2026-09-25 15:35 · WP2+3 · terra (3 Laeufe) · Lauf 1 stoppte am Gating (ungeloeste Template-Platzhalter, Fehlercode-Widerspruch bei Key-Aenderung -> Spec: -520015). Lauf 2 lieferte alles, aber als Einzeiler; Lauf 3 reine Formatierung. Review: Version-Check, Key-Regeln, Uebersetzungsparser mit Entity-Validierung, FromSession nur fuer Zufallsfaelle, Start mit templateId (Admins duerfen Entwuerfe testen). Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 75. Entscheidung: done. WP4 an terra.
- 2026-09-25 16:05 · WP4 · terra · Controller mit acht Routen, Rollen Admin/SuperAdmin, Routings, Fake-Uebersetzungszweig, 14 Tests. Review ohne Befund, keine Einzeiler. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 89. Entscheidung: done. WP5: Sol-Adversarial + README.
- 2026-09-25 16:40 · WP5 · Sol-Adversarial: 1 Blocker + 3 Should-fix. Uebernommen: `version` als EF-Concurrency-Token (verlorene Updates unmoeglich; im Rennfall kommt der generische Pipeline-Fehler -100001 statt -520009, weil `DbUpdateConcurrencyException` in Flaks TransactionPipeline gemappt wird), JSON-Wurzel der Uebersetzung muss ein Objekt sein, Generator ohne `situation` faellt auf den Fallback-Text zurueck (Import von Zufallssessions bleibt moeglich). Abgelehnt: Postgres-23505 auf -520011 mappen — Pipeline-Aenderung, Hinweis fuer Flak; die Vorpruefung liefert den Code im Normalfall. Nicht gebaut: echte Parallel-Tests. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 89. Draft-PR.

## Folgeaufgaben
- TransactionPipeline: DbUpdateConcurrencyException -> -520009, Postgres 23505 -> -520011 (Flak).
- Audit-Setter in den Basisklassen (Flak).
- Parallel-Tests fuer Update/Update und Create/Create.
- Phase 3: Frontend `/admin/room701`.
- 2026-09-25 17:05 · Stack-Check · Gateway kannte `/rest/NightShiftTemplate` nicht (fiel aufs alte Backend, 404) -> Route in `SLAIS_Deploy` ergaenzt. curl als Max.Admin: Liste (5 aktive, v1), Anlegen (pending), Aktivieren ohne Englisch -> -530009, Uebersetzung per echtem LLM (Mrs. Test, 70 / Test ward / none), Loeschen; Schueler -> 403. Fehlerkoerper des Backends ist PascalCase (`ErrorCode`/`ErrorMessage`), wie bei den anderen Controllern.
