alter table simulation.night_shift_sessions
    add column if not exists fk_night_shift_template_guid uuid;

create index if not exists idx_night_shift_sessions_fk_night_shift_template_guid
    on simulation.night_shift_sessions(fk_night_shift_template_guid);
