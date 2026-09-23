# Fortschritt Phase 1 — Fallvorlagen aus der Datenbank

Spec: `SPEC-phase1.md`. Branch `room701/night-shift-phase1` auf Phase 0. Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Template + Text Entities, Session-Erweiterung, Fehlercodes, Domain-Tests, Builder + TestData | done | (siehe git log) | + Session-Spaltenmapping und Migration V2026_09_01 vorgezogen |
| WP2 | Application: Repository-Interface, PatientCaseFactory, Handler-Umbau, CaseSanitizer, CuratedCases weg, DTO/Mapper | done | | |
| WP3 | Infrastructure: EF-Konfigurationen, Migrationen (Tabellen + Seed), Repository, DI | done | | |
| WP4 | Integration-Tests: Test-Repository, bestehende Tests anpassen, neue Tests | done | | |
| WP5 | Abschluss: Gate, README, Sol-Adversarial, Draft-PR gegen Phase 0 | in-progress | | |

## Runden
- 2026-09-23 18:05 · WP1 · terra · Review ok (Entity-Muster, Regeln, Testdaten 5 Faelle x 2 Sprachen). Selbst gefixt: Session-Fabrik (templateGuid an falscher Stelle eingefuegt), `_`-Praefixe der Testdaten-Felder, Hilfsklasse in eigene Datei. Integration 9 rot, weil EF `TemplateGuid` per Konvention mappte -> Spaltenmapping + Migration V2026_09_01 vorgezogen. Gate: format ok, Build 0 Fehler, Architektur 8, Domain 56, Integration 64. Entscheidung: done. WP2+WP3 in einem Lauf (Application ohne Infrastructure laeuft nicht), Commit nach WP4.
- 2026-09-23 18:40 · WP2+3 · terra · Review: Handler lesen aktive Vorlagen ueber Repository, Sprach-Rueckfall mit Flag, Session traegt TemplateGuid; Migrationen V2026_09_02/_03 (Tabellen, Seed mit festen GUIDs, 5 x 2 Texte, on conflict); CuratedCases geloescht. Gate ohne Integration-Seed erwartungsgemaess 11 rot.
- 2026-09-23 19:05 · WP4 · terra · Seed-Helfer `NightShiftTemplateTestRepository`, bestehende Tests seeden, 7 neue Tests. Ein Fix: Fallliste hat jetzt 6 Eintraege (Vogel ist aktive Vorlage, nicht mehr nur Fallback). Gate: format ok, Build 0 Fehler, Architektur 8, Domain 56, Integration 71. Entscheidung: done, ein Commit fuer WP2–4. WP5: Sol-Adversarial + README.
