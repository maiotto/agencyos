-- ============================================================
-- AgencyOS
-- Migration: 20260726190000_create_holiday_tables.sql
-- Release 1.1 / US-102
-- Domain: Holiday
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: holiday
-- ============================================================

create table public.holiday (

    id uuid not null default gen_random_uuid(),

    company_id uuid,

    name varchar(200) not null,

    description text,

    holiday_type varchar(30) not null,

    holiday_date date not null,

    state_code varchar(10),

    city varchar(120),

    recurring boolean not null default false,

    status varchar(30) not null,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_holiday
        primary key (id),

    constraint chk_holiday_name
        check (char_length(trim(name)) > 0),

    constraint chk_holiday_type
        check (holiday_type in ('National', 'State', 'Municipal', 'Company')),

    constraint chk_holiday_status
        check (status in ('Active', 'Inactive')),

    constraint chk_holiday_company_scope
        check (
            holiday_type <> 'Company'
            or company_id is not null
        ),

    constraint chk_holiday_state_scope
        check (
            holiday_type not in ('State', 'Municipal')
            or (state_code is not null and char_length(trim(state_code)) > 0)
        ),

    constraint chk_holiday_municipal_scope
        check (
            holiday_type <> 'Municipal'
            or (city is not null and char_length(trim(city)) > 0)
        ),

    constraint chk_holiday_national_scope
        check (
            holiday_type <> 'National'
            or (state_code is null and city is null)
        )

);

comment on table public.holiday is
'Company and geographic holidays used as non-working days for Capacity Planning';

comment on column public.holiday.recurring is
'When true, the holiday repeats every year on the same month and day';

-- ============================================================
-- INDEXES
-- ============================================================

create index idx_holiday_company_id
    on public.holiday(company_id);

create index idx_holiday_status
    on public.holiday(status);

create index idx_holiday_type
    on public.holiday(holiday_type);

create index idx_holiday_date
    on public.holiday(holiday_date);

create index idx_holiday_company_status
    on public.holiday(company_id, status);

create index idx_holiday_type_date
    on public.holiday(holiday_type, holiday_date);

create index idx_holiday_name
    on public.holiday(name);

create index idx_holiday_state_code
    on public.holiday(state_code);

create index idx_holiday_city
    on public.holiday(city);

create index idx_holiday_recurring
    on public.holiday(recurring);

create index idx_holiday_scope_lookup
    on public.holiday(company_id, holiday_type, holiday_date, state_code, city);


-- ============================================================
-- END OF MIGRATION
-- ============================================================
