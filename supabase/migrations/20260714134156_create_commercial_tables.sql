-- ============================================================
-- AgencyOS
-- Migration: 20260714134156_create_commercial_tables.sql
-- Sprint 1
-- Domain: Commercial
-- ============================================================

create extension if not exists pgcrypto;

-- ============================================================
-- TABLE: lead
-- ============================================================

create table public.lead (

    id uuid not null default gen_random_uuid(),

    code varchar(30) not null,

    company_name varchar(200) not null,

    trade_name varchar(200),

    website varchar(255),

    segment varchar(100),

    source varchar(100) not null,

    status varchar(30) not null,

    estimated_revenue numeric(15,2),

    owner_id uuid,

    notes text,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_lead
        primary key (id),

    constraint uq_lead_code
        unique (code)

);

comment on table public.lead is
'Commercial Leads';

-- ============================================================
-- TABLE: client
-- ============================================================

create table public.client (

    id uuid not null default gen_random_uuid(),

    lead_id uuid,

    legal_name varchar(200) not null,

    trade_name varchar(200),

    tax_id varchar(30),

    website varchar(255),

    segment varchar(100),

    status varchar(30) not null,

    account_owner uuid,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_client
        primary key (id),

    constraint fk_client_lead
        foreign key (lead_id)
        references public.lead(id)

);

comment on table public.client is
'Agency Clients';

-- ============================================================
-- TABLE: contact
-- ============================================================

create table public.contact (

    id uuid not null default gen_random_uuid(),

    first_name varchar(120) not null,

    last_name varchar(120),

    email varchar(255),

    phone varchar(40),

    mobile varchar(40),

    job_title varchar(120),

    is_primary boolean not null default false,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_contact
        primary key (id)

);

comment on table public.contact is
'Business Contacts';

-- ============================================================
-- TABLE: contract
-- ============================================================

create table public.client_contract (

    id uuid not null default gen_random_uuid(),

    client_id uuid not null,

    contract_number varchar(50),

    name varchar(200) not null,

    billing_model varchar(50) not null,

    value numeric(15,2),

    status varchar(30) not null,

    start_date date,

    end_date date,

    renewal_date date,

    created_at timestamptz not null default now(),

    updated_at timestamptz not null default now(),

    constraint pk_contract
        primary key (id),

    constraint fk_contract_client
        foreign key (client_id)
        references public.client(id)

);

comment on table public.client_contract is
'Commercial Contracts';
-- ============================================================
-- TABLE: lead_contact
-- ============================================================

create table public.lead_contact (

    lead_id uuid not null,

    contact_id uuid not null,

    created_at timestamptz not null default now(),

    constraint pk_lead_contact
        primary key (lead_id, contact_id),

    constraint fk_lead_contact_lead
        foreign key (lead_id)
        references public.lead(id)
        on delete cascade,

    constraint fk_lead_contact_contact
        foreign key (contact_id)
        references public.contact(id)
        on delete cascade

);

comment on table public.lead_contact is
'Relationship between Leads and Contacts';


-- ============================================================
-- TABLE: client_contact
-- ============================================================

create table public.client_contact (

    client_id uuid not null,

    contact_id uuid not null,

    created_at timestamptz not null default now(),

    constraint pk_client_contact
        primary key (client_id, contact_id),

    constraint fk_client_contact_client
        foreign key (client_id)
        references public.client(id)
        on delete cascade,

    constraint fk_client_contact_contact
        foreign key (contact_id)
        references public.contact(id)
        on delete cascade

);

comment on table public.client_contact is
'Relationship between Clients and Contacts';


-- ============================================================
-- INDEXES
-- ============================================================

create index idx_lead_status
    on public.lead(status);

create index idx_lead_company_name
    on public.lead(company_name);

create index idx_client_status
    on public.client(status);

create index idx_client_trade_name
    on public.client(trade_name);

create index idx_client_tax_id
    on public.client(tax_id);

create index idx_contact_email
    on public.contact(email);

create index idx_contact_last_name
    on public.contact(last_name);

create index idx_contract_status
    on public.client_contract(status);

create index idx_contract_client
    on public.client_contract(client_id);

create index idx_contract_start_date
    on public.client_contract(start_date);


-- ============================================================
-- UNIQUE CONSTRAINTS
-- ============================================================

alter table public.client
    add constraint uq_client_tax_id
    unique (tax_id);


-- ============================================================
-- END OF MIGRATION
-- ============================================================