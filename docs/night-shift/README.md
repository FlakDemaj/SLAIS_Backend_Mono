# NightShift (Nachtwache / Room 701)

Textbasierte Patientensimulation für die Pflegeausbildung: Der Schüler ist allein im Nachtdienst, die Klingel aus Zimmer 701 ertönt, ein simulierter Patient (LLM) reagiert auf Sprache und Handlungen. Am Ende bewertet ein Ausbilder-Prompt das Gespräch auf fünf Dimensionen. Ursprung: Uni-Projekt „Pepper Patientensimulation"; erste Umsetzung als eigener Dienst (`SLAIS_Deploy/room701`), seit Phase 0 (September 2026) Feature dieses Backends.

Bindende Unterlagen: `SPEC-phase0.md` (Vertrag, Datenmodell, Regeln), `PROGRESS-phase0.md` (Arbeitsprotokoll). Fachkonzept für die nächsten Phasen (Vorlagen in der Datenbank, Admin-Verwaltung): `SLAIS_Deploy/docs/KONZEPT-room701-fallvorlagen.md`.

## Schichten und Ordner

| Schicht | Ordner | Inhalt |
| --- | --- | --- |
| Domain | `Domain/Simulations/NightShift/` | `NightShiftSessionEntity` (Session mit Fall-Schnappschuss), `NightShiftMessageEntity`, `NightShiftFeedbackEntity`, `NightShiftTemplateEntity` + `NightShiftTemplateTextEntity` (Vorlagen, seit Phase 1), `NightShiftErrorCodes` (-5300xx); Enums `Language`, `NightShiftMessageRole` unter `Domain/Common/Enums/` |
| Application | `Application/Simulations/NightShift/` | Use Cases `Commands/{StartNightShiftSession,SendNightShiftMessage,FinishNightShiftSession}`, `Querys/{GetNightShiftCases,GetNightShiftSessions,GetNightShiftSession}`, `Cases/PatientCaseFactory.cs` (Vorlage → Fall in der Session-Sprache), `NightShiftLanguage` (Sprachtexte), `NightShiftPromptTools` (Ende-Marker, Handlungsanweisung, Feedback-Parser), `NightShiftErrorCodes` (-5200xx). DTOs unter `Application/Common/DTOs/Simulations/NightShift/`, Mapper `Application/Common/Mappers/Simulations/NightShiftMapperProfile.cs`, Schnittstellen `INightShiftSessionRepository`, `INightShiftTemplateRepository`, `ILlmClient`, `INightShiftPromptBuilder` |
| Infrastructure | `Infrastructure/InternalServices/NightShift/` | `OpenAiLlmClient` (typisierter HttpClient), `NightShiftPromptBuilder` + eingebettete Prompts `Prompts/{patient,feedback,generator}.md`; EF-Konfiguration `Persistence/EntityConfigurations/Entitys/Simulations/`, Migrationen `Persistence/Migrations/V2026_09_0{0..3}__*.sql`, `Repositorys/{NightShiftSessionRepository,NightShiftTemplateRepository}.cs`, Optionen `Configurations/NightShiftOptions.cs`, Registrierung in `DependencyInjection.cs` |
| Presentation | `Presentation/Controllers/Simulations/NightShift/{NightShiftController,NightShiftTemplateController}.cs` | `rest/NightShift` für Schüler (Student/Teacher/Admin/SuperAdmin, kein Server), `rest/NightShiftTemplate` für Admin/SuperAdmin |
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

## Fallvorlagen (Phase 1)

Die Fälle liegen in `simulation.night_shift_templates` (Kopf: `key`, `state` nach `States`, `tension_start`, `sort_order`, Audit-Spalten) und `simulation.night_shift_template_texts` (eine Zeile je Sprache mit Name, Situation, Emotion, Lernziel, Eröffnungssatz und den zehn Stammblatt-Feldern). `GET cases` liefert nur aktive, nicht gelöschte Vorlagen in der Reihenfolge `sort_order, key`; fehlt die englische Fassung, kommt die deutsche mit `languageFallback: true`. `POST start` löst den Key unter den aktiven Vorlagen auf und schreibt die Vorlage als Schnappschuss in die Session (`fk_night_shift_template_guid` verweist auf die Herkunft, ohne FK-Constraint). Keine aktive Vorlage → `-520008`.

Die fünf Startfälle (keller, schmidt, yilmaz, entzug, vogel) kommen aus `V2026_09_03__Seed_Night_Shift_Templates.sql` mit festen GUIDs, beide Sprachen, aktiv; die Migration ist idempotent. Dieselben Daten liegen für Tests in `Tests.Domain.Shared/TestDataCreator/NightShiftTemplateTestData.cs`; wer eine Vorlage ändert, pflegt beide Stellen. Integration-Tests leeren die Tabellen vor jedem Lauf und seeden über `Integration.Tests/Common/Helpers/NightShiftTemplateTestRepository.cs`.

Verwaltung, Übersetzung und Übernahme von Zufallsfällen: siehe „Verwaltung der Vorlagen (Phase 2)".

## Verwaltung der Vorlagen (Phase 2)

Admins und SuperAdmins pflegen Vorlagen über `rest/NightShiftTemplate` (Controller-Attribut und Handler prüfen die Rolle):

| Verb | Route | Zweck |
| --- | --- | --- |
| GET | `` | alle Vorlagen außer gelöschten, beide Sprachen |
| GET | `{templateGuid}` | eine Vorlage |
| POST | `` | anlegen (`key`, `tensionStart`, `sortOrder`, optional `german`/`english`), Zustand `pending` |
| PUT | `{templateGuid}` | bearbeiten; Body trägt `expectedVersion`, sonst `-520009`; Key nur im Zustand `pending` änderbar |
| POST | `{templateGuid}/state` | `active`, `archived` oder `pending` (mit `expectedVersion`); aktiv nur mit beiden Sprachen |
| DELETE | `{templateGuid}` | Soft Delete (Zustand `deleted`), laufende Sessions unberührt |
| POST | `translate` | KI-Übersetzungsvorschlag (`sourceLanguage`, `targetLanguage`, `text` mit 15 Feldern); speichert nichts, Fehler `-520010` |
| POST | `from-session/{sessionGuid}` | Zufallsfall einer Session als Entwurf übernehmen (`key`); nur Sessions ohne Vorlage |

Jede Änderung erhöht `version`; Update und Zustandswechsel scheitern mit `-520009`, wenn jemand dazwischen gespeichert hat. `POST rest/NightShift/start` akzeptiert zusätzlich `templateId`; Admins dürfen damit Entwürfe testen, Schüler nur aktive Vorlagen (`-520014`). Antwortform der Vorlage: `{templateId, key, state: pending|active|archived, tensionStart, sortOrder, version, createdAt, updatedAt, texts: {de: {…}, en: {…}}}`.

Der Übersetzungs-Prompt liegt unter `Infrastructure/InternalServices/NightShift/Prompts/translate.md`; die Antwort wird durch dieselben Regeln wie ein Vorlagentext geprüft. Offen für Flak: die Audit-Felder `updated_by`/`deleted_by` bleiben leer, solange die Basisklassen keine protected Setter haben.

## Bewusst nicht in Phase 0

- Frontend für die Vorlagenverwaltung (Phase 3, siehe Konzept); Backend seit Phase 2.
- Prompts aus `system.prompts` (Tabelle existiert, Entity noch nicht).
- Schwierigkeitsgrad: bewusst entfernt; die Startanspannung (`case_tension_start`) bleibt als verborgener Simulationswert.
