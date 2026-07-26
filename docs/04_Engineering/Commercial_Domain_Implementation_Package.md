# Commercial Domain Implementation Package

**Project:** AgencyOS MVP 1.0  
**Package:** Commercial Domain  
**Version:** 1.0  
**Status:** Approved for Implementation  
**Target:** Cursor AI  
**Baseline:** AgencyOS MVP 1.0

---

# Purpose

This Implementation Package defines exactly what shall be implemented for the Commercial Domain of AgencyOS MVP 1.0.

This document consolidates the approved Functional Specifications into a single execution package.

No architectural or functional decisions shall be made during implementation.

Whenever documentation conflicts exist, precedence is:

1. AgencyOS Baseline
2. Approved ADRs
3. Program Architecture
4. Functional Specifications
5. This Implementation Package

---

# Implementation Objective

Implement the complete Commercial Domain following the approved architecture and engineering standards.

The Commercial Domain is responsible for the complete commercial lifecycle.

Business Flow

Lead

↓

Client

↓

Client Contact

↓

Client Contract

Only approved commercial demand leaves this domain.

---

# Scope

The implementation includes:

- Lead Management
- Client Management
- Client Contact Management
- Client Contract Management

The implementation excludes:

- Missions
- Tasks
- Planning
- Decision Engine
- Frontend
- Decision Intelligence

---

# Implementation Order

Implementation shall follow the business lifecycle.

```
Lead
    ↓
Client
    ↓
Client Contact
    ↓
Client Contract
```

Implementation shall not change this order.

---

# Architecture

Implementation shall follow the official AgencyOS layered architecture.

```
Presentation

↓

Application

↓

Domain

↓

Infrastructure

↓

Database
```

Business rules belong to the Domain/Application layers.

Controllers shall never contain business logic.

Repositories shall contain persistence only.

---

# Entity 1 — Lead

Purpose

Represents a commercial opportunity that has not yet become a customer.

Required Deliverables

- Domain Entity
- Repository Interface
- Repository Implementation
- Application Service
- Create DTO
- Update DTO
- Response DTO
- Validators
- REST Controller
- Swagger
- HTTP Tests
- Unit Tests

Minimum CRUD

- Create Lead
- Get Lead
- List Leads
- Update Lead
- Delete Lead

---

# Entity 2 — Client

Purpose

Represents an approved customer.

Dependencies

Lead may become Client.

Required Deliverables

- Domain Entity
- Repository
- Service
- DTOs
- Validators
- Controller
- Swagger
- Tests

Minimum CRUD

- Create
- Read
- List
- Update
- Delete

---

# Entity 3 — Client Contact

Purpose

Represents contacts associated with a client.

Relationship

One Client

↓

Many Contacts

Required Deliverables

- Entity
- Repository
- Service
- DTOs
- Validators
- Controller
- Tests

CRUD Required.

---

# Entity 4 — Client Contract

Purpose

Represents approved commercial agreements.

Relationship

One Client

↓

Many Contracts

Required Deliverables

- Entity
- Repository
- Service
- DTOs
- Validators
- Controller
- Tests

CRUD Required.

---

# Application Layer

Each entity shall expose:

Application Service

Responsibilities

- Validation
- Business orchestration
- Repository access

Services shall not contain persistence logic.

---

# Repository Layer

Each entity shall expose:

Repository Interface

Repository Implementation

Repositories shall expose only persistence operations.

---

# Controllers

Each entity shall expose REST endpoints.

Minimum operations

GET

GET BY ID

POST

PUT

DELETE

Controllers shall be thin.

Controllers shall never contain business rules.

---

# DTOs

Each entity shall include

Create DTO

Update DTO

Response DTO

Search DTO (when required)

Domain entities shall never be exposed directly.

---

# Validation

Validation shall use FluentValidation.

Typical validations include

Required fields

Maximum length

Business rules

Relationship validation

Controllers shall never validate business rules.

---

# Dependency Injection

Repositories

Services

Validators

shall all be registered using Dependency Injection.

---

# Logging

Application Services shall generate structured logs for

Create

Update

Delete

Business validation failures

Unexpected exceptions

---

# Exception Handling

Use the standard AgencyOS exception handling strategy.

Controllers shall return standardized HTTP responses.

---

# Swagger

Every endpoint shall include

Summary

Description

Request example

Response example

HTTP response codes

---

# Tests

## Unit Tests

Every Service

Every Validator

Business Rules

---

## Repository Tests

Persistence validation.

---

## HTTP Tests

Every endpoint.

CRUD validation.

---

# Quality Gates

Implementation shall not be considered complete unless

Build succeeds

Zero warnings

Zero errors

Swagger working

HTTP Tests passing

Unit Tests passing

Dependency Injection working

Logging implemented

Validation implemented

---

# Restrictions

The implementation SHALL NOT

Change architecture

Create new entities

Change the database model

Introduce new business rules

Modify approved documentation

Refactor unrelated modules

Implement future roadmap features

Use external AI services

---

# Expected Folder Structure

Commercial

```
Domain

Entities

Repositories

Application

Services

DTOs

Validators

Infrastructure

Repositories

Presentation

Controllers
```

The implementation shall follow the existing project structure.

---

# Definition of Done

The Commercial Domain implementation is complete only when

✓ Lead implemented

✓ Client implemented

✓ Client Contact implemented

✓ Client Contract implemented

✓ CRUD complete

✓ Services complete

✓ Validators complete

✓ Controllers complete

✓ Swagger complete

✓ Tests passing

✓ Build successful

✓ Zero warnings

✓ Documentation updated

✓ Engineering Review approved

---

# Cursor Execution Instructions

Implement the Commercial Domain exactly as described in this package.

Do not redesign architecture.

Do not modify business rules.

Do not introduce additional abstractions.

Do not implement future roadmap capabilities.

If implementation uncertainty exists, preserve the approved documentation rather than making assumptions.

This package is the execution contract for the Commercial Domain implementation.

---

# End of Package