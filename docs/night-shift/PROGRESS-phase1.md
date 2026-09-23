# Fortschritt Phase 1 — Fallvorlagen aus der Datenbank

Spec: `SPEC-phase1.md`. Branch `room701/night-shift-phase1` auf Phase 0. Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Template + Text Entities, Session-Erweiterung, Fehlercodes, Domain-Tests, Builder + TestData | done | (siehe git log) | + Session-Spaltenmapping und Migration V2026_09_01 vorgezogen |
| WP2 | Application: Repository-Interface, PatientCaseFactory, Handler-Umbau, CaseSanitizer, CuratedCases weg, DTO/Mapper | in-progress | | |
| WP3 | Infrastructure: EF-Konfigurationen, Migrationen (Tabellen + Seed), Repository, DI | in-progress | | |
| WP4 | Integration-Tests: Test-Repository, bestehende Tests anpassen, neue Tests | open | | |
| WP5 | Abschluss: Gate, README, Sol-Adversarial, Draft-PR gegen Phase 0 | open | | |

## Runden
- 2026-09-23 18:05 · WP1 · terra · Review ok (Entity-Muster, Regeln, Testdaten 5 Faelle x 2 Sprachen). Selbst gefixt: Session-Fabrik (templateGuid an falscher Stelle eingefuegt), `_`-Praefixe der Testdaten-Felder, Hilfsklasse in eigene Datei. Integration 9 rot, weil EF `TemplateGuid` per Konvention mappte -> Spaltenmapping + Migration V2026_09_01 vorgezogen. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 56, Integration 64. Entscheidung: done. WP2+WP3 in einem Lauf (Application ohne Infrastructure laeuft nicht), Commit nach WP4.
