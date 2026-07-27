-- ============================================================
-- AgencyOS
-- Migration: 20260726180000_create_working_calendar_tables.sql
-- Release 1.1 / US-101
-- Domain: Working Calendar
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: working_calendar
-- ============================================================

create table public.working_calendar (

    id uuid not null default gen_random_uuid(),

    company_id uuid not null,

    name varchar(200) not null,

    status varchar(30) not null,

    effective_from date not null,

    effective_to date,

    working_days text[] not null,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_working_calendar
        primary key (id),

    constraint chk_working_calendar_name
        check (char_length(trim(name)) > 0),

    constraint chk_working_calendar_status
        check (status in ('Active', 'Inactive')),

    constraint chk_working_calendar_period
        check (effective_to is null or effective_to >= effective_from),

    constraint chk_working_calendar_working_days
        check (cardinality(working_days) > 0)

);

comment on table public.working_calendar is
'Company Working Calendar used as the operational calendar for Capacity Planning';

comment on column public.working_calendar.working_days is
'Canonical weekday names: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_working_calendar_company_id
    on public.working_calendar(company_id);

create index idx_working_calendar_status
    on public.working_calendar(status);

create index idx_working_calendar_company_status
    on public.working_calendar(company_id, status);

create index idx_working_calendar_effective_from
    on public.working_calendar(effective_from);

create index idx_working_calendar_period
    on public.working_calendar(company_id, effective_from, effective_to);

create index idx_working_calendar_name
    on public.working_calendar(name);

create index idx_working_calendar_working_days
    on public.working_calendar using gin (working_days);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
