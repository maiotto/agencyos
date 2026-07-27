-- ============================================================
-- AgencyOS
-- Migration: 20260726320000_create_recommendation_explainability_tables.sql
-- Release 1.1 / US-302
-- Domain: LLM Explainability
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: recommendation_explainability
-- ============================================================

create table public.recommendation_explainability (

    id uuid not null default gen_random_uuid(),

    recommendation_id uuid not null,

    ai_recommendation_id uuid null,

    explanation_type varchar(40) not null,

    generation_version integer not null,

    executive_summary varchar(2000) not null,

    detailed_explanation text not null,

    decision_factors jsonb not null,

    assumptions jsonb not null,

    risks jsonb not null,

    confidence_explanation text not null,

    capacity_explanation text not null,

    workload_explanation text not null,

    generated_at timestamptz not null,

    generated_by varchar(200) not null,

    model_version varchar(50) not null,

    prompt_version varchar(50) not null,

    status varchar(30) not null,

    archived_at timestamptz null,

    constraint pk_recommendation_explainability
        primary key (id),

    constraint fk_recommendation_explainability_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict,

    constraint fk_recommendation_explainability_ai_recommendation
        foreign key (ai_recommendation_id)
        references public.ai_recommendation (id)
        on delete restrict,

    constraint uq_recommendation_explainability_recommendation_generation
        unique (recommendation_id, generation_version),

    constraint chk_recommendation_explainability_generation_version
        check (generation_version >= 1),

    constraint chk_recommendation_explainability_type
        check (explanation_type in ('Recommendation', 'AIRecommendation')),

    constraint chk_recommendation_explainability_ai_id
        check (
            (explanation_type = 'Recommendation' and ai_recommendation_id is null)
            or (explanation_type = 'AIRecommendation' and ai_recommendation_id is not null)
        ),

    constraint chk_recommendation_explainability_generated_by
        check (char_length(trim(generated_by)) > 0),

    constraint chk_recommendation_explainability_executive_summary
        check (char_length(trim(executive_summary)) > 0),

    constraint chk_recommendation_explainability_detailed_explanation
        check (char_length(trim(detailed_explanation)) > 0),

    constraint chk_recommendation_explainability_confidence
        check (char_length(trim(confidence_explanation)) > 0),

    constraint chk_recommendation_explainability_capacity
        check (char_length(trim(capacity_explanation)) > 0),

    constraint chk_recommendation_explainability_workload
        check (char_length(trim(workload_explanation)) > 0),

    constraint chk_recommendation_explainability_model_version
        check (char_length(trim(model_version)) > 0),

    constraint chk_recommendation_explainability_prompt_version
        check (char_length(trim(prompt_version)) > 0),

    constraint chk_recommendation_explainability_status
        check (status in ('Active', 'Archived'))

);

comment on table public.recommendation_explainability is
'Immutable LLM Explainability records (US-302 / BR-1701..BR-1710). Informational only; never changes Recommendations.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_recommendation_explainability_recommendation_id
    on public.recommendation_explainability(recommendation_id);

create index idx_recommendation_explainability_ai_recommendation_id
    on public.recommendation_explainability(ai_recommendation_id);

create index idx_recommendation_explainability_type
    on public.recommendation_explainability(explanation_type);

create index idx_recommendation_explainability_status
    on public.recommendation_explainability(status);

create index idx_recommendation_explainability_generated_at
    on public.recommendation_explainability(generated_at desc);

create index idx_recommendation_explainability_model_version
    on public.recommendation_explainability(model_version);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
