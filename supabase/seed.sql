-- ============================================================
-- AgencyOS
-- Seed: seed.sql
-- WP-003 – Seed Data Foundation
-- ============================================================
--
-- Deterministic UUID scheme (stable across every db reset):
--   mission_type:   11111111-1111-4111-8111-00000000000N  (N = 1..5)
--   mission_status: 11111111-1111-4111-8121-00000000000N  (N = 1..5)
--   task_type:      11111111-1111-4111-8131-00000000000N  (N = 1..6)
--   task_status:    11111111-1111-4111-8141-00000000000N  (N = 1..6)
--
-- Reference data only. Transactional tables are intentionally excluded.
-- ============================================================

-- ============================================================
-- TABLE: mission_type
-- ============================================================

insert into public.mission_type (id, code, name, description, created_at, updated_at)
values
    (
        '11111111-1111-4111-8111-000000000001',
        'CONTENT_PRODUCTION',
        'Content Production',
        'End-to-end production of audiovisual and digital content deliverables.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8111-000000000002',
        'MARKETING_CAMPAIGN',
        'Marketing Campaign',
        'Integrated marketing campaigns spanning creative, media and activation.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8111-000000000003',
        'CONSULTING',
        'Consulting',
        'Advisory engagements covering strategy, operations and transformation.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8111-000000000004',
        'SOFTWARE_DEVELOPMENT',
        'Software Development',
        'Design and delivery of custom software products and integrations.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8111-000000000005',
        'SUPPORT',
        'Support',
        'Ongoing support, maintenance and operational assistance missions.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    );


-- ============================================================
-- TABLE: mission_status
-- ============================================================

insert into public.mission_status (id, code, name, description, created_at, updated_at)
values
    (
        '11111111-1111-4111-8121-000000000001',
        'DRAFT',
        'Draft',
        'Mission is being defined and is not yet approved for execution.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8121-000000000002',
        'PLANNED',
        'Planned',
        'Mission scope, timeline and resources are approved but work has not started.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8121-000000000003',
        'IN_PROGRESS',
        'In Progress',
        'Mission execution is actively underway.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8121-000000000004',
        'COMPLETED',
        'Completed',
        'All mission deliverables were accepted and the mission is closed.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8121-000000000005',
        'CANCELLED',
        'Cancelled',
        'Mission was cancelled before or during execution.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    );


-- ============================================================
-- TABLE: task_type
-- ============================================================

insert into public.task_type (id, code, name, description, created_at, updated_at)
values
    (
        '11111111-1111-4111-8131-000000000001',
        'PRODUCTION',
        'Production',
        'Primary production work such as filming, recording or asset creation.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8131-000000000002',
        'EDITING',
        'Editing',
        'Post-production editing, refinement and assembly of deliverables.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8131-000000000003',
        'REVIEW',
        'Review',
        'Quality review, feedback cycles and internal validation.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8131-000000000004',
        'APPROVAL',
        'Approval',
        'Formal client or stakeholder approval checkpoints.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8131-000000000005',
        'PUBLISHING',
        'Publishing',
        'Final packaging, distribution and publication of approved assets.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8131-000000000006',
        'MEETING',
        'Meeting',
        'Coordination meetings, workshops and alignment sessions.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    );


-- ============================================================
-- TABLE: task_status
-- ============================================================
--
-- Codes align with MissionTaskStatus domain constants for API compatibility.
--

insert into public.task_status (id, code, name, description, created_at, updated_at)
values
    (
        '11111111-1111-4111-8141-000000000001',
        'Draft',
        'Draft',
        'Task is defined but not yet ready for execution.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8141-000000000002',
        'Planned',
        'Planned',
        'Task is scheduled and awaiting start.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8141-000000000003',
        'In Progress',
        'In Progress',
        'Task execution is actively underway.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8141-000000000004',
        'Blocked',
        'Blocked',
        'Task cannot proceed until a dependency or issue is resolved.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8141-000000000005',
        'Completed',
        'Completed',
        'Task deliverables were accepted and the task is closed.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    ),
    (
        '11111111-1111-4111-8141-000000000006',
        'Cancelled',
        'Cancelled',
        'Task was cancelled before or during execution.',
        '2026-07-22 00:00:00+00',
        '2026-07-22 00:00:00+00'
    );
