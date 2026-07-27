-- ============================================================
-- AgencyOS
-- Migration: 20260726340000_create_company_decision_profile_tables.sql
-- Release 1.1 / US-401
-- Domain: Company Decision Profiles
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: company_decision_profile
-- ============================================================

create table public.company_decision_profile (

    id uuid not null default gen_random_uuid(),

    company_id uuid not null,

    profile_family_id uuid not null,

    code varchar(100) not null,

    name varchar(200) not null,

    description varchar(2000) null,

    status varchar(30) not null,

    priority_weights jsonb not null,

    capacity_weight numeric(8, 4) not null,

    workload_weight numeric(8, 4) not null,

    cost_weight numeric(8, 4) not null,

    risk_weight numeric(8, 4) not null,

    quality_weight numeric(8, 4) not null,

    preferred_strategy varchar(200) null,

    preferred_capacity_threshold numeric(5, 2) null,

    preferred_workload_threshold numeric(5, 2) null,

    default_profile boolean not null default false,

    version integer not null,

    created_at timestamptz not null,

    updated_at timestamptz not null,

    archived_at timestamptz null,

    constraint pk_company_decision_profile
        primary key (id),

    constraint uq_company_decision_profile_family_version
        unique (company_id, profile_family_id, version),

    constraint chk_company_decision_profile_version
        check (version >= 1),

    constraint chk_company_decision_profile_code
        check (char_length(trim(code)) > 0),

    constraint chk_company_decision_profile_name
        check (char_length(trim(name)) > 0),

    constraint chk_company_decision_profile_status
        check (status in ('Active', 'Inactive', 'Archived')),

    constraint chk_company_decision_profile_capacity_weight
        check (capacity_weight >= 0 and capacity_weight <= 1),

    constraint chk_company_decision_profile_workload_weight
        check (workload_weight >= 0 and workload_weight <= 1),

    constraint chk_company_decision_profile_cost_weight
        check (cost_weight >= 0 and cost_weight <= 1),

    constraint chk_company_decision_profile_risk_weight
        check (risk_weight >= 0 and risk_weight <= 1),

    constraint chk_company_decision_profile_quality_weight
        check (quality_weight >= 0 and quality_weight <= 1),

    constraint chk_company_decision_profile_capacity_threshold
        check (preferred_capacity_threshold is null
            or (preferred_capacity_threshold >= 0 and preferred_capacity_threshold <= 100)),

    constraint chk_company_decision_profile_workload_threshold
        check (preferred_workload_threshold is null
            or (preferred_workload_threshold >= 0 and preferred_workload_threshold <= 100))
);

comment on table public.company_decision_profile is
'Company Decision Profiles (US-401 / BR-1901..BR-1910). Versioned preferences for ranking and AI Decision Support.';

create unique index uq_company_decision_profile_one_default
    on public.company_decision_profile(company_id)
    where default_profile = true and status = 'Active';

create index idx_company_decision_profile_company_id
    on public.company_decision_profile(company_id);

create index idx_company_decision_profile_status
    on public.company_decision_profile(status);

create index idx_company_decision_profile_family
    on public.company_decision_profile(profile_family_id);

create index idx_company_decision_profile_name
    on public.company_decision_profile(company_id, name);

-- ============================================================
-- SEED: default company built-in Decision Profiles
-- Company: aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa
-- Ids match the historical config-based profiles (11111111-1111-1111-1111-111111111101..106).
-- BalancedStrategy (...106) is the seeded DefaultProfile.
-- ============================================================

insert into public.company_decision_profile (
    id, company_id, profile_family_id, code, name, description, status,
    priority_weights, capacity_weight, workload_weight, cost_weight, risk_weight, quality_weight,
    preferred_strategy, preferred_capacity_threshold, preferred_workload_threshold,
    default_profile, version, created_at, updated_at, archived_at
)
values
    (
        '11111111-1111-1111-1111-111111111101', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111101', 'ProfitMaximization', 'Profit Maximization',
        'Prioritizes lowest cost delivery strategies.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.35,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.10,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.10,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.20,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"AiResourceUsage","weight":0.05,"preferHigherValues":true},
            {"dimension":"ExternalResourceUsage","weight":0.10,"preferHigherValues":false},
            {"dimension":"AutomationUsage","weight":0.05,"preferHigherValues":true}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, false, 1, now(), now(), null
    ),
    (
        '11111111-1111-1111-1111-111111111102', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111102', 'DeliverySpeed', 'Delivery Speed',
        'Prioritizes fastest delivery strategies.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.10,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.35,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.10,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.15,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.10,"preferHigherValues":true},
            {"dimension":"AiResourceUsage","weight":0.10,"preferHigherValues":true},
            {"dimension":"ExternalResourceUsage","weight":0.05,"preferHigherValues":true},
            {"dimension":"AutomationUsage","weight":0.05,"preferHigherValues":true}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, false, 1, now(), now(), null
    ),
    (
        '11111111-1111-1111-1111-111111111103', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111103', 'OperationalStability', 'Operational Stability',
        'Prioritizes lowest operational risk and stable capacity utilization.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.10,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.10,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.20,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.35,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.10,"preferHigherValues":true},
            {"dimension":"AiResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"ExternalResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"AutomationUsage","weight":0.05,"preferHigherValues":false}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, false, 1, now(), now(), null
    ),
    (
        '11111111-1111-1111-1111-111111111104', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111104', 'AiAdoption', 'AI Adoption',
        'Prioritizes strategies with higher AI resource usage and automation.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.10,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.10,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.10,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.10,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"AiResourceUsage","weight":0.35,"preferHigherValues":true},
            {"dimension":"ExternalResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"AutomationUsage","weight":0.15,"preferHigherValues":true}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, false, 1, now(), now(), null
    ),
    (
        '11111111-1111-1111-1111-111111111105', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111105', 'HumanResourceOptimization', 'Human Resource Optimization',
        'Prioritizes strategies with higher human resource usage.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.10,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.10,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.15,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.15,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.35,"preferHigherValues":true},
            {"dimension":"AiResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"ExternalResourceUsage","weight":0.05,"preferHigherValues":false},
            {"dimension":"AutomationUsage","weight":0.05,"preferHigherValues":false}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, false, 1, now(), now(), null
    ),
    (
        '11111111-1111-1111-1111-111111111106', 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        '11111111-1111-1111-1111-111111111106', 'BalancedStrategy', 'Balanced Strategy',
        'Balanced weighting across all ranking dimensions. Default company profile.', 'Active',
        '[
            {"dimension":"EstimatedCost","weight":0.125,"preferHigherValues":false},
            {"dimension":"EstimatedDuration","weight":0.125,"preferHigherValues":false},
            {"dimension":"CapacityUtilization","weight":0.125,"preferHigherValues":false},
            {"dimension":"OperationalRisk","weight":0.125,"preferHigherValues":false},
            {"dimension":"HumanResourceUsage","weight":0.125,"preferHigherValues":true},
            {"dimension":"AiResourceUsage","weight":0.125,"preferHigherValues":true},
            {"dimension":"ExternalResourceUsage","weight":0.125,"preferHigherValues":true},
            {"dimension":"AutomationUsage","weight":0.125,"preferHigherValues":true}
        ]'::jsonb,
        0.2, 0.2, 0.2, 0.2, 0.2, null, null, null, true, 1, now(), now(), null
    )
on conflict (id) do nothing;

-- ============================================================
-- Profile version columns on generated artifacts (BR-1906..BR-1909)
-- ============================================================

alter table public.recommendation
    add column if not exists decision_profile_id uuid null,
    add column if not exists decision_profile_version integer not null default 0;

alter table public.recommendation_history
    add column if not exists decision_profile_id uuid null,
    add column if not exists decision_profile_version integer not null default 0;

alter table public.ai_recommendation
    add column if not exists decision_profile_id uuid null,
    add column if not exists decision_profile_version integer not null default 0;

alter table public.recommendation_explainability
    add column if not exists decision_profile_id uuid null,
    add column if not exists decision_profile_version integer not null default 0;

alter table public.executive_recommendation_summary
    add column if not exists decision_profile_id uuid null,
    add column if not exists decision_profile_version integer not null default 0;

create index if not exists idx_recommendation_decision_profile_id
    on public.recommendation(decision_profile_id);

create index if not exists idx_ai_recommendation_decision_profile_id
    on public.ai_recommendation(decision_profile_id);

create index if not exists idx_explainability_decision_profile_id
    on public.recommendation_explainability(decision_profile_id);

create index if not exists idx_executive_summary_decision_profile_id
    on public.executive_recommendation_summary(decision_profile_id);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
