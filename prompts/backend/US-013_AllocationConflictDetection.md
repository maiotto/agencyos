# US-013 – Allocation Conflict Detection

Sprint: 7 – Capacity Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the Allocation Conflict Detection Service.

The service identifies operational conflicts caused by assignments that exceed resource capacity or create scheduling overlaps.

Conflict detection provides operational alerts but does not automatically modify assignments.

---

# Business Value

Operational conflicts are one of the primary risks during project execution.

The Decision Engine depends on reliable conflict information before recommending execution strategies.

---

# User Story

As an Operations Manager,

I want the system to identify allocation conflicts,

So that I can proactively resolve operational issues before execution.

---

# Inputs

Planning Period

Execution Resources

Assignments

Capacity

Availability

Working Calendar

---

# Outputs

For every detected conflict return:

- Conflict Id
- Conflict Type
- Execution Resource
- Related Assignment(s)
- Severity
- Description
- Suggested Resolution

---

# Conflict Types

- Capacity Exceeded
- Schedule Overlap
- Resource Unavailable
- Calendar Conflict
- Invalid Assignment

---

# Severity

- Low
- Medium
- High
- Critical

---

# Algorithm

Step 1

Load Active Execution Resources.

Step 2

Retrieve Assignments for the Planning Period.

Step 3

Load Capacity.

Step 4

Load Availability.

Step 5

Detect overlapping assignments.

Step 6

Detect assignments exceeding available capacity.

Step 7

Detect assignments outside the working calendar.

Step 8

Classify conflict severity.

Step 9

Generate suggested resolution.

Step 10

Return ordered conflict list.

---

# Business Rules

BR-001

Cancelled Assignments are ignored.

BR-002

Completed Assignments are ignored.

BR-003

Only Active Resources are evaluated.

BR-004

Conflict detection never changes operational data.

BR-005

Suggested Resolution is informational only.

---

# Service

AllocationConflictDetectionService

---

# API

GET /allocation-conflicts

GET /allocation-conflicts/{resourceId}

GET /allocation-conflicts/summary

---

# Expected Layers

Application

AllocationConflictDetectionService

Infrastructure

Repository queries

API

AllocationConflictController

---

# Validation

Planning Period is required.

---

# Logging

Conflict detection started.

Conflict detection completed.

Number of conflicts detected.

Execution time.

Errors.

---

# Out of Scope

Automatic conflict resolution

Capacity optimization

Delivery Strategy

Artificial Intelligence

Scenario simulation

---

# Definition of Done

✔ Conflict detection implemented

✔ Severity classification implemented

✔ Suggested Resolution generated

✔ Unit Tests implemented

✔ Swagger updated

✔ HTTP Tests updated

✔ Build without warnings

✔ Build without errors