# US-002 – Lead Management

Sprint: 5 – Commercial Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the complete Lead Management module.

The objective is to allow the commercial team to register, qualify and manage business opportunities before they become clients.

This module represents the first step of the AgencyOS commercial workflow.

Lead

↓

Client

↓

Contract

↓

Delivery Strategy

---

# Business Value

Without Lead Management there is no commercial pipeline.

The Decision Engine depends on reliable commercial information before evaluating contract viability.

---

# User Story

As a Commercial Manager,

I want to register and manage Leads,

So that commercial opportunities can be tracked until they become Clients.

---

# Acceptance Criteria

## Create Lead

The system shall allow creating a Lead containing:

- Company Name
- Lead Name
- Email
- Phone
- Source
- Status
- Estimated Contract Value
- Expected Close Date
- Notes

---

## Update Lead

Commercial information can be updated.

---

## List Leads

Return paginated list.

Support filtering by:

- Status
- Company
- Expected Close Date
- Assigned User

---

## View Lead

Return all Lead information.

---

## Archive Lead

Lead is marked as Archived.

Historical information must be preserved.

---

## Convert Lead

Lead can be converted into Client.

Conversion must:

- create Client
- preserve Lead history
- change Lead status to Converted

---

# Business Rules

BR-001

Email must be unique among active Leads.

BR-002

Archived Leads cannot be edited.

BR-003

Converted Leads cannot return to Prospect.

BR-004

Only active Leads can be converted.

BR-005

Every Lead must have a Status.

---

# Lead Status

- Prospect
- Qualified
- Proposal
- Negotiation
- Won
- Lost
- Converted
- Archived

---

# Required API Endpoints

GET /leads

GET /leads/{id}

POST /leads

PUT /leads/{id}

DELETE /leads/{id}

POST /leads/{id}/convert

---

# Expected Layers

AgencyOS.Domain

- Lead Entity

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

- Company Name
- Lead Name
- Status

Optional

- Email
- Phone
- Notes
- Estimated Contract Value

---

# Logging

Generate logs for

- Create
- Update
- Archive
- Convert

---

# Error Handling

404

Lead not found

400

Validation errors

409

Duplicate email

500

Unexpected error

---

# Out of Scope

AI

Capacity Planning

Recommendation Engine

Notifications

Email integration

CRM synchronization

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