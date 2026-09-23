create table if not exists simulation.night_shift_sessions
(
    night_shift_session_guid     uuid        primary key default gen_random_uuid(),
    night_shift_session_id       int         generated always as identity unique,
    fk_user_guid                 uuid        not null,
    language                     smallint    not null,
    case_key                     text        not null,
    case_name                    text        not null,
    case_situation               text        not null,
    case_emotion                 text        not null,
    case_learning_goal           text        not null,
    case_opener                  text        not null,
    case_tension_start           smallint    not null,
    record_born                  text        not null,
    record_gender                text        not null,
    record_admission             text        not null,
    record_diagnoses             text        not null,
    record_allergies             text        not null,
    record_medication            text        not null,
    record_care_level            text        not null,
    record_risks                 text        not null,
    record_resuscitation         text        not null,
    record_relatives             text        not null,
    prompt_version               text        not null,
    model                        text        not null,
    started_at                   timestamptz not null,
    ended_at                     timestamptz,
    finished_at                  timestamptz,
    ended                        boolean     not null default false,
    created_at                   timestamptz not null default now(),
    created_by_user_guid         uuid,

    constraint fk_night_shift_sessions_user
    foreign key (fk_user_guid)
    references public.users(user_guid)
    on delete restrict
    );

create index if not exists idx_night_shift_sessions_fk_user_guid
    on simulation.night_shift_sessions(fk_user_guid);

create index if not exists idx_night_shift_sessions_started_at
    on simulation.night_shift_sessions(started_at);

create index if not exists idx_night_shift_sessions_created_by
    on simulation.night_shift_sessions(created_by_user_guid);

create table if not exists simulation.night_shift_messages
(
    night_shift_message_guid         uuid        primary key default gen_random_uuid(),
    night_shift_message_id           int         generated always as identity unique,
    fk_night_shift_session_guid      uuid        not null,
    role                             smallint    not null,
    content                          text        not null,
    sort_order                       int         not null,
    created_at                       timestamptz not null default now(),

    constraint fk_night_shift_messages_session
    foreign key (fk_night_shift_session_guid)
    references simulation.night_shift_sessions(night_shift_session_guid)
    on delete cascade
    );

create unique index if not exists idx_night_shift_messages_session_sort
    on simulation.night_shift_messages(fk_night_shift_session_guid, sort_order);

create table if not exists simulation.night_shift_feedbacks
(
    night_shift_feedback_guid        uuid        primary key default gen_random_uuid(),
    night_shift_feedback_id          int         generated always as identity unique,
    fk_night_shift_session_guid      uuid        not null unique,
    ok                               boolean     not null,
    score_professional               smallint    not null,
    score_rapport                    smallint    not null,
    score_empathy                    smallint    not null,
    score_listening                  smallint    not null,
    score_clarity                    smallint    not null,
    text_professional                text        not null,
    text_rapport                     text        not null,
    text_empathy                     text        not null,
    text_listening                   text        not null,
    text_clarity                     text        not null,
    summary                          text        not null,
    model                            text        not null,
    prompt_version                   text        not null,
    created_at                       timestamptz not null default now(),

    constraint fk_night_shift_feedbacks_session
    foreign key (fk_night_shift_session_guid)
    references simulation.night_shift_sessions(night_shift_session_guid)
    on delete cascade
    );

create unique index if not exists idx_night_shift_feedbacks_session
    on simulation.night_shift_feedbacks(fk_night_shift_session_guid);
