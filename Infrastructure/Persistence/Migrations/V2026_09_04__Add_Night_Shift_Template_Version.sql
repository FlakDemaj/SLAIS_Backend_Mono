alter table simulation.night_shift_templates
    add column if not exists version int not null default 1;
