-- ============================================================
-- AgencyOS
-- Migration: 20260726350000_create_company_tables.sql
-- Release 1.1 / US-402
-- Domain: Multi-Company Configuration
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: company
-- ============================================================

create table public.company (

    id uuid not null,

    company_code varchar(50) not null,

    company_name varchar(200) not null,

    legal_name varchar(300) null,

    status varchar(30) not null,

    timezone varchar(100) not null,

    country varchar(100) null,

    language varchar(20) null,

    currency varchar(10) null,

    planning_configuration jsonb not null default '{}'::jsonb,

    decision_profile_id uuid null,

    default_calendar_id uuid null,

    default_planning_template_id uuid null,

    created_at timestamptz not null,

    updated_at timestamptz not null,

    archived_at timestamptz null,

    constraint pk_company primary key (id),

    constraint uq_company_code unique (company_code),

    constraint uq_company_name unique (company_name),

    constraint chk_company_code check (char_length(trim(company_code)) > 0),

    constraint chk_company_name check (char_length(trim(company_name)) > 0),

    constraint chk_company_timezone check (char_length(trim(timezone)) > 0),

    constraint chk_company_status check (status in ('Active', 'Inactive', 'Archived'))
);

comment on table public.company is
'First-class Company tenant (US-402 / BR-2001..BR-2010). Configuration changes never rewrite historical data.';

create index idx_company_status on public.company(status);
create index idx_company_decision_profile_id on public.company(decision_profile_id);

-- Seed default company used by Release 1.1 fixtures
insert into public.company (
    id, company_code, company_name, legal_name, status, timezone, country, language, currency,
    planning_configuration, decision_profile_id, default_calendar_id, default_planning_template_id,
    created_at, updated_at, archived_at
) values (
    'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
    'AGENCYOS-DEFAULT',
    'AgencyOS Default Company',
    'AgencyOS Default Company Ltd',
    'Active',
    'UTC',
    'BR',
    'pt-BR',
    'BRL',
    '{"defaultPlanningWindowDays":30}'::jsonb,
    '11111111-1111-1111-1111-111111111106',
    null,
    null,
    timestamptz '2026-07-26 00:00:00+00',
    timestamptz '2026-07-26 00:00:00+00',
    null
);

-- Optional FKs from Company to defaults (profiles already seeded in prior migration)
alter table public.company
    add constraint fk_company_decision_profile
        foreign key (decision_profile_id)
        references public.company_decision_profile (id)
        on delete set null;

alter table public.company
    add constraint fk_company_default_calendar
        foreign key (default_calendar_id)
        references public.working_calendar (id)
        on delete set null;

alter table public.company
    add constraint fk_company_default_planning_template
        foreign key (default_planning_template_id)
        references public.planning_template (id)
        on delete set null;

-- ============================================================
-- Add CompanyId to AI Decision Support artifacts (BR-2004 / BR-2005)
-- ============================================================

alter table public.ai_recommendation
    add column if not exists company_id uuid null;

alter table public.recommendation_explainability
    add column if not exists company_id uuid null;

alter table public.executive_recommendation_summary
    add column if not exists company_id uuid null;

alter table public.recommendation_workflow
    add column if not exists company_id uuid null;

update public.ai_recommendation ar
set company_id = r.company_id
from public.recommendation r
where ar.recommendation_id = r.id
  and ar.company_id is null;

update public.recommendation_explainability re
set company_id = r.company_id
from public.recommendation r
where re.recommendation_id = r.id
  and re.company_id is null;

update public.executive_recommendation_summary es
set company_id = r.company_id
from public.recommendation r
where es.recommendation_id = r.id
  and es.company_id is null;

update public.recommendation_workflow rw
set company_id = r.company_id
from public.recommendation r
where rw.recommendation_id = r.id
  and rw.company_id is null;

create index if not exists idx_ai_recommendation_company_id
    on public.ai_recommendation(company_id);

create index if not exists idx_explainability_company_id
    on public.recommendation_explainability(company_id);

create index if not exists idx_executive_summary_company_id
    on public.executive_recommendation_summary(company_id);

create index if not exists idx_recommendation_workflow_company_id
    on public.recommendation_workflow(company_id);

-- Soft FKs from existing company_id columns to company (restrict delete)
-- Applied only where company_id is required / populated.

do $$
begin
    if not exists (
        select 1 from pg_constraint where conname = 'fk_working_calendar_company'
    ) then
        alter table public.working_calendar
            add constraint fk_working_calendar_company
                foreign key (company_id) references public.company (id) on delete restrict;
    end if;

    if not exists (
        select 1 from pg_constraint where conname = 'fk_planning_template_company'
    ) then
        alter table public.planning_template
            add constraint fk_planning_template_company
                foreign key (company_id) references public.company (id) on delete restrict;
    end if;

    if not exists (
        select 1 from pg_constraint where conname = 'fk_portfolio_company'
    ) then
        alter table public.portfolio
            add constraint fk_portfolio_company
                foreign key (company_id) references public.company (id) on delete restrict;
    end if;

    if not exists (
        select 1 from pg_constraint where conname = 'fk_recommendation_company'
    ) then
        alter table public.recommendation
            add constraint fk_recommendation_company
                foreign key (company_id) references public.company (id) on delete restrict;
    end if;

    if not exists (
        select 1 from pg_constraint where conname = 'fk_company_decision_profile_company'
    ) then
        alter table public.company_decision_profile
            add constraint fk_company_decision_profile_company
                foreign key (company_id) references public.company (id) on delete restrict;
    end if;
end $$;


-- ============================================================
-- END OF MIGRATION
-- ============================================================
