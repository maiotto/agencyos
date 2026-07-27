-- ============================================================
-- AgencyOS
-- Migration: 20260726310000_create_ai_recommendation_tables.sql
-- Release 1.1 / US-301
-- Domain: AI-assisted Recommendation
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: ai_recommendation
-- ============================================================

create table public.ai_recommendation (

    id uuid not null default gen_random_uuid(),

    recommendation_id uuid not null,

    recommendation_version integer not null,

    generation_version integer not null,

    generated_at timestamptz not null,

    generated_by varchar(200) not null,

    confidence_score numeric(5, 2) not null,

    executive_summary varchar(2000) not null,

    reasoning text not null,

    assumptions jsonb not null,

    risks jsonb not null,

    alternatives jsonb not null,

    suggested_delivery_strategy varchar(300) not null,

    suggested_capacity_impact jsonb not null,

    suggested_workload_impact jsonb not null,

    model_version varchar(50) not null,

    prompt_version varchar(50) not null,

    status varchar(30) not null,

    archived_at timestamptz null,

    constraint pk_ai_recommendation
        primary key (id),

    constraint fk_ai_recommendation_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict,

    constraint uq_ai_recommendation_recommendation_generation
        unique (recommendation_id, generation_version),

    constraint chk_ai_recommendation_recommendation_version
        check (recommendation_version >= 1),

    constraint chk_ai_recommendation_generation_version
        check (generation_version >= 1),

    constraint chk_ai_recommendation_confidence
        check (confidence_score >= 0 and confidence_score <= 100),

    constraint chk_ai_recommendation_generated_by
        check (char_length(trim(generated_by)) > 0),

    constraint chk_ai_recommendation_executive_summary
        check (char_length(trim(executive_summary)) > 0),

    constraint chk_ai_recommendation_reasoning
        check (char_length(trim(reasoning)) > 0),

    constraint chk_ai_recommendation_suggested_strategy
        check (char_length(trim(suggested_delivery_strategy)) > 0),

    constraint chk_ai_recommendation_model_version
        check (char_length(trim(model_version)) > 0),

    constraint chk_ai_recommendation_prompt_version
        check (char_length(trim(prompt_version)) > 0),

    constraint chk_ai_recommendation_status
        check (status in ('Active', 'Archived'))

);

comment on table public.ai_recommendation is
'Immutable AI-assisted advisory Recommendations (US-301 / BR-1601..BR-1610). Never replaces Recommendations.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_ai_recommendation_recommendation_id
    on public.ai_recommendation(recommendation_id);

create index idx_ai_recommendation_status
    on public.ai_recommendation(status);

create index idx_ai_recommendation_generated_at
    on public.ai_recommendation(generated_at desc);

create index idx_ai_recommendation_confidence_score
    on public.ai_recommendation(confidence_score desc);

create index idx_ai_recommendation_model_version
    on public.ai_recommendation(model_version);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
