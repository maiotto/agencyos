# US-009 – Resource Assignment

Sprint: 6 – Operational Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the Resource Assignment module.

Assignments connect Tasks to Execution Resources.

An Assignment represents the commitment of a specific Execution Resource to execute a Task.

Execution Resources may represent:

- Internal Human
- External Human
- AI Agent
- AI Service
- Automation

---

# Business Value

Assignments are the operational bridge between planning and execution.

Capacity Planning, Delivery Strategy and AI Recommendations depend on Assignment information.

---

# User Story

As an Operations Manager,

I want to assign Execution Resources to Tasks,

So that operational work can be executed using the most appropriate resources.

---

# Acceptance Criteria

## Create Assignment

The system shall allow assigning one Execution Resource to one Task.

Assignment contains:

- Task
- Execution Resource
- Assignment Role
- Planned Hours
- Planned Start Date
- Planned End Date
- Allocation Percentage
- Status
- Notes

---

## Update Assignment

Assignment information can be updated.

---

## List Assignments

Support filtering by:

- Task
- Mission
- Execution Resource
- Resource Type
- Status

Support ordering by:

- Planned Start Date
- Planned Hours

---

## View Assignment

Return complete Assignment information.

---

## Remove Assignment

Assignment becomes Cancelled.

Historical information remains available.

---

# Business Rules

BR-001

Task must exist.

BR-002

Execution Resource must exist.

BR-003

Execution Resource must be Active.

BR-004

Planned Hours must be greater than zero.

BR-005

Allocation Percentage must be between 1 and 100.

BR-006

Cancelled Assignments cannot be edited.

BR-007

Physical deletion is not allowed.

---

# Assignment Status

- Planned
- Confirmed
- In Progress
- Completed
- Cancelled

---

# Assignment Role

- Responsible
- Reviewer
- Approver
- Contributor

---

# Required API

GET /assignments

GET /assignments/{id}

POST /assignments

PUT /assignments/{id}

DELETE /assignments/{id}

PATCH /assignments/{id}/cancel

---

# Expected Layers

AgencyOS.Domain

- Assignment Entity

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

- Task
- Execution Resource
- Planned Hours
- Planned Start Date
- Planned End Date
- Status

Optional

- Notes

---

# Logging

Generate logs for

- Create
- Update
- Cancel

---

# Error Handling

404

Task not found

Execution Resource not found

400

Validation errors

409

Business rule violation

500

Unexpected error

---

# Out of Scope

Capacity calculation

Availability calculation

Conflict detection

Recommendation Engine

Automatic assignment

AI assignment

---

# Definition of Done

✔ CRUD complete

✔ Validation implemented

✔ Logging implemented

✔ Swagger updated

✔ HTTP Tests updated

✔ Build without warnings

✔ Build without errors

✔ Review approved