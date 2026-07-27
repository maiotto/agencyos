-- ============================================================
-- AgencyOS
-- Migration: 20260726270000_create_portfolio_tables.sql
-- Release 1.1 / US-109
-- Domain: Portfolio Planning
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: portfolio
-- ============================================================

create table public.portfolio (

    id uuid not null default gen_random_uuid(),

    company_id uuid not null,

    name varchar(200) not null,

    description varchar(2000),

    status varchar(30) not null,

    planning_template_id uuid,

    planning_period_start date not null,

    planning_period_end date not null,

    capacity_summary jsonb,

    workload_summary jsonb,

    portfolio_health varchar(30) not null default 'Unknown',

    health_details jsonb,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_portfolio
        primary key (id),

    constraint uq_portfolio_company_name
        unique (company_id, name),

    constraint fk_portfolio_planning_template
        foreign key (planning_template_id)
        references public.planning_template (id)
        on delete set null,

    constraint chk_portfolio_status
        check (status in ('Active', 'Inactive')),

    constraint chk_portfolio_name
        check (char_length(trim(name)) > 0),

    constraint chk_portfolio_period
        check (planning_period_end >= planning_period_start),

    constraint chk_portfolio_health
        check (portfolio_health in ('Unknown', 'Underutilized', 'Healthy', 'AtRisk', 'Overloaded'))

);

comment on table public.portfolio is
'Portfolio Planning aggregate consolidating Missions for capacity/workload analysis (US-109)';

-- ============================================================
-- TABLE: portfolio_mission
-- ============================================================

create table public.portfolio_mission (

    id uuid not null default gen_random_uuid(),

    portfolio_id uuid not null,

    mission_id uuid not null,

    priority integer not null,

    included_at timestamptz not null,

    constraint pk_portfolio_mission
        primary key (id),

    constraint uq_portfolio_mission
        unique (portfolio_id, mission_id),

    constraint fk_portfolio_mission_portfolio
        foreign key (portfolio_id)
        references public.portfolio (id)
        on delete cascade,

    constraint fk_portfolio_mission_mission
        foreign key (mission_id)
        references public.mission (id)
        on delete restrict,

    constraint chk_portfolio_mission_priority
        check (priority >= 1)

);

comment on table public.portfolio_mission is
'Missions associated to a Portfolio with priority (US-109 / BR-903..BR-905)';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_portfolio_company_id
    on public.portfolio(company_id);

create index idx_portfolio_status
    on public.portfolio(status);

create index idx_portfolio_planning_template_id
    on public.portfolio(planning_template_id);

create index idx_portfolio_period
    on public.portfolio(planning_period_start, planning_period_end);

create index idx_portfolio_health
    on public.portfolio(portfolio_health);

create index idx_portfolio_mission_portfolio_id
    on public.portfolio_mission(portfolio_id);

create index idx_portfolio_mission_mission_id
    on public.portfolio_mission(mission_id);

create index idx_portfolio_mission_priority
    on public.portfolio_mission(portfolio_id, priority);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
