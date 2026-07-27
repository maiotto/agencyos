-- ============================================================
-- AgencyOS
-- Migration: 20260726250000_create_recommendation_workflow_tables.sql
-- Release 1.1 / US-201
-- Domain: Recommendation Approval Workflow
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: recommendation_workflow
-- ============================================================

create table public.recommendation_workflow (

    id uuid not null default gen_random_uuid(),

    delivery_strategy_id uuid not null,

    contract_id uuid not null,

    mission_id uuid not null,

    company_decision_profile_id uuid null,

    title varchar(300) not null,

    summary varchar(2000) null,

    status varchar(30) not null,

    created_by varchar(200) not null,

    approver varchar(200) null,

    approval_date timestamptz null,

    approval_comment varchar(2000) null,

    rank_position integer null,

    final_score numeric(12,4) null,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_recommendation_workflow
        primary key (id),

    constraint chk_recommendation_workflow_title
        check (char_length(trim(title)) > 0),

    constraint chk_recommendation_workflow_created_by
        check (char_length(trim(created_by)) > 0),

    constraint chk_recommendation_workflow_status
        check (lower(status) in (
            'draft',
            'pendingapproval',
            'approved',
            'rejected',
            'cancelled',
            'reopened'
        ))

);

comment on table public.recommendation_workflow is
'Decision Engine recommendation approval workflow governance (US-201)';

-- ============================================================
-- TABLE: recommendation_workflow_transition
-- ============================================================

create table public.recommendation_workflow_transition (

    id uuid not null default gen_random_uuid(),

    recommendation_workflow_id uuid not null,

    from_status varchar(30) not null,

    to_status varchar(30) not null,

    actor varchar(200) not null,

    comment varchar(2000) null,

    occurred_at timestamptz not null,

    constraint pk_recommendation_workflow_transition
        primary key (id),

    constraint fk_recommendation_workflow_transition_workflow
        foreign key (recommendation_workflow_id)
        references public.recommendation_workflow (id)
        on delete cascade,

    constraint chk_recommendation_workflow_transition_actor
        check (char_length(trim(actor)) > 0)

);

comment on table public.recommendation_workflow_transition is
'Immutable append-only recommendation workflow transition history (BR-1010)';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_recommendation_workflow_status
    on public.recommendation_workflow(status);

create index idx_recommendation_workflow_delivery_strategy_id
    on public.recommendation_workflow(delivery_strategy_id);

create index idx_recommendation_workflow_contract_id
    on public.recommendation_workflow(contract_id);

create index idx_recommendation_workflow_mission_id
    on public.recommendation_workflow(mission_id);

create index idx_recommendation_workflow_created_at
    on public.recommendation_workflow(created_at desc);

create index idx_recommendation_workflow_transition_workflow_id
    on public.recommendation_workflow_transition(recommendation_workflow_id);

create index idx_recommendation_workflow_transition_occurred_at
    on public.recommendation_workflow_transition(occurred_at);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
