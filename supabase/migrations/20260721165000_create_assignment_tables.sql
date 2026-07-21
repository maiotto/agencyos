-- ============================================================
-- AgencyOS
-- Migration: 20260721165000_create_assignment_tables.sql
-- Sprint 6
-- Domain: Resource Assignment
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: assignment
-- ============================================================

create table public.assignment (

    id uuid not null default gen_random_uuid(),

    task_id uuid not null,

    execution_resource_id uuid not null,

    assignment_role varchar(30) not null,

    planned_hours numeric(10,2) not null,

    planned_start_date date not null,

    planned_end_date date not null,

    allocation_percentage integer not null,

    status varchar(30) not null,

    notes text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_assignment
        primary key (id),

    constraint fk_assignment_task
        foreign key (task_id)
        references public.task(id),

    constraint fk_assignment_execution_resource
        foreign key (execution_resource_id)
        references public.execution_resource(id),

    constraint chk_assignment_planned_hours
        check (planned_hours > 0),

    constraint chk_assignment_allocation_percentage
        check (allocation_percentage between 1 and 100),

    constraint chk_assignment_planned_dates
        check (planned_end_date >= planned_start_date)

);

comment on table public.assignment is
'Resource Assignments connecting Tasks to Execution Resources';


-- ============================================================
-- INDEXES
-- ============================================================

create index idx_assignment_task
    on public.assignment(task_id);

create index idx_assignment_execution_resource
    on public.assignment(execution_resource_id);

create index idx_assignment_status
    on public.assignment(status);

create index idx_assignment_planned_start_date
    on public.assignment(planned_start_date);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
