# Fortschritt Phase 0 — NightShift im neuen Backend

Spec: `SPEC-phase0.md`. Ein Paket pro Loop-Runde; Status `open` | `in-progress` | `rework` | `done`.

| Paket | Inhalt | Status | Commit | Notizen / Must-fix |
| --- | --- | --- | --- | --- |
| WP1 | Domain: Enums, Entities, ErrorCodes, Domain.Tests | done | (siehe git log) | Review ohne Befund; Domain-Tests 41 -> 49 |
| WP2 | Application: Interfaces, DTOs, CuratedCases, Use Cases, Mapper, ErrorCodes | done | (siehe git log) | Review ohne Blocker; Nit fuer WP5: lange Einzeiler bei Konstruktoren/Aufrufen auf Parameter-je-Zeile umbrechen |
| WP3 | Infrastructure: EF-Konfigurationen, Migration, Repository, OpenAiLlmClient, PromptBuilder + Prompts, Options, DI | done | | |
| WP4 | Presentation + Integration-Tests: Controller, appsettings, FakeLlmClient, Routings, Tests aus der Spec | done | | |
| WP5 | Abschluss: Gate komplett, README, Push, Draft-PR, Adversarial-Pass | done | | |

## Runden

(wird vom Loop gefuehrt: Datum, Paket, Modell, Review-Ergebnis, Gate, Entscheidung)
- 2026-09-23 11:25 · WP1 · terra · Codex stoppte regelkonform: "No 'Level' anywhere" kollidierte mit dem Pflegegrad-Feld `RecordCareLevel`. Klarstellung: verboten ist nur der Schwierigkeitsgrad (`Level`-Property, `{{LEVEL}}`), Pflegegrad bleibt. Neustart terra.
- 2026-09-23 11:50 · WP1 · terra (2. Lauf) · Review: Schichtgrenze, Stil, Entity-Muster, Fehlercodes -5300xx, 8 Tests — ohne Befund. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 51 (Debug). Lehre: Integration-Tests NICHT mit `--no-build -c Release` laufen lassen — Evolve findet dann die Migrationen doppelt (Debug+Release-Ausgaben); Gate = Build Release, Tests Debug. Entscheidung: done, commit.
- 2026-09-23 12:05 · Infra · Push auf `slais-room701-Add-NightShift-Simulation` abgelehnt (GH006, Branch-Protection auf `slais-*`: nur per PR, 6 Pflicht-Checks). Branch umbenannt in `room701/night-shift-phase0`, dort gepusht. WP2 an terra gestartet.
- 2026-09-23 12:30 · WP2 · terra · 34 Dateien, keine Bestandsdatei geaendert. Review: Logik 1:1 zur Referenz (Start/Chat/Finish, Sprache, Feedback-Parser), Schichtgrenzen ok, Fehlercodes -5200xx, DTO-Namen liefern das Spec-JSON. Nit (WP5): Einzeiler statt Parameter je Zeile. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 51. Entscheidung: done. WP3 an terra.
- 2026-09-23 13:00 · WP3 · terra · Review: Migration/EF-Mapping deckungsgleich, Repository nach Vorbild, LLM-Client mappt alle Transportfehler auf -520006, Prompts ohne LEVEL. Selbst gefixt (< 30 Zeilen): doppelte Unique-Definition in der Migration entfernt, fehlendes `using Microsoft.EntityFrameworkCore;` in drei Index-Extensions (Kompilierfehler), Usings statt voll qualifizierter Typen im DI. Nit (WP5): Attribute-Config als Einzeiler statt Kettenformat. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 51 (EF-Modell + Migration validiert). Entscheidung: done. WP4 an terra.
- 2026-09-23 13:35 · WP4 · terra · Review: Controller im Vorbildstil (Rollen ohne Server, Body-Varianten fuer chat), Options-Registrierung minimal, FakeLlmClient ueber ConfigureTestServices, zehn Spec-Tests vorhanden. Selbst gefixt: `EmptyBodyBehavior.Allow` fuer leeren start-Body, CallCount als Property. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 61 (10 neue, HTTP-Vertrag belegt). Entscheidung: done. WP5: Stil-Politur + README an terra, danach Sol-Adversarial.
- 2026-09-23 14:05 · WP5a · terra · Politur nur auf sechs Handlern (Einzeiler -> Parameter je Zeile), README nicht geliefert -> README von Claude. Whitespace-freier Vergleich: keine Logikaenderung. Gate gruen (8/49/61). Offen: Attribute-Configs, Mapper, NightShiftLanguage, CaseSanitizer noch Einzeiler — kommt mit den Sol-Befunden in einen letzten terra-Lauf. Jetzt Sol-Adversarial read-only.
- 2026-09-23 14:40 · WP5b · Sol-Adversarial (read-only) · Befunde: (B1) `sessions/{id}` liefert flache Summary statt Referenz-Envelope {session, case, stammblatt, messages, feedback} -> uebernommen. (B2) LLM-Aufrufe laufen innerhalb der TransactionPipeline-Transaktion (bis 60 s), Races bei sort_order/Feedback -> Commands auf `INoTransaction` (ein SaveChanges am Ende bleibt atomar), Unique-Indizes fangen Doppelaufrufe; Concurrency-Tests als Folgeaufgabe. (S1) Not-ok-Feedback (kein Schuelerbeitrag, kaputtes JSON) wurde gespeichert und beendete die Session -> wie Referenz: nicht speichern, nicht beenden. (S2) Composite-Index (user, started_at desc) statt zwei Einzelindizes -> uebernommen; Forderung nach voller BaseId/Audit-Kette abgelehnt (RefreshToken/RegistrationCode nutzen ebenfalls nur BaseGuidEntity; Identity-Spalte bleibt DB-seitig). (S3) Retry (2 Versuche, 2 s) und Weitergabe von Caller-Cancellation -> uebernommen. (S4) generator.md enthielt noch level -> entfernt + Prompt-Test. Sauber laut Sol: Auth, Schichten, Fehlercodes, Testnamen. Fix-Lauf terra inkl. Rest-Politur.
- 2026-09-23 15:10 · WP5c · terra (Fix-Lauf) · Alle sieben Punkte umgesetzt (Envelope, INoTransaction, Not-ok ohne Persistenz, Composite-Index, Retry/Cancellation, generator.md ohne level, Rest-Politur) + 3 neue Tests. Selbst gefixt: Using-Reihenfolge (dotnet format), verwaiste Klammer und FluentAssertions-Aufruf im Prompt-Test. Gate: format ok, Build Release 0 Fehler, Architektur 8, Domain 49, Integration 64. Entscheidung: done, Draft-PR.

## Folgeaufgaben (nicht Phase 0)
- Concurrency-Tests (paralleler chat, paralleler finish, chat vs. finish).
- Gateway-Umschaltung `/rest/NightShift*` auf das neue Backend + `NightShift__OpenAi__ApiKey` im Compose, alten room701-Container abschalten (SLAIS_Deploy, nach Abnahme).
- Prompts aus `system.prompts` laden, sobald es dort eine Entity gibt.
- Phase 1: Fallvorlagen in der Datenbank (siehe SLAIS_Deploy/docs/KONZEPT-room701-fallvorlagen.md).
