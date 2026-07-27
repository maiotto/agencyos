-- ============================================================
-- AgencyOS
-- Migration: 20260726290000_create_decision_tables.sql
-- Release 1.1 / US-205
-- Domain: Decision Tracking
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: decision
-- ============================================================

create table public.decision (

    id uuid not null default gen_random_uuid(),

    recommendation_id uuid not null,

    company_id uuid not null,

    mission_id uuid not null,

    contract_id uuid not null,

    decision_status varchar(30) not null,

    implementation_status varchar(30) not null,

    decision_date timestamptz not null,

    implementation_date timestamptz null,

    completed_date timestamptz null,

    outcome varchar(2000) null,

    business_value varchar(2000) null,

    created_by varchar(200) not null,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_decision
        primary key (id),

    constraint fk_decision_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict,

    constraint uq_decision_recommendation_id
        unique (recommendation_id),

    constraint chk_decision_created_by
        check (char_length(trim(created_by)) > 0),

    constraint chk_decision_status
        check (decision_status in ('Created', 'InProgress', 'Completed', 'Cancelled')),

    constraint chk_decision_implementation_status
        check (implementation_status in ('NotStarted', 'InProgress', 'Completed', 'Cancelled')),

    constraint chk_decision_outcome_after_complete
        check (
            outcome is null
            or decision_status = 'Completed'
        )

);

comment on table public.decision is
'Decision Tracking lifecycle for approved Recommendations (US-205 / BR-1401..BR-1407). Delete prohibited.';

comment on column public.decision.recommendation_id is
'Immutable originating Recommendation (BR-1402). One Decision per Recommendation (BR-1401).';

-- ============================================================
-- TABLE: decision_timeline
-- ============================================================

create table public.decision_timeline (

    id uuid not null default gen_random_uuid(),

    decision_id uuid not null,

    event_type varchar(40) not null,

    from_decision_status varchar(30) not null,

    to_decision_status varchar(30) not null,

    from_implementation_status varchar(30) not null,

    to_implementation_status varchar(30) not null,

    actor varchar(200) not null,

    comment varchar(2000) null,

    occurred_at timestamptz not null,

    constraint pk_decision_timeline
        primary key (id),

    constraint fk_decision_timeline_decision
        foreign key (decision_id)
        references public.decision (id)
        on delete restrict,

    constraint chk_decision_timeline_actor
        check (char_length(trim(actor)) > 0),

    constraint chk_decision_timeline_event_type
        check (event_type in (
            'Created',
            'StartImplementation',
            'Complete',
            'Cancel',
            'RecordOutcome'
        ))

);

comment on table public.decision_timeline is
'Append-only Decision timeline (BR-1404). No update/delete.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_decision_company_id
    on public.decision(company_id);

create index idx_decision_mission_id
    on public.decision(mission_id);

create index idx_decision_contract_id
    on public.decision(contract_id);

create index idx_decision_decision_status
    on public.decision(decision_status);

create index idx_decision_implementation_status
    on public.decision(implementation_status);

create index idx_decision_decision_date
    on public.decision(decision_date desc);

create index idx_decision_created_at
    on public.decision(created_at desc);

create index idx_decision_company_status
    on public.decision(company_id, decision_status);

create index idx_decision_timeline_decision_id
    on public.decision_timeline(decision_id);

create index idx_decision_timeline_occurred_at
    on public.decision_timeline(occurred_at);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
