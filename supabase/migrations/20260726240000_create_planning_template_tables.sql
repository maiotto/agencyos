-- ============================================================
-- AgencyOS
-- Migration: 20260726240000_create_planning_template_tables.sql
-- Release 1.1 / US-108
-- Domain: Planning Template
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: planning_template
-- ============================================================

create table public.planning_template (

    id uuid not null default gen_random_uuid(),

    company_id uuid not null,

    name varchar(200) not null,

    description varchar(1000) null,

    status varchar(20) not null,

    working_calendar_id uuid not null,

    working_hours_id uuid not null,

    resource_availability_strategy varchar(80) not null,

    default_planning_window_days integer not null,

    default_period_start_offset_days integer not null default 0,

    utilization_warning_percentage numeric(8,2) null,

    include_assignment_distribution boolean not null default true,

    planning_parameters_json jsonb null,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_planning_template
        primary key (id),

    constraint fk_planning_template_working_calendar
        foreign key (working_calendar_id)
        references public.working_calendar (id)
        on delete restrict,

    constraint fk_planning_template_working_hours
        foreign key (working_hours_id)
        references public.working_hours (id)
        on delete restrict,

    constraint chk_planning_template_status
        check (lower(status) in ('active', 'inactive')),

    constraint chk_planning_template_name
        check (char_length(trim(name)) > 0),

    constraint chk_planning_template_window
        check (default_planning_window_days > 0),

    constraint chk_planning_template_offset
        check (default_period_start_offset_days >= 0),

    constraint chk_planning_template_utilization
        check (
            utilization_warning_percentage is null
            or (utilization_warning_percentage >= 0 and utilization_warning_percentage <= 100)
        ),

    constraint uq_planning_template_company_name
        unique (company_id, name)

);

comment on table public.planning_template is
'Reusable planning configuration templates referencing calendars/hours/strategies (US-108). No operational data.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_planning_template_company_id
    on public.planning_template(company_id);

create index idx_planning_template_status
    on public.planning_template(status);

create index idx_planning_template_working_calendar_id
    on public.planning_template(working_calendar_id);

create index idx_planning_template_working_hours_id
    on public.planning_template(working_hours_id);

create index idx_planning_template_company_status
    on public.planning_template(company_id, status);

create index idx_planning_template_name
    on public.planning_template(name);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
