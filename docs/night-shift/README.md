# NightShift (Nachtwache / Room 701)

Textbasierte Patientensimulation für die Pflegeausbildung: Der Schüler ist allein im Nachtdienst, die Klingel aus Zimmer 701 ertönt, ein simulierter Patient (LLM) reagiert auf Sprache und Handlungen. Am Ende bewertet ein Ausbilder-Prompt das Gespräch auf fünf Dimensionen. Ursprung: Uni-Projekt „Pepper Patientensimulation"; erste Umsetzung als eigener Dienst (`SLAIS_Deploy/room701`), seit Phase 0 (September 2026) Feature dieses Backends.

Bindende Unterlagen: `SPEC-phase0.md` (Vertrag, Datenmodell, Regeln), `PROGRESS-phase0.md` (Arbeitsprotokoll). Fachkonzept für die nächsten Phasen (Vorlagen in der Datenbank, Admin-Verwaltung): `SLAIS_Deploy/docs/KONZEPT-room701-fallvorlagen.md`.

## Schichten und Ordner

| Schicht | Ordner | Inhalt |
| --- | --- | --- |
| Domain | `Domain/Simulations/NightShift/` | `NightShiftSessionEntity` (Session mit Fall-Schnappschuss), `NightShiftMessageEntity`, `NightShiftFeedbackEntity`, `NightShiftErrorCodes` (-5300xx); Enums `Language`, `NightShiftMessageRole` unter `Domain/Common/Enums/` |
| Application | `Application/Simulations/NightShift/` | Use Cases `Commands/{StartNightShiftSession,SendNightShiftMessage,FinishNightShiftSession}`, `Querys/{GetNightShiftCases,GetNightShiftSessions,GetNightShiftSession}`, kuratierte Fälle `Cases/CuratedCases.cs` (DE/EN, Phase-0-Provisorium), `NightShiftLanguage` (Sprachtexte), `NightShiftPromptTools` (Ende-Marker, Handlungsanweisung, Feedback-Parser), `NightShiftErrorCodes` (-5200xx). DTOs unter `Application/Common/DTOs/Simulations/NightShift/`, Mapper `Application/Common/Mappers/Simulations/NightShiftMapperProfile.cs`, Schnittstellen `INightShiftSessionRepository`, `ILlmClient`, `INightShiftPromptBuilder` |
| Infrastructure | `Infrastructure/InternalServices/NightShift/` | `OpenAiLlmClient` (typisierter HttpClient), `NightShiftPromptBuilder` + eingebettete Prompts `Prompts/{patient,feedback,generator}.md`; EF-Konfiguration `Persistence/EntityConfigurations/Entitys/Simulations/`, Migration `Persistence/Migrations/V2026_09_00__Create_Night_Shift.sql`, `Repositorys/NightShiftSessionRepository.cs`, Optionen `Configurations/NightShiftOptions.cs`, Registrierung in `DependencyInjection.cs` |
| Presentation | `Presentation/Controllers/Simulations/NightShift/NightShiftController.cs` | Route `rest/NightShift`, Rollen Student/Teacher/Admin/SuperAdmin (kein Server) |
| Tests | `Domain.Tests/Simulations/NightShift/`, `Integration.Tests/Simulations/NightShift/` | Entity-Regeln; HTTP-Vertrag gegen Testcontainers-Postgres mit `Integration.Tests/Common/FakeLlmClient.cs` |

Tabellen im Schema `simulation`: `night_shift_sessions`, `night_shift_messages`, `night_shift_feedbacks`. Jede Session speichert eine Kopie ihres Falls (Spalten `case_*`, `record_*`); spätere Änderungen an Fällen berühren alte Sessions nicht.

## HTTP-Vertrag (`rest/NightShift`, camelCase)

| Aufruf | Anfrage | Antwort |
| --- | --- | --- |
| `GET cases?lang=de\|en` | – | `[{key, name, situation, lernziel}]`, letzter Eintrag `{key:"random", …}` |
| `POST start` | `{caseKey?: string, language?: "de"\|"en"}`; leer = zufälliger kuratierter Fall, `"random"` = vom Modell generiert (Rückfall auf kuratierten Fall bei LLM-Ausfall) | `{sessionId, temperament, firstMessage, ended:false, language, case:{key,name,situation,lernziel}, stammblatt:{geboren,geschlecht,aufnahme,diagnosen,allergien,medikation,pflegegrad,risiken,reanimation,angehoerige}}` |
| `POST chat/{sessionId}` | `{text, kind: "say"\|"do"}` oder nacktes JSON-String-Literal (= say) | `{response, ended}`; nach `ended` → `-520002` |
| `POST finish/{sessionId}` | `{}` | `{feedback, ok, scores:{fachlich,sympathie,empathie,zuhoeren,klarheit}, labels:{…}, perDimension:{…}, summary}`; zweiter Aufruf liefert das gespeicherte Ergebnis |
| `GET sessions` | – | `[{sessionId, caseKey, caseName, startedAt, endedAt, finishedAt, ended, hasFeedback, turns}]`, nur eigene, neueste zuerst |
| `GET sessions/{sessionId}` | – | Summary + `messages:[{role: patient\|student\|action, text, at}]` + `feedback` oder `null` |

Fehlercodes (Application, Status 400 nach Backend-Regel): `-520001` Session nicht gefunden, `-520002` Session bereits beendet, `-520003` kein Zugriff, `-520004` leerer Beitrag, `-520005` ungültiger Fall, `-520006` LLM nicht erreichbar. Domain (`-5300xx`): ungültige Eingaben, Nachricht zu lang, ungültige Bewertung, ungültige Sortierung.

## Konfiguration

`appsettings.json`, Abschnitt `NightShift:OpenAi` mit `ApiKey` (leer im Repo), `Model` (Standard `gpt-4o-mini`), `BaseUrl`. Der Schlüssel kommt aus der Umgebung: `NightShift__OpenAi__ApiKey`. Der HttpClient hat 60 s Timeout; jeder Transportfehler wird zu `-520006`.

## Tests

Integration-Tests ersetzen `ILlmClient` im Test-Host durch `FakeLlmClient` (`IntegrationTestWebApplicationFactory.ConfigureWebHost`): Antworten sind deterministisch („Tschuess" beendet die Sitzung mit `[[ENDE]]`, Feedback-Prompt liefert festes JSON), `CallCount` belegt, dass `finish` nur einmal das Modell ruft. OpenAI wird in Tests nie aufgerufen. Gate: `dotnet format --verify-no-changes --severity error`, Build Release, Tests in Debug (Architecture, Domain, Integration mit Docker).

## Bewusst nicht in Phase 0

- Fallvorlagen in der Datenbank und Admin-Verwaltung (Phase 1–3, siehe Konzept).
- Übersetzungsvorschlag per KI und „Zufallsfall als Vorlage übernehmen".
- Prompts aus `system.prompts` (Tabelle existiert, Entity noch nicht).
- Schwierigkeitsgrad: bewusst entfernt; die Startanspannung (`case_tension_start`) bleibt als verborgener Simulationswert.
