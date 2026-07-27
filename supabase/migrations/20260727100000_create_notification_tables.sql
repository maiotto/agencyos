-- ============================================================
-- AgencyOS
-- Migration: 20260727100000_create_notification_tables.sql
-- Release 1.1 / US-506
-- Domain: Notification Center
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: notification
-- ============================================================

create table public.notification (

    id uuid not null,

    company_id uuid not null,

    user_id varchar(200) not null,

    title varchar(200) not null,

    message varchar(2000) not null,

    category varchar(50) not null,

    priority varchar(30) not null,

    status varchar(30) not null,

    source_entity varchar(80) not null,

    source_entity_id uuid null,

    created_at timestamptz not null,

    read_at timestamptz null,

    archived boolean not null default false,

    constraint pk_notification primary key (id),

    constraint chk_notification_user_id check (char_length(trim(user_id)) > 0),

    constraint chk_notification_title check (char_length(trim(title)) > 0),

    constraint chk_notification_message check (char_length(trim(message)) > 0),

    constraint chk_notification_category check (
        category in (
            'Recommendation',
            'Decision',
            'Capacity',
            'Portfolio',
            'Audit',
            'Planning',
            'Executive',
            'Company',
            'System'
        )
    ),

    constraint chk_notification_priority check (
        priority in ('Low', 'Medium', 'High', 'Critical')
    ),

    constraint chk_notification_status check (
        status in ('Unread', 'Read')
    ),

    constraint chk_notification_source_entity check (
        source_entity in (
            'RecommendationWorkflow',
            'Decision',
            'CapacityHistory',
            'Portfolio',
            'AuditEvent',
            'PlanningWorkspace',
            'RecommendationWorkspace',
            'DecisionWorkspace',
            'ExecutiveWorkspace',
            'Company',
            'CompanyContext',
            'System'
        )
    ),

    constraint fk_notification_company
        foreign key (company_id) references public.company (id)
);

comment on table public.notification is
'In-application Notification Center (US-506 / BR-2901..BR-2910). Informational only; never modifies business data.';

create index idx_notification_company_user on public.notification(company_id, user_id);
create index idx_notification_company_user_status on public.notification(company_id, user_id, status);
create index idx_notification_company_user_category on public.notification(company_id, user_id, category);
create index idx_notification_company_user_priority on public.notification(company_id, user_id, priority);
create index idx_notification_company_user_archived on public.notification(company_id, user_id, archived);
create index idx_notification_created_at on public.notification(created_at desc);
create index idx_notification_source_entity on public.notification(source_entity, source_entity_id);
