-- ============================================================
-- AgencyOS
-- Migration: 20260726330000_create_executive_recommendation_summary_tables.sql
-- Release 1.1 / US-303
-- Domain: Executive Recommendation Summary
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: executive_recommendation_summary
-- ============================================================

create table public.executive_recommendation_summary (

    id uuid not null default gen_random_uuid(),

    recommendation_id uuid not null,

    ai_recommendation_id uuid null,

    explainability_id uuid null,

    summary_version integer not null,

    executive_summary varchar(4000) not null,

    key_decision_factors jsonb not null,

    business_impact text not null,

    capacity_impact text not null,

    workload_impact text not null,

    risks jsonb not null,

    assumptions jsonb not null,

    confidence_level numeric(5, 2) not null,

    recommended_actions jsonb not null,

    generated_at timestamptz not null,

    generated_by varchar(200) not null,

    model_version varchar(50) not null,

    prompt_version varchar(50) not null,

    status varchar(30) not null,

    archived_at timestamptz null,

    constraint pk_executive_recommendation_summary
        primary key (id),

    constraint fk_executive_summary_recommendation
        foreign key (recommendation_id)
        references public.recommendation (id)
        on delete restrict,

    constraint fk_executive_summary_ai_recommendation
        foreign key (ai_recommendation_id)
        references public.ai_recommendation (id)
        on delete restrict,

    constraint fk_executive_summary_explainability
        foreign key (explainability_id)
        references public.recommendation_explainability (id)
        on delete restrict,

    constraint uq_executive_summary_recommendation_version
        unique (recommendation_id, summary_version),

    constraint chk_executive_summary_version
        check (summary_version >= 1),

    constraint chk_executive_summary_confidence
        check (confidence_level >= 0 and confidence_level <= 100),

    constraint chk_executive_summary_generated_by
        check (char_length(trim(generated_by)) > 0),

    constraint chk_executive_summary_text
        check (char_length(trim(executive_summary)) > 0),

    constraint chk_executive_summary_business_impact
        check (char_length(trim(business_impact)) > 0),

    constraint chk_executive_summary_capacity_impact
        check (char_length(trim(capacity_impact)) > 0),

    constraint chk_executive_summary_workload_impact
        check (char_length(trim(workload_impact)) > 0),

    constraint chk_executive_summary_model_version
        check (char_length(trim(model_version)) > 0),

    constraint chk_executive_summary_prompt_version
        check (char_length(trim(prompt_version)) > 0),

    constraint chk_executive_summary_status
        check (status in ('Active', 'Archived'))

);

comment on table public.executive_recommendation_summary is
'Immutable Executive Recommendation Summaries (US-303 / BR-1801..BR-1810). Informational only; never changes Recommendations.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_executive_summary_recommendation_id
    on public.executive_recommendation_summary(recommendation_id);

create index idx_executive_summary_ai_recommendation_id
    on public.executive_recommendation_summary(ai_recommendation_id);

create index idx_executive_summary_explainability_id
    on public.executive_recommendation_summary(explainability_id);

create index idx_executive_summary_status
    on public.executive_recommendation_summary(status);

create index idx_executive_summary_generated_at
    on public.executive_recommendation_summary(generated_at desc);

create index idx_executive_summary_confidence_level
    on public.executive_recommendation_summary(confidence_level desc);

create index idx_executive_summary_model_version
    on public.executive_recommendation_summary(model_version);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
