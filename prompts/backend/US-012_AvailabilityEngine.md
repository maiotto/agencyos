# US-012 – Availability Engine

Sprint: 7 – Capacity Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the Availability Engine.

The service calculates when an Execution Resource is available to receive additional work.

Availability considers capacity, current assignments, working calendar and planning period.

---

# Business Value

Availability is a fundamental operational indicator.

Unlike Capacity, which measures how much work can be performed, Availability determines when work can be scheduled.

The Decision Engine will use this service to evaluate execution strategies.

---

# User Story

As an Operations Manager,

I want to know when each Execution Resource is available,

So that new work can be planned without creating scheduling conflicts.

---

# Inputs

Planning Period

Execution Resources

Assignments

Working Calendar

Capacity

Workload

---

# Outputs

For every Execution Resource return:

- Next Available Date
- Available Hours
- Occupied Hours
- Availability Percentage
- Available Time Slots

---

# Algorithm

Step 1

Load all Active Execution Resources.

Step 2

Retrieve Capacity for the planning period.

Step 3

Retrieve Workload for the planning period.

Step 4

Load Working Calendar.

Step 5

Subtract occupied periods.

Step 6

Identify available periods.

Step 7

Calculate:

Availability Percentage

=

Available Hours

/

Capacity Hours

×

100

Step 8

Return ordered availability.

---

# Business Rules

BR-001

Only Active Resources are considered.

BR-002

Cancelled Assignments are ignored.

BR-003

Completed Assignments are ignored.

BR-004

Working Calendar must be respected.

BR-005

Availability cannot exceed Capacity.

---

# Service

AvailabilityEngineService

---

# API

GET /availability

GET /availability/{resourceId}

GET /availability/summary

---

# Expected Layers

Application

AvailabilityEngineService

Infrastructure

Repository queries

API

AvailabilityController

---

# Validation

Planning Period is required.

---

# Logging

Availability calculation started.

Availability calculation completed.

Execution time.

Errors.

---

# Out of Scope

Recommendations

Optimization

AI

Automatic scheduling

Conflict resolution

---

# Definition of Done

✔ Availability correctly calculated

✔ Unit Tests implemented

✔ Swagger updated

✔ HTTP Tests updated

✔ Build without warnings

✔ Build without errors