# Fortschritt Phase 2 — Verwaltung der Fallvorlagen (Admin-API)

Spec: `SPEC-phase2.md`. Branch `room701/night-shift-phase2` auf Phase 1. Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Version, Zustandsregeln, Reopen, EnsureEditable, Text.Update, Domain-Tests | done | | |
| WP2+3 | Application (Fehlercodes, DTOs, Mapper, acht Use Cases, TemplateId beim Start, Uebersetzungsparser) + Infrastructure (Migration, Mapping, GetByKeyAsync, translate.md, PromptBuilder) | in-progress | | |
| WP4 | Presentation (Controller, Routings) + Integration-Tests + FakeLlmClient-Zweig | open | | |
| WP5 | Gate, README, Sol-Adversarial, Draft-PR gegen Phase 1, Stack, curl-Check | open | | |

## Runden
- 2026-09-25 14:55 · WP1 · terra · Review ok (Version++, Zustandsregeln, Reopen, EnsureEditable, Text.Update, 6 Tests). Wie in Phase 1: `Version` per Konvention gemappt -> Mapping + Migration V2026_09_04 vorgezogen. Zweiter Stolperstein: Finder-Duplikate `… 2.sql` im Build-Output (nicht im Quellbaum) liessen Evolve doppelte Versionen sehen -> `*/bin/*/Persistence/Migrations` geleert. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 62, Integration 75. Entscheidung: done.
