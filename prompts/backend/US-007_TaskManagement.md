# US-007 – Task Management

Sprint: 6 – Operational Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the complete Task Management module.

Tasks represent executable work required to complete a Mission.

Every Task belongs to one Mission.

Tasks may later be assigned to Human Resources, External Resources or AI Operational Resources.

---

# Business Value

Tasks represent the smallest unit of operational planning.

Capacity Planning and the Decision Engine depend on accurate Task information.

---

# User Story

As an Operations Manager,

I want to create and manage Tasks,

So that every Mission can be executed through structured operational activities.

---

# Acceptance Criteria

## Create Task

The system shall allow creating a Task containing:

- Mission
- Task Code
- Task Name
- Description
- Task Type
- Status
- Priority
- Estimated Hours
- Planned Start Date
- Planned End Date

---

## Update Task

Task information can be updated.

---

## List Tasks

Support filtering by:

- Mission
- Status
- Priority
- Task Type

---

## View Task

Return complete Task information.

---

## Close Task

Status becomes Completed.

Historical information must remain.

---

# Business Rules

BR-001

Every Task belongs to one Mission.

BR-002

Estimated Hours must be greater than zero.

BR-003

Completed Tasks cannot be edited.

BR-004

Task Code must be unique inside the Mission.

---

# Task Status

- Draft
- Planned
- In Progress
- Blocked
- Completed
- Cancelled

---

# Priority

- Critical
- High
- Medium
- Low

---

# Required API

GET /tasks

GET /tasks/{id}

POST /tasks

PUT /tasks/{id}

DELETE /tasks/{id}

PATCH /tasks/{id}/complete

---

# Definition of Done

✔ CRUD complete

✔ Validation implemented

✔ Logging implemented

✔ Swagger updated

✔ HTTP Tests updated

✔ Build OK