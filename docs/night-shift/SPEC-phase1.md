# Spec Phase 1: Fallvorlagen aus der Datenbank

Stand 2026-09-23. Branch `room701/night-shift-phase1` (auf `room701/night-shift-phase0`), PR zielt auf den Phase-0-Branch. Fachkonzept: `SLAIS_Deploy/docs/KONZEPT-room701-fallvorlagen.md`. Regeln, Stil und Gate wie in `SPEC-phase0.md` (Abschnitt „Regeln"); Fortschritt in `PROGRESS-phase1.md`.

## Ziel

`GET rest/NightShift/cases` und `POST rest/NightShift/start` lesen die Fälle aus der Datenbank statt aus `CuratedCases.cs`. Die fünf heutigen Fälle (keller, schmidt, yilmaz, entzug, vogel) werden per Migration eingespielt, beide Sprachen, Zustand aktiv. Für Schüler ändert sich nichts Sichtbares; jede Session merkt sich, aus welcher Vorlage sie stammt. Verwaltung (Anlegen, Bearbeiten, Übersetzen) ist Phase 2.

## Datenmodell (Schema `simulation`)

`night_shift_templates` (Präfix `night_shift_template_`): `night_shift_template_guid uuid pk`, `night_shift_template_id int identity unique`, `key text not null`, `state smallint not null default 1` (Enum `States`: Active=0, Pending=1, Deactived=2, Deleted=3), `tension_start smallint not null` (0–10), `sort_order int not null default 100`, Audit-Spalten der Kette `BaseDeleteByEntity` (`created_at`, `created_by_user_guid`, `updated_at`, `updated_by_user_guid`, `deleted_at`, `deleted_by_user_guid`; Spaltennamen wie `public.users`). Eindeutigkeit: `create unique index if not exists idx_night_shift_templates_key on simulation.night_shift_templates (key) where deleted_at is null`. Index `idx_night_shift_templates_state_sort_order (state, sort_order)`.

`night_shift_template_texts` (Präfix `night_shift_template_text_`): guid/id, `fk_night_shift_template_guid uuid not null`, `language smallint not null`, `name, situation, emotion, learning_goal, opener, born, gender, admission, diagnoses, allergies, medication, care_level, risks, resuscitation, relatives text not null`, Audit-Spalten der Kette `BaseUpdatedByEntity`. Unique `idx_night_shift_template_texts_template_language (fk_night_shift_template_guid, language)`, FK `fk_night_shift_template_texts_template` mit `on delete cascade`.

`night_shift_sessions`: neue Spalte `fk_night_shift_template_guid uuid` (nullable, kein FK-Constraint, damit gelöschte Vorlagen alte Sessions nicht blockieren), Index `idx_night_shift_sessions_fk_night_shift_template_guid` — Mapping und Migration `V2026_09_01` liegen schon in WP1, weil EF die Eigenschaft sonst per Konvention auf eine fehlende Spalte mappt.

Migrationen: `V2026_09_01__Add_Night_Shift_Session_Template_Guid.sql` (Spalte + Index an Sessions, bereits mit WP1), `V2026_09_02__Create_Night_Shift_Templates.sql` (Tabellen, Indizes) und `V2026_09_03__Seed_Night_Shift_Templates.sql` (fünf Vorlagen × zwei Sprachen, Daten 1:1 aus `docs/night-shift/reference/CuratedCases.cs` inkl. Fallback „Vogel" als `key = 'vogel'`; `sort_order` 10/20/30/40/50; `created_by_user_guid` null; idempotent über `insert … on conflict (key) where deleted_at is null do nothing` bzw. `on conflict (fk_night_shift_template_guid, language) do nothing`; feste GUIDs je Vorlage, damit der Seed wiederholbar ist).

## Domain (`Domain/Simulations/NightShift/`)

- `NightShiftTemplateNavigationPropertyEntity : BaseDeleteByEntity` — nur `Texts` (ICollection<NightShiftTemplateTextEntity>).
- `NightShiftTemplateEntity : NightShiftTemplateNavigationPropertyEntity` — `Key`, `State` (States), `TensionStart` (short), `SortOrder` (int). Fabrik `CreateDraft(Guid? createdByUserGuid, string key, short tensionStart, int sortOrder)` → State `Pending`. Verhalten: `UpdateHead(Guid? updatedByUserGuid, string key, short tensionStart, int sortOrder)`, `AddOrReplaceText(NightShiftTemplateTextEntity text)` (ersetzt den Text derselben Sprache), `Activate(Guid? updatedByUserGuid)` (wirft `TemplateActivationNeedsBothLanguages`, wenn nicht Deutsch und Englisch vorhanden), `Archive(Guid? updatedByUserGuid)` → `Deactived`, `MarkDeleted(Guid deletedByUserGuid)` → `Deleted` + Delete-Audit (Methodenname der Basisklasse prüfen und wiederverwenden), `GetText(Language)` und `GetTextOrFallback(Language)` (Rückfall auf Deutsch, liefert Tupel/Objekt mit `IsFallback`). Regeln in `CheckInputs`: Key `^[a-z0-9-]{2,40}$` sonst `TemplateKeyInvalid`; Anspannung 0–10 sonst `InvalidInput`; Sortierung ≥ 0.
- `NightShiftTemplateTextEntity : BaseUpdatedByEntity` — `TemplateGuid`, `Language`, die 15 Textfelder, Navigation `Template`. Fabrik `Create(Guid? createdByUserGuid, Guid templateGuid, Language language, …15 Felder…)`. Regeln: Name ≤ 60, Situation/LearningGoal/Opener ≤ 200, Emotion ≤ 120, Stammblattfelder ≤ 140, alle nicht leer (`TemplateTextTooLong` / `InvalidInput`); enthält ein Feld `{{` oder `[[ENDE]]` → `TemplateTextContainsMarker`.
- `NightShiftSessionEntity`: neue Eigenschaft `TemplateGuid` (Guid?), Fabrik `Create` bekommt am Ende den optionalen Parameter `Guid? templateGuid = null`.
- `NightShiftErrorCodes` (Domain, weiter ab -530006): `TemplateKeyInvalid = -530006`, `TemplateTextTooLong = -530007`, `TemplateTextContainsMarker = -530008`, `TemplateActivationNeedsBothLanguages = -530009`, `TemplateStateInvalid = -530010`.

## Application

- `Application/Common/Interfaces/Repositorys/INightShiftTemplateRepository : IBaseRepository<NightShiftTemplateEntity>`: `Task<List<NightShiftTemplateEntity>> GetActiveAsync()` (State Active, nicht gelöscht, mit Texts, sortiert `SortOrder, Key`), `Task<NightShiftTemplateEntity?> GetActiveByKeyAsync(string key)`, `Task<NightShiftTemplateEntity?> GetByGuidAsync(Guid templateGuid)` (alle Zustände außer Deleted, mit Texts), `Task<List<NightShiftTemplateEntity>> GetAllAsync()` (außer Deleted, mit Texts, für Phase 2).
- `Application/Simulations/NightShift/Cases/PatientCase.cs` bleibt; neu `Application/Simulations/NightShift/Cases/PatientCaseFactory.cs` (static): `FromTemplate(NightShiftTemplateEntity template, Language language)` → PatientCase aus `GetTextOrFallback(language)`; Rückgabe enthält `IsLanguageFallback`. `CuratedCases.cs` wird **gelöscht**; `CaseSanitizer.FromJson(string json, PatientCase fallback)` nimmt den Fallback als Parameter.
- `GetNightShiftCasesQueryHandler`: aktive Vorlagen aus dem Repository in der Sprache; DTO `NightShiftCaseResponseDto` bekommt `LanguageFallback` (bool); Zufallskarte bleibt der letzte Eintrag. Keine aktiven Vorlagen → leere Liste plus Zufallskarte.
- `StartNightShiftSessionCommandHandler`: leerer Key → zufällige aktive Vorlage (`NoActiveTemplates = -520008`, wenn keine); `"random"` → Generator wie bisher, Fallback = zufällige aktive Vorlage, ohne aktive Vorlage bei LLM-Ausfall → `-520008`; sonst `GetActiveByKeyAsync`, null → `InvalidCase`. Session wird mit `templateGuid` erzeugt (null bei generiertem Fall).
- `NightShiftErrorCodes` (Application): `NoActiveTemplates = -520008`.
- DTO `NightShiftSessionSummaryResponseDto` bekommt `TemplateId` (Guid?, JSON `templateId`); Mapper ergänzen.

## Infrastructure

- EF-Konfigurationen `NightShiftTemplateEntityConfig/` (Basis `BaseDeletedByEntityConfig`, Tabelle `night_shift_templates`, Schema `simulation`, Präfix `night_shift_template_`; FKs Texts cascade; Indizes wie oben, Unique auf Key mit Filter `deleted_at is null` per `.HasFilter`) und `NightShiftTemplateTextEntityConfig/` (Basis `BaseUpdatedByEntityConfig`). Session-Config: Spalte `fk_night_shift_template_guid` mappen, Index.
- `Repositorys/NightShiftTemplateRepository.cs` nach Vorbild `UserRepository`/`NightShiftSessionRepository`; Registrierung in `AddRepositories`.
- Migrationen wie oben; Seed-Daten sorgfältig aus der Referenz übernehmen (deutsche Umlaute wie dort, englische Fassung ebenso).

## Presentation

Keine neuen Endpunkte. `NightShiftController` unverändert.

## Tests

- Domain: `NightShiftTemplateTests` — `CreateDraft_WithInvalidKey_ShouldThrowTemplateKeyInvalid`, `Activate_WithoutEnglish_ShouldThrowActivationNeedsBothLanguages`, `Activate_WithBothLanguages_ShouldSetActive`, `AddOrReplaceText_SameLanguage_ShouldReplace`, `GetTextOrFallback_MissingEnglish_ShouldReturnGermanWithFallbackFlag`; `NightShiftTemplateTextTests` — `Create_WithEndMarker_ShouldThrowTemplateTextContainsMarker`, `Create_WithNameTooLong_ShouldThrowTemplateTextTooLong`.
- `Tests.Domain.Shared`: `Builders/NightShiftTemplateEntityBuilder.cs` (Vorlage mit DE-Text, optional EN), `TestDataCreator/NightShiftTemplateTestData.cs` mit `CreateCuratedTemplates()` → die fünf Vorlagen (Daten 1:1 wie der Seed) als Entities, aktiv.
- Integration: `TestBase` leert vor jedem Test alle Tabellen, deshalb neuer Helfer `Integration.Tests/Common/NightShiftTemplateTestRepository` (Stil `InstituteTestRepository`) mit `SeedCuratedAsync()` und `CreateAsync(NightShiftTemplateEntity)`. Bestehende NightShift-Tests rufen `SeedCuratedAsync()` vorab auf. Neue Tests: `Cases_ShouldNotListPendingOrArchivedTemplates`, `Cases_ShouldOrderBySortOrder`, `Cases_WithEnglish_WhenEnglishTextMissing_ShouldFallbackToGermanAndFlagIt`, `Start_WithArchivedKey_ShouldReturnInvalidCase`, `Start_WithoutActiveTemplates_ShouldReturnNoActiveTemplates`, `Start_ShouldStoreTemplateGuidOnSession` (über `GET sessions` → `templateId`). Prompt-Test nutzt `NightShiftTemplateTestData` statt `CuratedCases`.
- Zusätzlich ein Integration-Test, der belegt, dass der SQL-Seed gelaufen ist, ist nicht möglich (Truncate); stattdessen prüft `Migration_ShouldCreateTemplateTables` per `_dbContext.Database.ExecuteSqlRaw`/Abfrage auf `information_schema.tables`, dass beide Tabellen und die Spalte `fk_night_shift_template_guid` existieren.

## Arbeitspakete

| Paket | Inhalt |
| --- | --- |
| WP1 | Domain: zwei Entities + Navigation, Session-Erweiterung, Fehlercodes, Domain-Tests, Builder + TestData |
| WP2 | Application: Repository-Interface, PatientCaseFactory, Handler-Umbau (cases/start), CaseSanitizer-Signatur, CuratedCases löschen, DTO/Mapper |
| WP3 | Infrastructure: EF-Konfigurationen, zwei Migrationen (Tabellen, Seed), Repository, DI |
| WP4 | Integration-Tests: Test-Repository, bestehende Tests anpassen, neue Tests |
| WP5 | Abschluss: Gate, README-Abschnitt „Vorlagen", Sol-Adversarial, Draft-PR gegen `room701/night-shift-phase0` |

## Nicht in Phase 1

Admin-Endpunkte, Übersetzungsvorschlag, „Zufallsfall als Vorlage", Testen von Entwürfen, Frontend.
