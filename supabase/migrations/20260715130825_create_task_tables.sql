-- ============================================================
-- AgencyOS
-- Migration: create_task_tables.sql
-- Sprint 3
-- Domain: Task
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: task_type
-- ============================================================

create table public.task_type (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    name varchar(100) not null,

    description text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_task_type
        primary key (id),

    constraint uq_task_type_code
        unique (code)

);

comment on table public.task_type is
'Task Types';


-- ============================================================
-- TABLE: task_status
-- ============================================================

create table public.task_status (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    name varchar(100) not null,

    description text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_task_status
        primary key (id),

    constraint uq_task_status_code
        unique (code)

);

comment on table public.task_status is
'Task Status';


-- ============================================================
-- TABLE: task
-- ============================================================

create table public.task (

    id uuid not null default gen_random_uuid(),

    mission_id uuid not null,

    code varchar(30) not null,

    name varchar(200) not null,

    description text,

    task_type_id uuid not null,

    task_status_id uuid not null,

    priority varchar(20) not null default 'NORMAL',

    planned_start date,

    planned_end date,

    actual_start date,

    actual_end date,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_task
        primary key (id),

    constraint uq_task_code
        unique (code),

    constraint fk_task_mission
        foreign key (mission_id)
        references public.mission(id),

    constraint fk_task_type
        foreign key (task_type_id)
        references public.task_type(id),

    constraint fk_task_status
        foreign key (task_status_id)
        references public.task_status(id)

,
    constraint chk_task_planned_dates
        check (
            planned_start is null
            or planned_end is null
            or planned_end >= planned_start
        ),

    constraint chk_task_actual_dates
        check (
            actual_start is null
            or actual_end is null
            or actual_end >= actual_start
        )

);

comment on table public.task is
'Mission Tasks';


-- ============================================================
-- INDEXES
-- ============================================================

create index idx_task_mission
    on public.task(mission_id);

create index idx_task_name
    on public.task(name);

create index idx_task_type
    on public.task(task_type_id);

create index idx_task_status
    on public.task(task_status_id);

create index idx_task_priority
    on public.task(priority);

create index idx_task_planned_start
    on public.task(planned_start);

create index idx_task_planned_end
    on public.task(planned_end);

create index idx_task_actual_start
    on public.task(actual_start);

create index idx_task_actual_end
    on public.task(actual_end);


-- ============================================================
-- END OF MIGRATION
-- ============================================================