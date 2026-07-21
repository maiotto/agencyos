-- ============================================================
-- AgencyOS
-- Migration: add_task_estimated_hours.sql
-- Sprint 6
-- Domain: Task
-- ============================================================

alter table public.task
    add column estimated_hours numeric(10, 2) not null default 1;

alter table public.task
    alter column estimated_hours drop default;

comment on column public.task.estimated_hours is
'Estimated effort in hours required to complete the task.';

-- ============================================================
-- END OF MIGRATION
-- ============================================================
