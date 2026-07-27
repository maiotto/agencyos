-- ============================================================
-- AgencyOS
-- Migration: 20260726300000_create_audit_event_tables.sql
-- Release 1.1 / US-206
-- Domain: Decision Audit Trail
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: audit_event
-- ============================================================

create table public.audit_event (

    id uuid not null default gen_random_uuid(),

    entity_type varchar(100) not null,

    entity_id uuid not null,

    entity_version varchar(50) null,

    event_type varchar(80) not null,

    action varchar(120) not null,

    company_id uuid null,

    user_id varchar(200) not null,

    user_name varchar(200) not null,

    occurred_at timestamptz not null default now(),

    source varchar(80) not null,

    correlation_id uuid null,

    session_id varchar(100) null,

    request_id varchar(100) null,

    previous_state jsonb null,

    current_state jsonb null,

    metadata jsonb null,

    constraint pk_audit_event
        primary key (id),

    constraint chk_audit_event_entity_type
        check (char_length(trim(entity_type)) > 0),

    constraint chk_audit_event_event_type
        check (char_length(trim(event_type)) > 0),

    constraint chk_audit_event_action
        check (char_length(trim(action)) > 0),

    constraint chk_audit_event_user_id
        check (char_length(trim(user_id)) > 0),

    constraint chk_audit_event_user_name
        check (char_length(trim(user_name)) > 0),

    constraint chk_audit_event_source
        check (char_length(trim(source)) > 0)

);

comment on table public.audit_event is
'Immutable append-only Decision Audit Trail (US-206 / BR-1501..BR-1510). No update/delete.';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_audit_event_entity
    on public.audit_event(entity_type, entity_id);

create index idx_audit_event_entity_id
    on public.audit_event(entity_id);

create index idx_audit_event_company_id
    on public.audit_event(company_id);

create index idx_audit_event_user_id
    on public.audit_event(user_id);

create index idx_audit_event_correlation_id
    on public.audit_event(correlation_id);

create index idx_audit_event_event_type
    on public.audit_event(event_type);

create index idx_audit_event_occurred_at
    on public.audit_event(occurred_at desc);

create index idx_audit_event_company_occurred
    on public.audit_event(company_id, occurred_at desc);

create index idx_audit_event_entity_occurred
    on public.audit_event(entity_id, occurred_at desc);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
