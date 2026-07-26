-- ============================================================
-- AgencyOS
-- Migration: 20260726120000_add_client_lead_id_unique.sql
-- WP-Lead-001
-- Domain: Commercial
-- ============================================================
-- Enforces one Client per Lead when lead_id is present.
-- Multiple NULL lead_id values remain allowed.
-- ============================================================

create unique index if not exists uq_client_lead_id
    on public.client (lead_id)
    where lead_id is not null;

comment on index public.uq_client_lead_id is
'Ensures at most one Client originates from a given Lead.';
