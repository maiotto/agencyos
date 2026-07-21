# US-001 — Mission CRUD

## Epic

Mission Management

---

## Objective

Implement the complete CRUD for the Mission entity.

The database schema already exists in Supabase.

Do NOT create migrations.

Use the existing database.

---

## Business Context

A Mission represents a project contracted by a client.

A Mission belongs to exactly one Client Contract.

A Mission contains multiple Tasks.

---

## Architecture

Follow all project standards defined in:

prompts/system/

- 00_Project_Context.md
- 01_Engineering_Principles.md
- 02_Coding_Standards.md
- 03_Definition_of_Done.md
- 04_Tech_Stack.md

---

## Projects

Implement only in:

AgencyOS.Domain

AgencyOS.Application

AgencyOS.Infrastructure

AgencyOS.Api

---

## Database

Existing table

mission

Existing relationships

mission_type

mission_status

client_contract

---

## Required Endpoints

GET /missions

GET /missions/{id}

POST /missions

PUT /missions/{id}

DELETE /missions/{id}

---

## Domain Entity

Mission

Properties

- Id
- ClientContractId
- Code
- Name
- Description
- MissionTypeId
- MissionStatusId
- Priority
- StartDate
- EndDate
- CreatedAt
- UpdatedAt

---

## Application Layer

Create

DTOs

Interfaces

Services

Validators

---

## Infrastructure

Create

MissionRepository

ApplicationDbContext configuration

Entity Framework mapping

---

## API

Create

MissionController

Swagger documentation

Dependency Injection

---

## Validation

Use FluentValidation.

Required

Code

Name

ClientContractId

MissionTypeId

MissionStatusId

---

## Logging

Log

Mission Created

Mission Updated

Mission Deleted

---

## Constraints

Do NOT modify the database.

Do NOT create migrations.

Do NOT redesign the architecture.

Do NOT introduce new frameworks.

Use Entity Framework Core.

Use asynchronous methods.

Follow REST conventions.

---

## Acceptance Criteria

The solution builds successfully.

Swagger exposes all endpoints.

CRUD operates against Supabase.

No build warnings introduced.

Architecture remains compliant.

Definition of Done satisfied.