# US-011 – Workload Calculator

Sprint: 7 – Capacity Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the Workload Calculator Service.

The service calculates the operational workload of every Execution Resource during a planning period.

Workload represents the total amount of planned work assigned to a resource.

---

# Business Value

Workload is one of the fundamental indicators used by AgencyOS to understand operational utilization and identify overloaded or underutilized resources.

This service provides the basis for Capacity Planning and future AI recommendations.

---

# User Story

As an Operations Manager,

I want to calculate the workload of every Execution Resource,

So that I can understand current and future operational demand.

---

# Inputs

Planning Period

Execution Resources

Assignments

Task Planned Hours

Assignment Status

---

# Outputs

For every Execution Resource return:

- Total Planned Hours
- Number of Assignments
- Average Hours per Assignment
- Workload Percentage
- Assignment Distribution

---

# Algorithm

Step 1

Load all Active Execution Resources.

Step 2

Load all Assignments within the Planning Period.

Step 3

Ignore Cancelled Assignments.

Step 4

Ignore Completed Assignments.

Step 5

Group Assignments by Execution Resource.

Step 6

Sum Planned Hours.

Step 7

Calculate:

Workload Percentage

=

Allocated Hours

/

Capacity Hours

×

100

Step 8

Return workload summary.

---

# Business Rules

BR-001

Only Active Resources participate.

BR-002

Cancelled Assignments are ignored.

BR-003

Completed Assignments are ignored.

BR-004

Planning Period is mandatory.

BR-005

Workload Percentage cannot be negative.

---

# Service

WorkloadCalculatorService

---

# API

GET /workload

GET /workload/{resourceId}

GET /workload/summary

---

# Expected Layers

Application

WorkloadCalculatorService

Infrastructure

Repository queries

API

WorkloadController

---

# Validation

Planning Period required.

---

# Logging

Workload calculation started.

Workload calculation completed.

Execution time.

Errors.

---

# Out of Scope

Capacity optimization

Recommendations

AI

Scenario simulation

---

# Definition of Done

✔ Workload correctly calculated

✔ Unit Tests implemented

✔ Swagger updated

✔ HTTP Tests updated

✔ Build without warnings

✔ Build without errors