-- ============================================================
-- AgencyOS
-- Migration: 20260726200000_create_working_hours_tables.sql
-- Release 1.1 / US-103
-- Domain: Working Hours
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: working_hours
-- ============================================================

create table public.working_hours (

    id uuid not null default gen_random_uuid(),

    working_calendar_id uuid not null,

    name varchar(200) not null,

    status varchar(30) not null,

    effective_from date not null,

    effective_to date,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_working_hours
        primary key (id),

    constraint fk_working_hours_working_calendar
        foreign key (working_calendar_id)
        references public.working_calendar (id)
        on delete restrict,

    constraint chk_working_hours_name
        check (char_length(trim(name)) > 0),

    constraint chk_working_hours_status
        check (status in ('Active', 'Inactive')),

    constraint chk_working_hours_period
        check (effective_to is null or effective_to >= effective_from)

);

comment on table public.working_hours is
'Standard Working Hours associated with a Working Calendar for Capacity Planning';

-- ============================================================
-- TABLE: working_hours_day
-- ============================================================

create table public.working_hours_day (

    id uuid not null default gen_random_uuid(),

    working_hours_id uuid not null,

    day_of_week varchar(20) not null,

    enabled boolean not null default false,

    start_time time,

    end_time time,

    break_start time,

    break_end time,

    constraint pk_working_hours_day
        primary key (id),

    constraint fk_working_hours_day_working_hours
        foreign key (working_hours_id)
        references public.working_hours (id)
        on delete cascade,

    constraint uq_working_hours_day
        unique (working_hours_id, day_of_week),

    constraint chk_working_hours_day_of_week
        check (day_of_week in (
            'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'
        )),

    constraint chk_working_hours_day_times
        check (
            enabled = false
            or (
                start_time is not null
                and end_time is not null
                and start_time < end_time
            )
        ),

    constraint chk_working_hours_day_break_pair
        check (
            (break_start is null and break_end is null)
            or (break_start is not null and break_end is not null)
        ),

    constraint chk_working_hours_day_break_order
        check (
            break_start is null
            or break_start < break_end
        ),

    constraint chk_working_hours_day_break_inside
        check (
            break_start is null
            or (
                start_time is not null
                and end_time is not null
                and break_start >= start_time
                and break_end <= end_time
            )
        )

);

comment on table public.working_hours_day is
'Weekday schedule entries for a Working Hours configuration';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_working_hours_calendar_id
    on public.working_hours(working_calendar_id);

create index idx_working_hours_status
    on public.working_hours(status);

create index idx_working_hours_calendar_status
    on public.working_hours(working_calendar_id, status);

create index idx_working_hours_effective_from
    on public.working_hours(effective_from);

create index idx_working_hours_period
    on public.working_hours(working_calendar_id, effective_from, effective_to);

create index idx_working_hours_name
    on public.working_hours(name);

create index idx_working_hours_day_working_hours_id
    on public.working_hours_day(working_hours_id);

create index idx_working_hours_day_of_week
    on public.working_hours_day(day_of_week);

create index idx_working_hours_day_enabled
    on public.working_hours_day(enabled);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
