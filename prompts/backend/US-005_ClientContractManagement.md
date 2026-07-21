# US-005 – Client Contract Management

Sprint: 5 – Commercial Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the complete Client Contract Management module.

A Client Contract represents a commercial agreement between the company and a Client.

Contracts are the primary input for the AgencyOS Decision Engine.

Every execution strategy starts from a Contract.

---

# Business Value

The Client Contract is the bridge between Commercial and Operations.

Once approved, a Contract becomes the source for Delivery Strategy recommendations, Mission creation and operational planning.

---

# User Story

As a Commercial Manager,

I want to register and manage Client Contracts,

So that every commercial agreement becomes an executable operational commitment.

---

# Acceptance Criteria

## Create Contract

The system shall allow creating a Contract containing:

- Client
- Contract Code
- Contract Name
- Contract Type
- Status
- Description
- Start Date
- End Date
- Estimated Value
- Currency
- Estimated Hours
- Priority
- SLA
- Commercial Owner
- Notes

---

## Update Contract

Commercial information can be updated.

---

## List Contracts

Return paginated list.

Support filtering by:

- Client
- Status
- Contract Type
- Commercial Owner
- Start Date
- End Date

Support ordering by:

- Contract Name
- Client
- Start Date
- Estimated Value

---

## View Contract

Return complete Contract information.

---

## Activate Contract

A Draft Contract can become Active.

---

## Close Contract

Contract becomes Closed.

Historical information must remain available.

---

## Cancel Contract

Contract becomes Cancelled.

Historical information must remain available.

---

# Business Rules

BR-001

Every Contract belongs to one Client.

BR-002

Contract Code must be unique.

BR-003

Only Active Clients may receive new Contracts.

BR-004

Closed Contracts cannot be edited.

BR-005

Cancelled Contracts cannot be reactivated.

BR-006

Estimated Value must be greater than zero.

BR-007

Start Date must not be after End Date.

BR-008

Physical deletion is not allowed.

---

# Contract Status

- Draft
- Active
- Suspended
- Closed
- Cancelled

---

# Contract Type

- Fixed Price
- Time & Material
- Monthly Retainer
- Managed Service
- Subscription
- Other

---

# Required API Endpoints

GET /contracts

GET /contracts/{id}

POST /contracts

PUT /contracts/{id}

DELETE /contracts/{id}

PATCH /contracts/{id}/activate

PATCH /contracts/{id}/close

PATCH /contracts/{id}/cancel

---

# Expected Layers

AgencyOS.Domain

- ClientContract Entity

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

- Client
- Contract Code
- Contract Name
- Contract Type
- Status
- Start Date
- Estimated Value

Optional

- Description
- End Date
- Estimated Hours
- SLA
- Notes

---

# Logging

Generate logs for

- Create
- Update
- Activate
- Close
- Cancel

---

# Error Handling

404

Contract not found

400

Validation errors

409

Duplicate Contract Code

500

Unexpected error

---

# Out of Scope

Mission creation

Task generation

Capacity Planning

Decision Engine

AI Recommendation

Financial integration

ERP integration

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