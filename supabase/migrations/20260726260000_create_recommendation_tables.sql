-- ============================================================
-- AgencyOS
-- Migration: 20260726260000_create_recommendation_tables.sql
-- Release 1.1 / US-202
-- Domain: Recommendation Persistence
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: recommendation
-- ============================================================

create table public.recommendation (

    id uuid not null default gen_random_uuid(),

    company_id uuid not null,

    mission_id uuid not null,

    contract_id uuid not null,

    delivery_strategy_id uuid not null,

    recommendation_number varchar(100) not null,

    title varchar(300) not null,

    summary varchar(2000),

    reason varchar(4000),

    score numeric(12, 4),

    rank integer,

    status varchar(30) not null,

    version integer not null,

    decision_engine_version varchar(50) not null,

    planning_template_id uuid,

    capacity_snapshot jsonb not null,

    workload_snapshot jsonb not null,

    recommendation_payload jsonb not null,

    generated_at timestamptz not null,

    generated_by varchar(200) not null,

    archived boolean not null default false,

    archived_at timestamptz,

    created_at timestamptz not null default now(),

    constraint pk_recommendation
        primary key (id),

    constraint uq_recommendation_number_version
        unique (recommendation_number, version),

    constraint fk_recommendation_mission
        foreign key (mission_id)
        references public.mission (id)
        on delete restrict,

    constraint fk_recommendation_contract
        foreign key (contract_id)
        references public.client_contract (id)
        on delete restrict,

    constraint chk_recommendation_status
        check (status in ('Active', 'Archived')),

    constraint chk_recommendation_version
        check (version >= 1),

    constraint chk_recommendation_number
        check (char_length(trim(recommendation_number)) > 0),

    constraint chk_recommendation_title
        check (char_length(trim(title)) > 0),

    constraint chk_recommendation_decision_engine_version
        check (char_length(trim(decision_engine_version)) > 0),

    constraint chk_recommendation_generated_by
        check (char_length(trim(generated_by)) > 0),

    constraint chk_recommendation_archived_consistency
        check (
            (archived = false and archived_at is null and status = 'Active')
            or (archived = true and archived_at is not null and status = 'Archived')
        )

);

comment on table public.recommendation is
'Immutable persisted Decision Engine recommendations (US-202). No delete. Archive/restore only.';

comment on column public.recommendation.recommendation_payload is
'Full operational payload JSON preserved at generation time (BR-1105)';

comment on column public.recommendation.status is
'Active/Archived only. Approval status belongs to recommendation_workflow (BR-1104)';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_recommendation_company_id
    on public.recommendation(company_id);

create index idx_recommendation_mission_id
    on public.recommendation(mission_id);

create index idx_recommendation_contract_id
    on public.recommendation(contract_id);

create index idx_recommendation_status
    on public.recommendation(status);

create index idx_recommendation_number
    on public.recommendation(recommendation_number);

create index idx_recommendation_version
    on public.recommendation(version);

create index idx_recommendation_generated_at
    on public.recommendation(generated_at desc);

create index idx_recommendation_delivery_strategy_id
    on public.recommendation(delivery_strategy_id);

create index idx_recommendation_archived
    on public.recommendation(archived);

create index idx_recommendation_company_generated
    on public.recommendation(company_id, generated_at desc);

create index idx_recommendation_mission_version
    on public.recommendation(mission_id, recommendation_number, version);


-- ============================================================
-- ALTER: recommendation_workflow — consume persisted Recommendation
-- ============================================================

alter table public.recommendation_workflow
    add column if not exists recommendation_id uuid;

-- Nullable for pre-US-202 rows; application Create requires RecommendationId (BR-1102).
alter table public.recommendation_workflow
    drop constraint if exists fk_recommendation_workflow_recommendation;

alter table public.recommendation_workflow
    add constraint fk_recommendation_workflow_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict;

create index if not exists idx_recommendation_workflow_recommendation_id
    on public.recommendation_workflow(recommendation_id);

comment on column public.recommendation_workflow.recommendation_id is
'FK to persisted Recommendation (US-202). Workflow consumes durable recommendations (BR-1102).';


-- ============================================================
-- END OF MIGRATION
-- ============================================================
