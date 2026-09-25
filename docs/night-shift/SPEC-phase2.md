# Spec Phase 2: Verwaltung der Fallvorlagen (Admin-API)

Stand 2026-09-25. Branch `room701/night-shift-phase2` (auf `room701/night-shift-phase1`), PR zielt auf den Phase-1-Branch. Fachkonzept: `SLAIS_Deploy/docs/KONZEPT-room701-fallvorlagen.md`. Regeln, Stil und Gate wie `SPEC-phase0.md`; Fortschritt in `PROGRESS-phase2.md`. Entscheidungen von Marvin (2026-09-23): Admin und SuperAdmin gleichwertig, KI-Übersetzungsvorschlag, Startfälle ohne Sonderstatus, keine Historientabelle, Zufallsfälle als Vorlage übernehmbar, kein Schwierigkeitsgrad.

## Ziel

Admins legen Vorlagen an, bearbeiten, übersetzen, aktivieren, archivieren und löschen sie über `rest/NightShiftTemplate`, testen Entwürfe als echte Session und übernehmen KI-generierte Zufallsfälle als Entwurf. Schülerpfad (`rest/NightShift`) bleibt unverändert bis auf `start` mit `templateId`.

## Domain (`Domain/Simulations/NightShift/`)

- `NightShiftTemplateEntity`: neue Eigenschaft `Version` (int, Start 1). Jede Änderung über `UpdateHead`, `AddOrReplaceText`, `Activate`, `Archive`, `Reopen`, `MarkDeleted` erhöht `Version` um 1. Neu `Reopen(Guid? updatedByUserGuid)`: `Deactived` oder `Active` → `Pending`. Zustandsregeln (`TemplateStateInvalid`): aus `Deleted` keine Änderung; `Activate` nur aus `Pending`/`Deactived`; `Archive` nur aus `Active`/`Pending`; `MarkDeleted` aus jedem Zustand außer `Deleted`. `EnsureEditable()` wirft `TemplateStateInvalid` bei `Deleted`.
- `NightShiftTemplateTextEntity`: `Update(Guid? updatedByUserGuid, …15 Felder…)` mit denselben Regeln wie `Create`.
- Fehlercodes Domain: keine neuen.

## Datenmodell

Migration `V2026_09_04__Add_Night_Shift_Template_Version.sql`: `alter table simulation.night_shift_templates add column if not exists version int not null default 1;` EF-Mapping `version` in `NightShiftTemplateEntityAttributeConfig`.

## Application

### Fehlercodes (`NightShiftErrorCodes`, Application, weiter ab -520009)

`TemplateModifiedByOtherUser = -520009`, `TranslationUnavailable = -520010`, `TemplateKeyAlreadyExists = -520011`, `TemplateNotFound = -520012`, `SessionIsNotRandomCase = -520013`, `TemplateNotActive = -520014`, `TemplateStateTransitionInvalid = -520015` (wenn die Domain `TemplateStateInvalid` wirft, wird sie nicht umgemappt; dieser Code ist für ungültige Zielzustände im Request).

### Schnittstellen

- `INightShiftTemplateRepository` ergänzen: `Task<NightShiftTemplateEntity?> GetByKeyAsync(string key)` (alle Zustände außer `Deleted`), `Task<NightShiftTemplateEntity?> GetByGuidAsync` existiert.
- `INightShiftPromptBuilder`: `string BuildTranslationPrompt(Language source, Language target)`.
- `INightShiftSessionRepository`: `GetByGuidAsync` existiert (Session eines beliebigen Benutzers lesbar für Admin-Übernahme).

### DTOs (`Application/Common/DTOs/Simulations/NightShift/`)

- `NightShiftTemplateTextDto` { Name, Situation, Emotion, LearningGoal, Opener, Born, Gender, Admission, Diagnoses, Allergies, Medication, CareLevel, Risks, Resuscitation, Relatives } (alle string, `required`).
- `GetNightShiftTemplateResponseDto` { TemplateId (Guid), Key, State (string: `pending|active|archived`), TensionStart (short), SortOrder (int), Version (int), CreatedAt, UpdatedAt (DateTime?), Texts (Dictionary<string, NightShiftTemplateTextDto>, Schlüssel `de`/`en`) }.
- Mapper `NightShiftMapperProfile`: Entity → `GetNightShiftTemplateResponseDto` (State-Mapping `Pending→pending`, `Active→active`, `Deactived→archived`), TextEntity → `NightShiftTemplateTextDto`.

### Use Cases (`Application/Simulations/NightShift/Templates/`)

| Ordner | Anfrage | Antwort | Regeln |
| --- | --- | --- | --- |
| `Querys/GetNightShiftTemplates` | `GetNightShiftTemplatesQuery : INoTransaction` | `List<GetNightShiftTemplateResponseDto>` | alle außer `Deleted`, Reihenfolge `SortOrder, Key` |
| `Querys/GetNightShiftTemplate` | `GetNightShiftTemplateQuery { TemplateGuid } : INoTransaction` | `GetNightShiftTemplateResponseDto` | `TemplateNotFound` |
| `Commands/CreateNightShiftTemplate` | `{ Key, TensionStart, SortOrder, German (NightShiftTemplateTextDto?), English (NightShiftTemplateTextDto?) }` | `CreateObjectResponseDto` | Key normalisiert (trim, lowercase); `GetByKeyAsync` belegt → `TemplateKeyAlreadyExists`; Entwurf; Texte, die mitkommen, werden angelegt |
| `Commands/UpdateNightShiftTemplate` | `{ TemplateGuid, ExpectedVersion, Key, TensionStart, SortOrder, German?, English? }` | `GetNightShiftTemplateResponseDto` | `TemplateNotFound`; `ExpectedVersion != Version` → `TemplateModifiedByOtherUser`; Key-Änderung nur im Zustand `Pending`, sonst `TemplateStateInvalid` (Domain); Key belegt durch andere Vorlage → `TemplateKeyAlreadyExists`; mitgelieferte Texte ersetzen (`AddOrReplaceText`), fehlende bleiben |
| `Commands/SetNightShiftTemplateState` | `{ TemplateGuid, ExpectedVersion, State (string) }` | `GetNightShiftTemplateResponseDto` | `active` → `Activate`, `archived` → `Archive`, `pending` → `Reopen`; anderer Wert → `TemplateStateTransitionInvalid` |
| `Commands/DeleteNightShiftTemplate` | `{ TemplateGuid }` | `CreateObjectResponseDto` (Success, ObjectGuid) | Soft Delete über `MarkDeleted`; laufende Sessions unberührt |
| `Commands/SuggestNightShiftTemplateTranslation` | `{ SourceLanguage (string), TargetLanguage (string), Text (NightShiftTemplateTextDto) } : INoTransaction` | `NightShiftTemplateTextDto` | Quelle == Ziel → `InvalidRequest`; `ILlmClient.ChatAsync(system = BuildTranslationPrompt, user = JSON der 15 Felder, temperature 0.2, maxTokens 900, jsonMode true, tag "translate")`; Antwort-JSON mit denselben 15 Schlüsseln (snake_case wie das Stammblatt: `name, situation, emotion, learning_goal, opener, born, gender, admission, diagnoses, allergies, medication, care_level, risks, resuscitation, relatives`) parsen; fehlendes Feld oder kaputtes JSON → `TranslationUnavailable`; Ergebnis durch dieselben Längen-/Markerregeln wie die Entity (`NightShiftTemplateTextEntity.Create` mit Dummy-Guid als Validierung oder eine statische `Validate`) → bei Verstoß `TranslationUnavailable` |
| `Commands/CreateNightShiftTemplateFromSession` | `{ SessionGuid, Key }` | `CreateObjectResponseDto` | Session per `INightShiftSessionRepository.GetByGuidAsync`, fehlt → `SessionNotFound`; `TemplateGuid != null` oder `CaseKey != "random"` → `SessionIsNotRandomCase`; Key-Regeln wie Create; Entwurf mit Kopf aus der Session (`CaseTensionStart`, SortOrder 100) und einem Text in der Session-Sprache aus den `Case*`/`Record*`-Spalten |

Schülerpfad: `StartNightShiftSessionCommand` bekommt `TemplateId` (Guid?). Wenn gesetzt: `GetByGuidAsync`; fehlt → `TemplateNotFound`; nicht `Active` und Rolle nicht Admin/SuperAdmin → `TemplateNotActive`; Admin/SuperAdmin dürfen `Pending`/`Deactived` starten („Testen"). `CaseKey` wird ignoriert, wenn `TemplateId` gesetzt ist.

Autorisierung: alle Template-Handler prüfen `authentication.UserRole` ∈ {Admin, SuperAdmin}, sonst `Forbidden` (zusätzlich zum Controller-Attribut).

## Infrastructure

- Migration `V2026_09_04` (oben); EF-Mapping `version`.
- `NightShiftTemplateRepository.GetByKeyAsync`.
- `Prompts/translate.md` (eingebettet, Version `2026-09-25.1` im Kopf): Rolle Fachübersetzer Pflege, Quelle `{{QUELLE}}`, Ziel `{{ZIEL}}`, Regeln: Ton und Register erhalten, medizinische Begriffe korrekt, keine Ergänzungen, Datumsformat `TT.MM.JJJJ` ↔ `DD/MM/YYYY`, Pflegegrad `kein` ↔ `none`, Anrede `Herr/Frau` ↔ `Mr/Mrs`, Ausgabe NUR JSON mit den 15 Schlüsseln. `NightShiftPromptBuilder.BuildTranslationPrompt` ersetzt `QUELLE`/`ZIEL` mit `Deutsch`/`Englisch`.

## Presentation

`Presentation/Controllers/Simulations/NightShift/NightShiftTemplateController.cs`, Route `rest/NightShiftTemplate`, `[Authorize(Roles = $"{Role.Admin},{Role.SuperAdmin}")]`:

| Verb | Route | Handler |
| --- | --- | --- |
| GET | `` | GetNightShiftTemplatesQuery |
| GET | `{templateGuid:guid}` | GetNightShiftTemplateQuery |
| POST | `` | CreateNightShiftTemplateCommand |
| PUT | `{templateGuid:guid}` | UpdateNightShiftTemplateCommand (Guid aus der Route) |
| POST | `{templateGuid:guid}/state` | SetNightShiftTemplateStateCommand |
| DELETE | `{templateGuid:guid}` | DeleteNightShiftTemplateCommand |
| POST | `translate` | SuggestNightShiftTemplateTranslationCommand |
| POST | `from-session/{sessionGuid:guid}` | CreateNightShiftTemplateFromSessionCommand (Body `{ key }`) |

`NightShiftController.StartAsync` unverändert (Command trägt `TemplateId`). Fehlerkörper wie üblich; Rollenverstoß am Controller liefert 403 (ASP.NET), im Handler `-520003`.

## Tests

- Domain: `Version_ShouldIncrementOnEveryChange`, `Activate_FromDeleted_ShouldThrowTemplateStateInvalid`, `Reopen_FromArchived_ShouldSetPending`, `Text_Update_WithMarker_ShouldThrow`.
- Integration (`Integration.Tests/Simulations/NightShift/NightShiftTemplateControllerTests.cs`, Admin-User via Builder, FakeLlmClient bekommt einen Translation-Zweig: System-Prompt enthält `Fachübersetzer` → liefert JSON mit den 15 Schlüsseln, Name `Mr Keller, 58`): `Create_AsAdmin_ShouldReturnPendingTemplate`, `Create_AsStudent_ShouldReturnForbidden` (403 vom Controller), `Create_WithExistingKey_ShouldReturnKeyAlreadyExists`, `Update_WithStaleVersion_ShouldReturnModifiedByOtherUser`, `Update_ShouldReplaceEnglishText`, `SetState_ActiveWithoutEnglish_ShouldReturnActivationNeedsBothLanguages` (Domain `-530009`), `SetState_Active_ShouldListInStudentCases`, `Delete_ShouldHideFromListAndStudentCases`, `Translate_ShouldReturnEnglishText`, `Translate_WhenLlmFails_ShouldReturnTranslationUnavailable`, `FromSession_OnRandomSession_ShouldCreateDraft` (Student startet `random` mit Fake-Generator, Admin übernimmt → Entwurf mit Text in der Session-Sprache), `FromSession_OnTemplateSession_ShouldReturnSessionIsNotRandomCase`, `Start_PendingTemplate_AsAdmin_ShouldWork`, `Start_PendingTemplate_AsStudent_ShouldReturnTemplateNotActive`.
- `Routings.cs` ergänzen.

## Arbeitspakete

| Paket | Inhalt |
| --- | --- |
| WP1 | Domain: `Version`, Zustandsregeln, `Reopen`, `EnsureEditable`, `Text.Update`, Domain-Tests |
| WP2+3 | Application (Fehlercodes, DTOs, Mapper, acht Use Cases, `TemplateId` beim Start, Übersetzungsparser) und Infrastructure (Migration, Mapping, `GetByKeyAsync`, `translate.md`, PromptBuilder) in einem Lauf |
| WP4 | Presentation (Controller, Routings) + Integration-Tests + FakeLlmClient-Zweig |
| WP5 | Gate, README-Abschnitt „Verwaltung", Sol-Adversarial, Draft-PR gegen `room701/night-shift-phase1`, Stack neu bauen, Endpunkte per curl prüfen |

## Nicht in Phase 2

Frontend (Phase 3), Historie/Wiederherstellen, Prompts aus `system.prompts`, Mandantenbezug.
