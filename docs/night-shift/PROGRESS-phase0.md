# Fortschritt Phase 0 — NightShift im neuen Backend

Spec: `SPEC-phase0.md`. Ein Paket pro Loop-Runde; Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Enums, Entities, ErrorCodes, Domain.Tests | done | (siehe git log) | Review ohne Befund; Domain-Tests 41 -> 49 |
| WP2 | Application: Interfaces, DTOs, CuratedCases, Use Cases, Mapper, ErrorCodes | open | | |
| WP3 | Infrastructure: EF-Konfigurationen, Migration, Repository, OpenAiLlmClient, PromptBuilder + Prompts, Options, DI | open | | |
| WP4 | Presentation + Integration-Tests: Controller, appsettings, FakeLlmClient, Routings, Tests aus der Spec | open | | |
| WP5 | Abschluss: Gate komplett, README, Push, Draft-PR, Adversarial-Pass | open | | |

## Runden

(wird vom Loop gefuehrt: Datum, Paket, Modell, Review-Ergebnis, Gate, Entscheidung)
- 2026-09-23 11:25 · WP1 · terra · Codex stoppte regelkonform: "No 'Level' anywhere" kollidierte mit dem Pflegegrad-Feld `RecordCareLevel`. Klarstellung: verboten ist nur der Schwierigkeitsgrad (`Level`-Property, `{{LEVEL}}`), Pflegegrad bleibt. Neustart terra.
- 2026-09-23 11:50 · WP1 · terra (2. Lauf) · Review: Schichtgrenze, Stil, Entity-Muster, Fehlercodes -5300xx, 8 Tests — ohne Befund. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 51 (Debug). Lehre: Integration-Tests NICHT mit `--no-build -c Release` laufen lassen — Evolve findet dann die Migrationen doppelt (Debug+Release-Ausgaben); Gate = Build Release, Tests Debug. Entscheidung: done, commit.
