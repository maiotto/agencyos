-- ============================================================
-- AgencyOS
-- Migration: create_mission_tables.sql
-- Sprint 2
-- Domain: Mission
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: mission_type
-- ============================================================

create table public.mission_type (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    name varchar(100) not null,

    description text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_mission_type
        primary key (id),

    constraint uq_mission_type_code
        unique (code)

);

comment on table public.mission_type is
'Mission Types';


-- ============================================================
-- TABLE: mission_status
-- ============================================================

create table public.mission_status (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    name varchar(100) not null,

    description text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_mission_status
        primary key (id),

    constraint uq_mission_status_code
        unique (code)

);

comment on table public.mission_status is
'Mission Status';


-- ============================================================
-- TABLE: mission
-- ============================================================

create table public.mission (

    id uuid not null default gen_random_uuid(),

    client_contract_id uuid not null,

    code varchar(30) not null,

    name varchar(200) not null,

    description text,

    mission_type_id uuid not null,

    mission_status_id uuid not null,
	priority varchar(20) not null default 'NORMAL',

    start_date date,

    end_date date,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_mission
        primary key (id),

    constraint uq_mission_code
        unique (code),

    constraint fk_mission_contract
        foreign key (client_contract_id)
        references public.client_contract(id),

    constraint fk_mission_type
        foreign key (mission_type_id)
        references public.mission_type(id),

    constraint fk_mission_status
        foreign key (mission_status_id)
        references public.mission_status(id)

);

comment on table public.mission is
'Agency Mission';


-- ============================================================
-- INDEXES
-- ============================================================

create index idx_mission_name
    on public.mission(name);

create index idx_mission_contract
    on public.mission(client_contract_id);

create index idx_mission_type
    on public.mission(mission_type_id);

create index idx_mission_status
    on public.mission(mission_status_id);

create index idx_mission_start_date
    on public.mission(start_date);

create index idx_mission_end_date
    on public.mission(end_date);


-- ============================================================
-- END OF MIGRATION
-- ============================================================