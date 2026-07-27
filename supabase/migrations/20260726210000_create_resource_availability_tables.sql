-- ============================================================
-- AgencyOS
-- Migration: 20260726210000_create_resource_availability_tables.sql
-- Release 1.1 / US-104
-- Domain: Resource Availability
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: resource_availability
-- ============================================================

create table public.resource_availability (

    id uuid not null default gen_random_uuid(),

    execution_resource_id uuid not null,

    working_calendar_id uuid not null,

    working_hours_id uuid not null,

    name varchar(200) not null,

    status varchar(30) not null,

    effective_from date not null,

    effective_to date,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_resource_availability
        primary key (id),

    constraint fk_resource_availability_execution_resource
        foreign key (execution_resource_id)
        references public.execution_resource (id)
        on delete restrict,

    constraint fk_resource_availability_working_calendar
        foreign key (working_calendar_id)
        references public.working_calendar (id)
        on delete restrict,

    constraint fk_resource_availability_working_hours
        foreign key (working_hours_id)
        references public.working_hours (id)
        on delete restrict,

    constraint chk_resource_availability_name
        check (char_length(trim(name)) > 0),

    constraint chk_resource_availability_status
        check (status in ('Active', 'Inactive')),

    constraint chk_resource_availability_period
        check (effective_to is null or effective_to >= effective_from)

);

comment on table public.resource_availability is
'Planned individual Resource Availability for Capacity Planning and Assignments';

-- ============================================================
-- TABLE: resource_availability_week_day
-- ============================================================

create table public.resource_availability_week_day (

    id uuid not null default gen_random_uuid(),

    resource_availability_id uuid not null,

    day_of_week varchar(20) not null,

    enabled boolean not null default false,

    constraint pk_resource_availability_week_day
        primary key (id),

    constraint fk_resource_availability_week_day_parent
        foreign key (resource_availability_id)
        references public.resource_availability (id)
        on delete cascade,

    constraint uq_resource_availability_week_day
        unique (resource_availability_id, day_of_week),

    constraint chk_resource_availability_week_day_of_week
        check (day_of_week in (
            'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'
        ))

);

-- ============================================================
-- TABLE: resource_availability_day_override
-- ============================================================

create table public.resource_availability_day_override (

    id uuid not null default gen_random_uuid(),

    resource_availability_id uuid not null,

    override_date date not null,

    available boolean not null,

    start_time time,

    end_time time,

    notes text,

    constraint pk_resource_availability_day_override
        primary key (id),

    constraint fk_resource_availability_day_override_parent
        foreign key (resource_availability_id)
        references public.resource_availability (id)
        on delete cascade,

    constraint uq_resource_availability_day_override
        unique (resource_availability_id, override_date),

    constraint chk_resource_availability_override_times
        check (
            (start_time is null and end_time is null)
            or (start_time is not null and end_time is not null and start_time < end_time)
        )

);

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_resource_availability_resource_id
    on public.resource_availability(execution_resource_id);

create index idx_resource_availability_calendar_id
    on public.resource_availability(working_calendar_id);

create index idx_resource_availability_hours_id
    on public.resource_availability(working_hours_id);

create index idx_resource_availability_status
    on public.resource_availability(status);

create index idx_resource_availability_resource_status
    on public.resource_availability(execution_resource_id, status);

create index idx_resource_availability_period
    on public.resource_availability(execution_resource_id, effective_from, effective_to);

create index idx_resource_availability_name
    on public.resource_availability(name);

create index idx_resource_availability_week_day_parent
    on public.resource_availability_week_day(resource_availability_id);

create index idx_resource_availability_override_parent
    on public.resource_availability_day_override(resource_availability_id);

create index idx_resource_availability_override_date
    on public.resource_availability_day_override(override_date);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
