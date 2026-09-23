create table if not exists simulation.night_shift_templates
(
    night_shift_template_guid     uuid        primary key default gen_random_uuid(),
    night_shift_template_id       int         generated always as identity unique,
    key                           text        not null,
    state                         smallint    not null default 1,
    tension_start                 smallint    not null,
    sort_order                    int         not null default 100,
    created_at                    timestamptz not null default now(),
    created_by_user_guid          uuid,
    updated_at                    timestamptz,
    updated_by_user_guid          uuid,
    deleted_at                    timestamptz,
    deleted_by_user_guid          uuid
    );

create unique index if not exists idx_night_shift_templates_key
    on simulation.night_shift_templates(key) where state <> 3;

create index if not exists idx_night_shift_templates_state_sort_order
    on simulation.night_shift_templates(state, sort_order);

create index if not exists idx_night_shift_templates_created_by
    on simulation.night_shift_templates(created_by_user_guid);

create index if not exists idx_night_shift_templates_updated_by
    on simulation.night_shift_templates(updated_by_user_guid);

create index if not exists idx_night_shift_templates_deleted_by
    on simulation.night_shift_templates(deleted_by_user_guid);

create table if not exists simulation.night_shift_template_texts
(
    night_shift_template_text_guid    uuid        primary key default gen_random_uuid(),
    night_shift_template_text_id      int         generated always as identity unique,
    fk_night_shift_template_guid      uuid        not null,
    language                          smallint    not null,
    name                              text        not null,
    situation                         text        not null,
    emotion                           text        not null,
    learning_goal                     text        not null,
    opener                            text        not null,
    born                              text        not null,
    gender                            text        not null,
    admission                         text        not null,
    diagnoses                         text        not null,
    allergies                         text        not null,
    medication                        text        not null,
    care_level                        text        not null,
    risks                             text        not null,
    resuscitation                     text        not null,
    relatives                         text        not null,
    created_at                        timestamptz not null default now(),
    created_by_user_guid              uuid,
    updated_at                        timestamptz,
    updated_by_user_guid              uuid,

    constraint fk_night_shift_template_texts_template
    foreign key (fk_night_shift_template_guid)
    references simulation.night_shift_templates(night_shift_template_guid)
    on delete cascade
    );

create unique index if not exists idx_night_shift_template_texts_template_language
    on simulation.night_shift_template_texts(fk_night_shift_template_guid, language);
