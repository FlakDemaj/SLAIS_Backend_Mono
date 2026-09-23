# Spec Phase 0: Room 701 (Nachtwache) ins neue Backend

Stand 2026-09-23. Ziel-Repo: `FlakDemaj/SLAIS_Backend_Mono`, Branch `slais-room701-Add-NightShift-Simulation`, Basis `origin/slais-4-Add-UserController-With-Logic` (Flaks aktiver Stand; enthält User-Feature und Test-Infrastruktur). Konzept: `docs/KONZEPT-room701-fallvorlagen.md`. Dieser Text ist die Arbeitsanweisung für Codex; die Kopie im Backend-Branch liegt unter `docs/night-shift/SPEC-phase0.md`, der Fortschritt unter `docs/night-shift/PROGRESS-phase0.md`.

## Ziel

Der heutige Room-701-Dienst (`SLAIS_Deploy/room701`: Minimal-API, Raw-Npgsql, Records) wird als Feature **NightShift** im neuen Backend nachgebaut, nach dessen Regeln, mit **unverändertem HTTP-Vertrag** für das Frontend. Danach zeigt das Gateway `/rest/NightShift/*` auf das neue Backend; der alte Dienst wird abgeschaltet. Fälle bleiben in Phase 0 im Code (kuratierte Liste DE/EN); Datenbank-Vorlagen sind Phase 1. **Kein Schwierigkeitsgrad** (`level`) mehr, nirgends.

## Regeln (Kurzfassung, gelten für jede Datei)

Aus `.editorconfig`, `CLAUDE.md` und dem bestehenden Code des Repos (Vorbild: Feature `User`):

- Schichten `Domain ← Application ← Infrastructure ← Presentation`; `Architecture.Tests` müssen grün bleiben.
- `var` überall; **keine Expression-Bodied Members** (auch Properties mit `get { return …; }`); Allman-Klammern; 4 Leerzeichen; LF; file-scoped Namespaces; klassische Konstruktor-Injektion mit Parametern auf eigenen Zeilen; keine `record`-Typen; keine Primärkonstruktoren; `_camelCase` für private/protected Felder; `Async`-Suffix; `#region`-Gruppen wie in `UserEntity`; System-Usings zuerst mit Leerzeile zwischen Gruppen; Nullable an, CS8600–8604 sind Fehler.
- Namespace-Wurzel wie die Nachbardateien des jeweiligen Projekts (die Wurzeln weichen teils vom Ordner ab; nicht „korrigieren").
- Entities: private Setter, privater Konstruktor, statische Fabriken, `CheckInputs` wirft `SlaisException(<ErrorCodes>)`, Vererbung über die Kette `BaseGuidEntity → BaseCreatedByEntity → BaseUpdatedByEntity → BaseDeleteByEntity → BaseIdEntity`, Guid v7 aus `BaseGuidEntity`; Navigationseigenschaften in einer eigenen `…NavigationPropertyEntity`.
- Use Cases: `Application/<Bereich>/<Feature>/Commands/<UseCase>/{<UseCase>Command,<UseCase>CommandHandler}.cs` und `Querys/<UseCase>/{…Query,…QueryHandler}.cs` (Schreibweise `Querys`); Handler erben `BaseHandler<TRequest>` und implementieren `IRequestHandler<TRequest,TResponse>.HandleAsync(request, IAuthentication?, CancellationToken)`; reine Lesefälle implementieren `INoTransaction`; Handler lesen nie `HttpContext`, nur `authentication`.
- DTOs unter `Application/Common/DTOs/<Bereich>/<Feature>/`, Klassen mit `required`/`init`; Mapping per AutoMapper-`Profile` unter `Application/Common/Mappers/<Bereich>/`; Repository-Interfaces unter `Application/Common/Interfaces/Repositorys/`, Dienst-Interfaces unter `Application/Common/Interfaces/Services/`.
- Fehler: `throw new SlaisException(ErrorCode)`, Codes als negative Enum-Werte mit deutschem `[Description]`; Bereiche für dieses Feature: Application `NightShiftErrorCodes` **-5200xx**, Domain `NightShiftErrorCodes` **-5300xx**. HTTP-Status ergibt sich aus der ersten Ziffer (5 → 400); das ist so gewollt.
- Persistenz: EF Core, Konfiguration je Entity in drei Dateien unter `Infrastructure/Persistence/EntityConfigurations/Entitys/<Bereich>/<Entity>Config/` (`…AttributeConfig.cs`, `…ForeignKeyExtension.cs`, `…IndexExtension.cs`), Basis-Config-Kette wie bei `UserEntityConfig`; Tabellen im Schema `simulation`, Präfix `night_shift_`; Spaltenpräfix je Entity; Migration als Evolve-SQL `Infrastructure/Persistence/Migrations/V2026_09_00__Create_Night_Shift.sql`, idempotent, Kleinschreibung, FK-Constraints im `DO $$ … $$`-Block, Namen `fk_<tabelle>_<ziel>`, Indizes `idx_<tabelle>_<spalte>`.
- Repositories: Interface `INightShiftSessionRepository : IBaseRepository<NightShiftSessionEntity>`; Implementierung unter `Infrastructure/Repositorys/`, nutzt `GetTrackingSet`/`GetNoTrackingSet`; `CreateAsync` speichert nicht (das macht die `TransactionPipeline`); Registrierung in `Infrastructure/DependencyInjection.cs` → `AddRepositories`.
- Controller: `Presentation/Controllers/Simulations/NightShift/NightShiftController.cs`, erbt `BaseRestController`, Route `rest/NightShift`, Klassenattribut `[Authorize(Roles = $"{Role.Student},{Role.Teacher},{Role.Admin},{Role.SuperAdmin}")]` (kein `Server`), Aktionen `…Async`, Rückgabe `ActionResult<TDto>`, `_mediator.SendAsync(request, Authentication, cancellationToken)`.
- Logging nur `ISlaisLogger<T>`. Keine Secrets in `appsettings.json` (leerer Schlüssel, Wert kommt aus Umgebung `NightShift__OpenAi__ApiKey`).
- Tests: xUnit + FluentAssertions; Domain-Tests unter `Domain.Tests/Simulations/NightShift/`, Namensschema `Method_Condition_ShouldResult`, `act.ThrowsException(ErrorCode)`; Integration-Tests unter `Integration.Tests/Simulations/NightShift/` auf Basis `Integration.Tests/Common/TestBase` (Testcontainers-Postgres, `AuthenticateAs`), Routen in `Common/Routings.cs` ergänzen. **Einzige Ausnahme von „keine Mocks":** `ILlmClient` wird im Test-Host durch `FakeLlmClient` (Integration.Tests/Common) ersetzt, der vorbereitete Antworten liefert; OpenAI wird in Tests nie aufgerufen.
- Gate vor jedem Commit: `dotnet format SLAIS.sln --verify-no-changes --severity error`, `dotnet build SLAIS.sln -c Release`, `dotnet test Architecture.Tests`, `dotnet test Domain.Tests`, `dotnet test Integration.Tests` (Docker läuft). Commit-Nachrichten wie im Repo: `SLAIS-ROOM701: <Was>`.

## Ordner (neu, alles andere bleibt unangetastet)

```
Domain/Common/Enums/Language.cs                         (German = 0, English = 1; falls schon ein Sprach-Enum existiert, das verwenden)
Domain/Common/Enums/NightShiftMessageRole.cs            (System = 0, Student = 1, Action = 2, Patient = 3)
Domain/Simulations/NightShift/NightShiftSessionEntity.cs
Domain/Simulations/NightShift/NightShiftSessionNavigationPropertyEntity.cs
Domain/Simulations/NightShift/NightShiftMessageEntity.cs
Domain/Simulations/NightShift/NightShiftFeedbackEntity.cs
Domain/Simulations/NightShift/NightShiftErrorCodes.cs   (-5300xx)
Application/Simulations/NightShift/NightShiftErrorCodes.cs (-5200xx)
Application/Simulations/NightShift/Cases/CuratedCases.cs   (statische Liste DE/EN, Phase-0-Provisorium, Kommentar mit Verweis auf Phase 1)
Application/Simulations/NightShift/Cases/PatientCase.cs    (Klasse, kein Record: Key, Name, Situation, Emotion, LearningGoal, TensionStart, Opener, Record-Felder)
Application/Simulations/NightShift/Querys/GetNightShiftCases/{GetNightShiftCasesQuery,GetNightShiftCasesQueryHandler}.cs
Application/Simulations/NightShift/Commands/StartNightShiftSession/…
Application/Simulations/NightShift/Commands/SendNightShiftMessage/…
Application/Simulations/NightShift/Commands/FinishNightShiftSession/…
Application/Simulations/NightShift/Querys/GetNightShiftSessions/…
Application/Simulations/NightShift/Querys/GetNightShiftSession/…
Application/Common/DTOs/Simulations/NightShift/{NightShiftCaseResponseDto,StartNightShiftSessionResponseDto,NightShiftStammblattDto,NightShiftMessageResponseDto,NightShiftFeedbackResponseDto,NightShiftSessionSummaryResponseDto,NightShiftSessionDetailResponseDto}.cs
Application/Common/Interfaces/Repositorys/INightShiftSessionRepository.cs
Application/Common/Interfaces/Services/{ILlmClient,INightShiftPromptBuilder}.cs
Application/Common/Mappers/Simulations/NightShiftMapperProfile.cs
Infrastructure/Configurations/NightShiftOptions.cs        (OpenAi: ApiKey, Model = "gpt-4o-mini", BaseUrl = "https://api.openai.com/v1/")
Infrastructure/InternalServices/NightShift/OpenAiLlmClient.cs
Infrastructure/InternalServices/NightShift/NightShiftPromptBuilder.cs
Infrastructure/InternalServices/NightShift/Prompts/{patient,feedback,generator}.md   (eingebettete Ressourcen; Inhalt 1:1 aus SLAIS_Deploy/room701, ohne die Zeile "Schwierigkeitsgrad")
Infrastructure/Persistence/EntityConfigurations/Entitys/Simulations/NightShiftSessionEntityConfig/{…AttributeConfig,…ForeignKeyExtension,…IndexExtension}.cs
Infrastructure/Persistence/EntityConfigurations/Entitys/Simulations/NightShiftMessageEntityConfig/…
Infrastructure/Persistence/EntityConfigurations/Entitys/Simulations/NightShiftFeedbackEntityConfig/…
Infrastructure/Persistence/Migrations/V2026_09_00__Create_Night_Shift.sql
Infrastructure/Repositorys/NightShiftSessionRepository.cs
Infrastructure/DependencyInjection.cs                      (+ Repository, + HttpClient<OpenAiLlmClient>, + PromptBuilder, + Options)
Infrastructure/Pipelines/GuidResolver…                     (+ Fall "NightShiftSession", falls Requests IHasGuid nutzen; sonst nicht anfassen)
Presentation/Controllers/Simulations/NightShift/NightShiftController.cs
Presentation/appsettings.json                              (+ Abschnitt "NightShift": { "OpenAi": { "ApiKey": "", "Model": "gpt-4o-mini", "BaseUrl": "https://api.openai.com/v1/" } })
Domain.Tests/Simulations/NightShift/{NightShiftSessionTests,NightShiftFeedbackTests}.cs
Integration.Tests/Common/FakeLlmClient.cs
Integration.Tests/Simulations/NightShift/NightShiftControllerTests.cs
Tests.Domain.Shared/Builders/NightShiftSessionEntityBuilder.cs
docs/night-shift/{SPEC-phase0,PROGRESS-phase0}.md
```

## Datenmodell

`simulation.night_shift_sessions` (Präfix `night_shift_session_`): `night_shift_session_guid uuid pk`, `night_shift_session_id int identity unique`, `fk_user_guid uuid not null` (Besitzer), `language smallint not null`, `case_key text not null`, Schnappschuss des Falls als Spalten `case_name, case_situation, case_emotion, case_learning_goal, case_opener text not null`, `case_tension_start smallint not null`, Stammblatt `record_born, record_gender, record_admission, record_diagnoses, record_allergies, record_medication, record_care_level, record_risks, record_resuscitation, record_relatives text not null`, `prompt_version text not null`, `model text not null`, `started_at timestamptz not null`, `ended_at timestamptz`, `finished_at timestamptz`, `ended boolean not null default false`, Audit-Spalten der Basis-Kette (`created_at`, `created_by_user_guid`, …). Index `idx_night_shift_sessions_fk_user_guid_started_at` (user, started_at desc). FK auf `public.users` wie bei anderen Tabellen.

`simulation.night_shift_messages` (Präfix `night_shift_message_`): guid/id, `fk_night_shift_session_guid`, `role smallint not null`, `content text not null`, `sort_order int not null`, `created_at`; unique `(fk_night_shift_session_guid, sort_order)`; Index auf `(fk_night_shift_session_guid, sort_order)`.

`simulation.night_shift_feedbacks` (Präfix `night_shift_feedback_`): guid/id, `fk_night_shift_session_guid unique`, `ok boolean`, `score_professional, score_rapport, score_empathy, score_listening, score_clarity smallint`, `text_professional, text_rapport, text_empathy, text_listening, text_clarity text`, `summary text`, `model text`, `prompt_version text`, `created_at`.

Keine `template`-Spalten in Phase 0 (kommen mit Phase 1).

## HTTP-Vertrag (muss exakt so bleiben; camelCase wie die anderen Controller)

| Aufruf | Anfrage | Antwort |
| --- | --- | --- |
| `GET rest/NightShift/cases?lang=de\|en` | – | `[{key, name, situation, lernziel}]` plus letzter Eintrag `{key:"random", name:"Zufallspatient"/"Random patient", situation:"Ein neuer, unbekannter Fall"/"A new, unknown case", lernziel:""}` |
| `POST rest/NightShift/start` | `{caseKey?: string, language?: "de"\|"en"}`; leer = zufälliger kuratierter Fall; `"random"` = generiert (Generator-Prompt; bei Fehler Rückfall auf zufälligen kuratierten Fall) | `{sessionId, temperament, firstMessage, ended:false, language, case:{key,name,situation,lernziel}, stammblatt:{geboren,geschlecht,aufnahme,diagnosen,allergien,medikation,pflegegrad,risiken,reanimation,angehoerige}}` |
| `POST rest/NightShift/chat/{sessionId:guid}` | `{text: string, kind: "say"\|"do"}` (ein nacktes JSON-String-Literal wird ebenfalls als `say` akzeptiert) | `{response, ended}`; nach `ended` → `-520002 SessionAlreadyEnded` |
| `POST rest/NightShift/finish/{sessionId:guid}` | `{}` | `{feedback (= summary), ok, scores:{fachlich,sympathie,empathie,zuhoeren,klarheit}, labels:{…in Session-Sprache}, perDimension:{…}, summary}`; ohne Schülerbeitrag `ok:false` mit Hinweistext; zweiter Aufruf liefert das gespeicherte Ergebnis ohne LLM-Aufruf |
| `GET rest/NightShift/sessions` | – | `[{sessionId, caseKey, caseName, startedAt, endedAt, finishedAt, ended, hasFeedback, turns}]` neueste zuerst, nur eigene |
| `GET rest/NightShift/sessions/{sessionId:guid}` | – | Summary + `messages:[{role:"patient"\|"student"\|"action", text, at}]` + `feedback` (wie finish) oder `null` |

Fehler: Session eines anderen Benutzers → `-520003 Forbidden`-Code (Status folgt der Backend-Regel); unbekannte Session `-520001`; leerer Beitrag `-520004`; ungültiger Fall `-520005`; LLM nicht erreichbar `-520006`. Die JSON-Schlüssel `stammblatt`, `lernziel`, `fachlich` usw. bleiben deutsch, weil das Frontend sie so erwartet.

## Verhalten (Direktiven)

- Sprache: `Language`-Enum intern, außen `"de"`/`"en"`; unbekannt → `de`. Fälle, Zufallskarte, Labels, Statusmeldungen und die Prompt-Sprachregel folgen der Session-Sprache (Logik 1:1 aus `SLAIS_Deploy/room701/Slais.Room701/Cases/Language.cs`).
- Prompt-Builder: Platzhalter `NAME, SITUATION, EMOTION, LERNZIEL, ANSPANNUNG, STAMMBLATT, SPRACHE` (kein `LEVEL`); `[[ENDE]]`-Erkennung entfernt den Marker und setzt `ended`; Do-Beiträge werden mit `[HANDLUNG DES PFLEGESCHUELERS] …` an das Modell und als Rolle `Action` mit Präfix `[Handlung] ` gespeichert; Verlauf auf die letzten 24 Nachrichten gekappt; Feedback-Parser klemmt Scores 0–10, ignoriert unbekannte Dimensionen, kaputtes JSON → `ok:false`.
- Start speichert System-Prompt (Rolle `System`, Sort 0) und Eröffnung (`Patient`, Sort 1); Chat hängt Schüler/Action + Patient an; Finish speichert Feedback, setzt `ended`/`finished_at`.
- `OpenAiLlmClient`: `HttpClient` typisiert, Timeout 60 s, `chat/completions`, Temperatur/MaxTokens/JSON-Modus als Parameter; Fehler → `LlmUnavailableException` (Infrastructure) → Handler wandelt in `-520006`.
- Autorisierung: Controller-Attribut schließt `Server` aus; Handler prüfen Besitz über `authentication.UserGuid`.

## Tests (müssen existieren)

Domain: Session-Fabrik lehnt leeren Key/Opener ab; `MarkEnded` idempotent; Feedback-Fabrik klemmt Scores. Integration (FakeLlmClient): `Cases_AsStudent_ShouldListCuratedAndRandom`, `Cases_WithEnglish_ShouldReturnEnglishNames`, `Start_WithKeller_ShouldReturnOpenerAndStammblatt`, `Chat_WithDo_ShouldStoreActionRole`, `Chat_AfterEnde_ShouldReturnSessionAlreadyEnded`, `Chat_AsOtherUser_ShouldReturnForbiddenCode`, `Finish_Twice_ShouldCallLlmOnce`, `Finish_WithoutStudentTurn_ShouldReturnNotOk`, `Sessions_ShouldListOnlyOwn`, `Start_AsServer_ShouldBeRejected`. Architecture-Tests bleiben grün.

## Nicht in Phase 0

Vorlagen in der DB, Übersetzung, Admin-API, `system.prompts` als Prompt-Quelle (Folgeaufgabe, weil dort noch keine Entity existiert), Gateway-Umschaltung (macht Claude im Deploy-Repo nach Abnahme).
