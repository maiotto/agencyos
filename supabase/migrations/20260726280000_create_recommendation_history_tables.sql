-- ============================================================
-- AgencyOS
-- Migration: 20260726280000_create_recommendation_history_tables.sql
-- Release 1.1 / US-203
-- Domain: Recommendation History
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: recommendation_history
-- ============================================================

create table public.recommendation_history (

    id uuid not null default gen_random_uuid(),

    recommendation_id uuid not null,

    recommendation_number varchar(100) not null,

    recommendation_version integer not null,

    company_id uuid not null,

    mission_id uuid not null,

    contract_id uuid not null,

    delivery_strategy_id uuid not null,

    title varchar(300) not null,

    summary varchar(2000),

    event_type varchar(40) not null,

    recommendation_status varchar(30) not null,

    workflow_status varchar(30),

    workflow_id uuid,

    approver varchar(200),

    approval_date timestamptz,

    approval_comment varchar(2000),

    score numeric(12, 4),

    rank integer,

    planning_template_id uuid,

    decision_engine_version varchar(50) not null,

    capacity_snapshot jsonb not null,

    workload_snapshot jsonb not null,

    recommendation_payload jsonb not null,

    created_by varchar(200) not null,

    created_at timestamptz not null default now(),

    constraint pk_recommendation_history
        primary key (id),

    constraint fk_recommendation_history_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict,

    constraint chk_recommendation_history_event_type
        check (event_type in ('VersionCreated', 'Archived', 'Restored', 'WorkflowTransition')),

    constraint chk_recommendation_history_recommendation_status
        check (recommendation_status in ('Active', 'Archived')),

    constraint chk_recommendation_history_version
        check (recommendation_version >= 1),

    constraint chk_recommendation_history_number
        check (char_length(trim(recommendation_number)) > 0),

    constraint chk_recommendation_history_title
        check (char_length(trim(title)) > 0),

    constraint chk_recommendation_history_created_by
        check (char_length(trim(created_by)) > 0),

    constraint chk_recommendation_history_decision_engine_version
        check (char_length(trim(decision_engine_version)) > 0)

);

comment on table public.recommendation_history is
'Immutable Recommendation History snapshots and workflow timeline events (US-203). No update/delete.';

comment on column public.recommendation_history.recommendation_payload is
'Frozen recommendation payload at event time (BR-1204)';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_recommendation_history_recommendation_id
    on public.recommendation_history(recommendation_id);

create index idx_recommendation_history_recommendation_number
    on public.recommendation_history(recommendation_number);

create index idx_recommendation_history_company_id
    on public.recommendation_history(company_id);

create index idx_recommendation_history_mission_id
    on public.recommendation_history(mission_id);

create index idx_recommendation_history_contract_id
    on public.recommendation_history(contract_id);

create index idx_recommendation_history_version
    on public.recommendation_history(recommendation_version);

create index idx_recommendation_history_workflow_status
    on public.recommendation_history(workflow_status);

create index idx_recommendation_history_created_at
    on public.recommendation_history(created_at desc);

create index idx_recommendation_history_event_type
    on public.recommendation_history(event_type);

create index idx_recommendation_history_company_created
    on public.recommendation_history(company_id, created_at desc);

create index idx_recommendation_history_number_version
    on public.recommendation_history(recommendation_number, recommendation_version);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
