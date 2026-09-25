# Fortschritt Phase 2 — Verwaltung der Fallvorlagen (Admin-API)

Spec: `SPEC-phase2.md`. Branch `room701/night-shift-phase2` auf Phase 1. Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Version, Zustandsregeln, Reopen, EnsureEditable, Text.Update, Domain-Tests | done | | |
| WP2+3 | Application (Fehlercodes, DTOs, Mapper, acht Use Cases, TemplateId beim Start, Uebersetzungsparser) + Infrastructure (Migration, Mapping, GetByKeyAsync, translate.md, PromptBuilder) | done | | |
| WP4 | Presentation (Controller, Routings) + Integration-Tests + FakeLlmClient-Zweig | in-progress | | |
| WP5 | Gate, README, Sol-Adversarial, Draft-PR gegen Phase 1, Stack, curl-Check | open | | |

## Runden
- 2026-09-25 14:55 · WP1 · terra · Review ok (Version++, Zustandsregeln, Reopen, EnsureEditable, Text.Update, 6 Tests). Wie in Phase 1: `Version` per Konvention gemappt -> Mapping + Migration V2026_09_04 vorgezogen. Zweiter Stolperstein: Finder-Duplikate `… 2.sql` im Build-Output (nicht im Quellbaum) liessen Evolve doppelte Versionen sehen -> `*/bin/*/Persistence/Migrations` geleert. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 75. Entscheidung: done.
- 2026-09-25 15:35 · WP2+3 · terra (3 Laeufe) · Lauf 1 stoppte am Gating (ungeloeste Template-Platzhalter, Fehlercode-Widerspruch bei Key-Aenderung -> Spec: -520015). Lauf 2 lieferte alles, aber als Einzeiler; Lauf 3 reine Formatierung. Review: Version-Check, Key-Regeln, Uebersetzungsparser mit Entity-Validierung, FromSession nur fuer Zufallsfaelle, Start mit templateId (Admins duerfen Entwuerfe testen). Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 75. Entscheidung: done. WP4 an terra.
