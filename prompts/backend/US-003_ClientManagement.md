# US-003 – Client Management

Sprint: 5 – Commercial Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the complete Client Management module.

Clients represent organizations that maintain one or more commercial contracts with the company.

This module is responsible for maintaining the master data used throughout the operational lifecycle.

Lead

↓

Client

↓

Contract

↓

Delivery Strategy

---

# Business Value

Clients are the central commercial entity of AgencyOS.

Every Contract must belong to a Client.

The Decision Engine depends on accurate Client information for future operational recommendations.

---

# User Story

As a Commercial Manager,

I want to create and maintain Clients,

So that commercial contracts can be associated with a valid organization.

---

# Acceptance Criteria

## Create Client

The system shall allow creating a Client containing:

- Legal Name
- Trade Name
- Tax Identifier
- Email
- Phone
- Website
- Industry
- Company Size
- Status
- Notes

---

## Update Client

Commercial information can be updated.

---

## List Clients

Return paginated list.

Support filtering by:

- Status
- Industry
- Company Name

Support ordering by:

- Company Name
- Created Date

---

## View Client

Return complete Client information.

---

## Deactivate Client

Client becomes Inactive.

Historical information must be preserved.

Existing Contracts remain unchanged.

---

# Business Rules

BR-001

Legal Name is mandatory.

BR-002

Tax Identifier must be unique.

BR-003

Inactive Clients cannot receive new Contracts.

BR-004

Existing Contracts remain valid after Client deactivation.

BR-005

Clients with active Contracts cannot be deleted.

BR-006

Physical deletion is not allowed.

---

# Client Status

- Active
- Inactive

---

# Required API Endpoints

GET /clients

GET /clients/{id}

POST /clients

PUT /clients/{id}

DELETE /clients/{id}

---

# Expected Layers

AgencyOS.Domain

- Client Entity

AgencyOS.Application

- DTOs
- Validators
- Service
- Interfaces

AgencyOS.Infrastructure

- Repository
- EF Mapping

AgencyOS.Api

- Controller
- Swagger

---

# Validation

Required

- Legal Name
- Status

Optional

- Trade Name
- Email
- Phone
- Website
- Notes

---

# Logging

Generate logs for:

- Create
- Update
- Deactivate

---

# Error Handling

404

Client not found

400

Validation errors

409

Duplicate Tax Identifier

500

Unexpected error

---

# Out of Scope

Contracts

Contacts

AI

Decision Engine

Notifications

---

# Definition of Done

✔ CRUD complete

✔ Swagger updated

✔ HTTP file updated

✔ Validation implemented

✔ Logging implemented

✔ Build without warnings

✔ Build without errors

✔ Review approved