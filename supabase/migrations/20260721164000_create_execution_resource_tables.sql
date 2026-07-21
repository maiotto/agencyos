-- ============================================================
-- AgencyOS
-- Migration: 20260721164000_create_execution_resource_tables.sql
-- Sprint 6
-- Domain: Execution Resource
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: execution_resource
-- ============================================================

create table public.execution_resource (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    name varchar(200) not null,

    resource_type varchar(50) not null,

    status varchar(30) not null,

    capacity_hours_per_week numeric(10,2) not null,

    cost_rate numeric(15,2),

    currency varchar(3),

    skills text[],

    availability text,

    notes text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_execution_resource
        primary key (id),

    constraint uq_execution_resource_code
        unique (code),

    constraint chk_execution_resource_capacity
        check (capacity_hours_per_week > 0)

);

comment on table public.execution_resource is
'Execution Resources capable of performing operational work';


-- ============================================================
-- INDEXES
-- ============================================================

create index idx_execution_resource_name
    on public.execution_resource(name);

create index idx_execution_resource_type
    on public.execution_resource(resource_type);

create index idx_execution_resource_status
    on public.execution_resource(status);

create index idx_execution_resource_skills
    on public.execution_resource using gin (skills);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
