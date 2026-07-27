-- ============================================================
-- AgencyOS
-- Migration: 20260726220000_create_capacity_history_tables.sql
-- Release 1.1 / US-106
-- Domain: Capacity History
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: capacity_history
-- ============================================================

create table public.capacity_history (

    id uuid not null default gen_random_uuid(),

    execution_resource_id uuid not null,

    company_id uuid not null,

    calculation_date timestamptz not null,

    period_start date not null,

    period_end date not null,

    working_days integer not null,

    holiday_days integer not null,

    available_days integer not null,

    configured_hours numeric(12,2) not null,

    available_hours numeric(12,2) not null,

    capacity_hours numeric(12,2) not null,

    allocated_hours numeric(12,2) not null,

    utilization_percentage numeric(8,2) not null,

    calculation_version varchar(50) not null,

    operational_inputs_json jsonb not null,

    created_at timestamptz not null default now(),

    constraint pk_capacity_history
        primary key (id),

    constraint fk_capacity_history_execution_resource
        foreign key (execution_resource_id)
        references public.execution_resource (id)
        on delete restrict,

    constraint chk_capacity_history_period
        check (period_end >= period_start),

    constraint chk_capacity_history_day_counts
        check (
            working_days >= 0
            and holiday_days >= 0
            and available_days >= 0
        ),

    constraint chk_capacity_history_version
        check (char_length(trim(calculation_version)) > 0)

);

comment on table public.capacity_history is
'Immutable historical snapshots of completed Capacity calculations (US-106)';

comment on column public.capacity_history.operational_inputs_json is
'Preserved operational inputs and day breakdown for reproducibility (BR-609)';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_capacity_history_execution_resource_id
    on public.capacity_history(execution_resource_id);

create index idx_capacity_history_company_id
    on public.capacity_history(company_id);

create index idx_capacity_history_period
    on public.capacity_history(period_start, period_end);

create index idx_capacity_history_calculation_date
    on public.capacity_history(calculation_date desc);

create index idx_capacity_history_calculation_version
    on public.capacity_history(calculation_version);

create index idx_capacity_history_resource_period
    on public.capacity_history(execution_resource_id, period_start, period_end);

create index idx_capacity_history_company_period
    on public.capacity_history(company_id, period_start, period_end);

create index idx_capacity_history_resource_version
    on public.capacity_history(execution_resource_id, calculation_version);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
