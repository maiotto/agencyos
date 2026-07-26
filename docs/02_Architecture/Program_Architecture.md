# AgencyOS Program Architecture

Version: 2.0

Status: Official Baseline

Baseline: 1.0

Date: 2026-07

---

# Overview

AgencyOS is an AI-First Goal-Oriented Operational Planning Platform designed for service companies whose operational execution depends on limited productive capacity.

Unlike traditional ERP, CRM or Project Management systems, AgencyOS does not organize work around customers, projects or tasks.

AgencyOS organizes the company around productive capacity and operational decision making.

Its purpose is to continuously answer questions such as:

- Can we sell this contract?
- Do we have enough capacity?
- Should we hire?
- Should we outsource?
- Which execution strategy is the best?
- What happens if constraints change?
- Which operational plan maximizes business objectives?

AgencyOS combines deterministic operational engines with Artificial Intelligence to generate, evaluate, optimize and explain execution strategies.

---

# Program Structure

AgencyOS consists of two independent strategic programs.

AgencyOS Program

│

├── Program A

│     AgencyOS Product

│

└── Program B

      AgencyOS AI Factory

Both programs share the same long-term vision.

However, they evolve independently.

The AI Factory exists to accelerate software engineering.

The AgencyOS Product exists to solve customer business problems.

Neither program dictates the roadmap of the other.

---

# Program A — AgencyOS Product

## Vision

Build the world's best operational planning platform for service businesses.

AgencyOS transforms business objectives into executable operational strategies.

---

## Mission

Convert commercial demand into optimized operational execution.

The platform continuously evaluates:

- business objectives
- operational constraints
- productive capacity
- execution alternatives

before recommending the best strategy.

---

# Product Evolution

AgencyOS evolved through four major architectural stages.

Phase 1

Capacity Planning

↓

Operational Planning

↓

Decision Support

↓

Decision Intelligence

Each phase extends the previous one.

No architectural redesign invalidated previous work.

---

# Product Principles

AgencyOS is based on the following principles.

## Capacity First

Capacity is the central business asset.

Every commercial decision must consider productive capacity before acceptance.

---

## Hours as Operational Unit

AgencyOS does not measure production by:

- videos
- projects
- campaigns

The operational unit is productive hours.

Every service consumes hours.

Every resource provides hours.

Every decision balances hours.

---

## Goal-Oriented Planning

Planning starts from business objectives.

Not from available resources.

Example

Business Goal

↓

Deliver 25 videos

↓

Budget

↓

Deadline

↓

Quality

↓

AgencyOS generates the operational strategy.

Resources become part of the solution.

Not the starting point.

---

## Explainable Decisions

Every recommendation must explain:

Why this strategy was selected.

Why competing strategies were rejected.

Every recommendation must be auditable.

---

## Deterministic Core

Business calculations must always be deterministic.

Given identical inputs the system must always produce identical outputs.

Artificial Intelligence never replaces deterministic business calculations.

It complements them.

---

# Product Scope

AgencyOS supports the complete operational lifecycle.

Lead

↓

Client

↓

Contract

↓

Mission

↓

Task

↓

Execution Resource

↓

Assignment

↓

Capacity Planning

↓

Decision Engine

↓

Decision Intelligence

↓

Execution

↓

Continuous Replanning

---

# High-Level Architecture

AgencyOS is organized into six architectural layers.

Presentation Layer

↓

Application Layer

↓

Business Domain Layer

↓

Operational Intelligence Layer

↓

Decision Layer

↓

Decision Intelligence Layer

Each layer depends only on lower layers.

Dependencies always point inward.

No circular dependency is permitted.

---

# Layer 1 — Presentation

Responsibilities

- REST APIs
- Future Web Application
- Future Mobile Application
- External Integrations

Current implementation

Backend REST APIs.

Future implementation

React Frontend.

---

# Layer 2 — Application

Responsibilities

- Use Cases
- Application Services
- Validation
- Authorization
- Logging
- Transactions
- Orchestration

The Application Layer never contains business rules.

Business rules belong to the Domain Layer.

---

# Layer 3 — Business Domains

Business Domains model the operational reality of the organization.

Current domains

Commercial

Operations

Planning

Decision

Future domains

Financial

Analytics

Simulation

Optimization

Knowledge

Each domain owns:

- entities
- repositories
- services
- business rules

No domain directly modifies another domain.

Interaction occurs only through public interfaces.

---

# Commercial Domain

Purpose

Manage the commercial lifecycle.

Entities

Lead

Client

Client Contact

Client Contract

Mission

Responsibilities

- Customer acquisition
- Opportunity management
- Contract management
- Commercial history

Commercial Domain is responsible for demand generation.

Operational execution begins only after Contract approval.
---

# Operations Domain

Purpose

Transform commercial commitments into executable operational work.

The Operations Domain is responsible for organizing execution independently of resource availability.

Its responsibility ends when the operational structure is completely defined.

Entities

Mission

Task

Execution Resource

Assignment

Relationships

Contract

↓

Mission

↓

Task

↓

Assignment

↓

Execution Resource

Responsibilities

- Mission management
- Task management
- Resource allocation
- Assignment lifecycle
- Operational execution structure

The Operations Domain does not calculate capacity.

Capacity belongs to the Planning Layer.

---

# Planning Layer

Purpose

Transform operational data into measurable operational intelligence.

The Planning Layer is completely deterministic.

No Artificial Intelligence is used.

It provides reusable analytical services consumed by higher-level engines.

Current Planning Engines

Capacity Engine

Workload Engine

Availability Engine

Allocation Conflict Detection Engine

Each engine has a single responsibility.

---

## Capacity Engine

Purpose

Calculate productive capacity.

Inputs

Execution Resources

Working Calendar

Assignments

Business Calendar

Outputs

Available Hours

Consumed Hours

Remaining Capacity

Capacity Percentage

Responsibilities

- Installed Capacity
- Planned Capacity
- Remaining Capacity
- Capacity by Profile
- Capacity by Resource

---

## Workload Engine

Purpose

Measure planned operational workload.

Responsibilities

- Planned Hours
- Assigned Hours
- Workload Distribution
- Operational Load

Outputs

Workload per:

- Resource
- Mission
- Contract
- Period

---

## Availability Engine

Purpose

Determine operational availability.

Inputs

Capacity

Workload

Assignments

Outputs

Available Resources

Unavailable Resources

Available Time Slots

Availability Matrix

---

## Allocation Conflict Detection

Purpose

Detect operational inconsistencies.

Responsibilities

Identify

- Overallocation

- Schedule conflicts

- Resource conflicts

- Invalid assignments

Produces

Conflict Report

Operational Warnings

No corrections are performed.

Only detection.

---

# Planning Pipeline

Capacity

↓

Workload

↓

Availability

↓

Allocation Conflict Detection

Each stage consumes outputs produced by previous stages.

No stage recalculates upstream information.

---

# Decision Layer

Purpose

Recommend the best operational strategy.

Unlike Planning Engines, the Decision Layer compares multiple execution alternatives.

The Decision Layer remains deterministic.

Current implementation corresponds to the MVP Decision Engine.

---

# Decision Engine

The Decision Engine consists of four independent services.

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

Each service has exactly one responsibility.

---

## Delivery Strategy Builder

Purpose

Generate valid execution strategies.

Inputs

Contracts

Missions

Tasks

Execution Resources

Policies

Business Constraints

Outputs

Candidate Strategies

The Builder does not evaluate quality.

It only generates feasible alternatives.

---

## Delivery Strategy Evaluator

Purpose

Measure each candidate strategy.

Metrics include

Operational Cost

Execution Time

Capacity Consumption

Risk

Resource Utilization

Constraint Violations

The Evaluator does not rank strategies.

---

## Delivery Strategy Ranking

Purpose

Order evaluated strategies.

Uses

Company Decision Profiles

Weighted Scoring

Normalization

Business Priorities

Produces

Ordered Strategy List

No explanations are generated here.

---

## Delivery Strategy Explanation

Purpose

Explain recommendations.

Produces

Reasons

Trade-offs

Decision Metrics

Business Justifications

Rejected Alternatives

The MVP explanation engine remains deterministic.

Large Language Models are intentionally excluded.

---

# Company Decision Profiles

Every organization may prioritize different business objectives.

Default profiles

Profit Maximization

Delivery Speed

Operational Stability

AI Adoption

Human Resource Optimization

Balanced Strategy

Future releases may allow custom profiles.

Decision Profiles never modify calculations.

They modify only ranking priorities.

---

# Decision Layer Responsibilities

The Decision Layer never performs operational calculations.

Instead, it orchestrates analytical services produced by the Planning Layer.

Responsibilities

Generate

Evaluate

Rank

Explain

It never executes work.

Execution belongs to Operations.

---

# Operational Intelligence Flow

Commercial

↓

Operations

↓

Planning

↓

Decision

↓

Execution

↓

Continuous Monitoring

Every recommendation is generated before execution begins.

Recommendations may be regenerated whenever operational conditions change.

---

# Decision Intelligence Layer

Purpose

Transform AgencyOS from an operational decision platform into a Goal-Oriented Operational Planning Platform.

The Decision Intelligence Layer extends the MVP Decision Engine.

It does not replace it.

The deterministic Decision Engine remains the execution foundation.

Decision Intelligence adds strategic planning capabilities.
---

# Decision Intelligence Layer

The Decision Intelligence Layer represents the long-term strategic evolution of AgencyOS.

While the Decision Engine evaluates predefined execution alternatives, the Decision Intelligence Layer creates, optimizes and simulates operational strategies from business goals.

This layer introduces Artificial Intelligence without compromising deterministic business calculations.

The objective is to answer questions such as:

- How should this project be executed?
- Is there a better operational strategy?
- What happens if the deadline changes?
- What happens if another freelancer is hired?
- What happens if AI replaces part of the operation?
- Which strategy maximizes company objectives?

---

# Decision Intelligence Architecture

The Decision Intelligence Layer is composed of six independent engines.

Business Goal

↓

Constraint Engine

↓

Strategy Generation Engine

↓

Optimization Engine

↓

Ranking Engine

↓

Explainability Engine

↓

Simulation Engine

↓

Recommended Operational Strategy

Each engine has a single responsibility.

---

## Constraint Engine

Purpose

Represent every operational restriction as reusable business constraints.

Instead of embedding business rules throughout the application, constraints become independent components.

Typical Constraints

Budget

Deadline

Capacity

Skills

Customer Policies

Internal Policies

Required Equipment

Minimum Team Size

Maximum Outsourcing

Human Approval

Risk Threshold

Every strategy generated by the platform must satisfy all mandatory constraints.

Future releases may allow organizations to create custom constraints.

---

## Strategy Generation Engine

Purpose

Generate feasible operational strategies.

Unlike the MVP Decision Engine, which evaluates predefined alternatives, the Strategy Generation Engine creates alternatives automatically.

Inputs

Business Goals

Operational Constraints

Commercial Data

Capacity

Execution Resources

Planning Results

Outputs

Candidate Operational Strategies

A strategy may differ by:

- resource allocation
- execution sequence
- outsourcing level
- AI utilization
- schedule
- operational approach

There is no predefined limit on the number of generated strategies.

---

## Optimization Engine

Purpose

Identify the optimal strategy according to configurable business objectives.

Optimization Objectives

Lowest Cost

Shortest Delivery Time

Lowest Operational Risk

Highest Profit

Highest Capacity Utilization

Maximum AI Utilization

Minimum Human Effort

Balanced Strategy

Future versions may support multi-objective optimization.

Optimization never changes business constraints.

It searches only within feasible strategies.

---

## Ranking Engine

Purpose

Order optimized strategies.

The Ranking Engine extends the MVP implementation.

Future versions may consider:

Business Priorities

Historical Performance

Machine Learning Scores

Company Decision Profiles

Operational Preferences

Ranking remains deterministic.

Artificial Intelligence may assist ranking but never replace deterministic scoring.

---

## Explainability Engine

Purpose

Produce transparent operational recommendations.

AgencyOS shall never recommend a strategy without explaining:

Why it was selected.

Why competing strategies were rejected.

Which constraints influenced the decision.

Which trade-offs were accepted.

Which optimization objective was prioritized.

Every recommendation must be reproducible.

---

## Simulation Engine

Purpose

Support operational simulations.

The Simulation Engine evaluates hypothetical scenarios without modifying operational data.

Typical simulations

Increase budget

Reduce deadline

Hire another freelancer

Replace human resources with AI

Increase installed capacity

Reduce available staff

Introduce new operational policies

Modify customer priorities

Outputs

Operational Impact

Cost Impact

Capacity Impact

Delivery Impact

Recommended Actions

Simulation becomes one of the principal competitive differentiators of AgencyOS.

---

# Artificial Intelligence Strategy

AgencyOS adopts a Hybrid AI Architecture.

Artificial Intelligence complements deterministic business logic.

It never replaces it.

---

## Deterministic Components

Remain fully deterministic.

Includes

Commercial Rules

Capacity

Workload

Availability

Conflict Detection

Ranking

Business Constraints

Financial Calculations

Business Validation

These components always produce identical outputs for identical inputs.

---

## AI Components

Artificial Intelligence is responsible for exploratory reasoning.

Includes

Strategy Generation

Scenario Exploration

Optimization Assistance

Recommendation Refinement

Natural Language Interaction

Future Copilot Features

Artificial Intelligence operates within deterministic boundaries defined by business rules.

---

# Architecture Principles

AgencyOS follows these architectural principles.

Single Responsibility

Every service performs one business responsibility.

Layer Isolation

Dependencies always point toward lower layers.

Deterministic Core

Business calculations remain deterministic.

Explainability

Every recommendation must be explainable.

Auditability

Every recommendation must be reproducible.

Extensibility

New engines must be added without modifying existing engines.

Business First

Technology serves business objectives.

Goal-Oriented Planning

Planning starts from desired outcomes rather than available resources.

---

# Technical Architecture

AgencyOS follows a layered backend architecture.

Presentation

↓

Application

↓

Domain

↓

Infrastructure

↓

Database

Cross-cutting concerns are isolated in shared components.

The architecture supports independent evolution of business domains.

---

## Presentation Layer

Responsibilities

REST APIs

Authentication

Authorization

Request Validation

Swagger

Future Frontend Integration

---

## Application Layer

Responsibilities

Application Services

Use Cases

DTO Mapping

Transactions

Logging

Orchestration

Controllers invoke Application Services directly.

No MediatR.

No CQRS framework.

Business calculations are delegated to domain services.

---

## Domain Layer

Responsibilities

Business Entities

Business Rules

Value Objects

Domain Services

Repositories Interfaces

This layer contains no infrastructure dependencies.

---

## Infrastructure Layer

Responsibilities

Persistence

Repositories

External Services

Supabase Integration

Logging

Configuration

Messaging

Infrastructure never contains business rules.

---

## Shared Components

AgencyOS.Shared contains reusable components.

Examples

Common Exceptions

Base Classes

Utilities

Constants

Shared DTOs

Shared Interfaces

These components may be referenced by every layer without violating dependency rules.
---

# Database Architecture

AgencyOS uses PostgreSQL hosted on Supabase as the system of record.

The database is designed following a Database First approach.

Business entities are modeled before application services and user interfaces.

---

## Database Governance

Database evolution is managed exclusively through version-controlled SQL migrations.

Rules

- Every schema change must be implemented through a migration.
- Migrations are immutable after execution.
- Each migration represents a single logical change.
- Rollback procedures must be documented when applicable.

Entity Framework Core is used exclusively as the data access layer.

Entity Framework migrations are not permitted.

---

## Naming Standards

The following conventions apply to all database objects.

Tables

snake_case

Columns

snake_case

Primary Keys

UUID

Foreign Keys

*_id

Audit Fields

created_at

updated_at

archived_at (when the entity supports Archive)

soft_delete (technical maintenance flag only; never a business lifecycle field)

Status Fields

status

Business inactivation uses Archive according to ADR-011.

Physical deletion and deleted_at lifecycle fields are not used for Commercial business entities.

These conventions are mandatory across every business domain.

---

## Seed Data

Reference data initialization uses:

supabase/seed.sql

Seed data is deterministic.

Random identifiers are not permitted.

Closed enumerations defined by ADR-012 are not seeded.

Closed enumerations are implemented as C# enums and persisted as smallint columns.

No lookup tables are created for closed enumerations.

Transactional data is never seeded.

Business data is created exclusively through application APIs.

Optional non-enumeration reference data may be seeded only when explicitly approved by architecture and never for MVP Commercial closed enumerations.

---

## Audit Persistence

Audit history is centralized in AuditEvents according to ADR-009.

Audit records are written only after successful transaction commit.

GetHistory queries read exclusively from AuditEvents.

Event Sourcing is not adopted.

Aggregate state is never reconstructed from audit history.

---

## Aggregate Persistence Boundaries

According to ADR-010:

Commercial Aggregate Roots

- Lead
- Client
- Contract

Contact is persisted as a child of Client.

Mission belongs to Operations and is not owned by Commercial Aggregates.

Repositories exist only for Aggregate Roots.

---

# API Architecture

AgencyOS exposes all business capabilities through REST APIs.

Each business domain owns its own API surface.

The API layer is responsible for:

- Request validation
- Authentication
- Authorization
- DTO mapping
- Error handling
- API documentation

Business rules remain in the Domain Layer.

---

## Current API Domains

Commercial

- Leads
- Clients
- Contacts
- Contracts

Operations

- Missions
- Tasks
- Execution Resources
- Assignments

Planning

- Capacity
- Workload
- Availability
- Allocation Conflicts

Decision

- Delivery Strategy Builder
- Delivery Strategy Evaluation
- Delivery Strategy Ranking
- Delivery Strategy Explanation

Future

- Decision Intelligence
- Simulation
- Optimization
- Analytics

---

# Security Principles

AgencyOS follows Security by Design.

Core principles

- Authentication before authorization
- Least privilege
- Secure defaults
- Input validation
- Structured exception handling
- Audit logging
- API versioning
- Deterministic business processing

Future releases may include Row Level Security policies where appropriate.

---

# Observability

Every business service should support:

- Structured logging
- Correlation identifiers
- Performance metrics
- Error tracking
- Operational diagnostics

Business calculations should remain observable without exposing sensitive information.

---

# Program B — AgencyOS AI Factory

## Vision

AI executes.

Humans govern.

---

## Purpose

AgencyOS AI Factory is an independent engineering program responsible for accelerating software delivery through AI-assisted development.

It is not part of the AgencyOS product scope.

Its responsibility is to improve how AgencyOS is built.

---

## Engineering Principles

The AI Factory follows these principles.

Documentation First

Every implementation begins from approved documentation.

Small Iterations

Development is organized into small, independently verifiable work packages.

Human Governance

AI proposes.

Humans approve.

Single Responsibility

Each AI Agent performs one clearly defined responsibility.

Traceability

Every implementation can be traced back to its originating requirement.

---

## AI Agent Organization

Management

- Product Owner Agent
- Product Manager Agent
- Scrum Master Agent

Architecture

- Solution Architect Agent

Engineering

- Backend Agent
- Frontend Agent
- Database Agent
- DevOps Agent

Quality

- QA Agent
- Code Review Agent
- Documentation Agent

Future agent specializations may be introduced without changing the overall engineering model.

---

## Engineering Workflow

The standard implementation lifecycle is:

Business Requirement

↓

Architecture Review

↓

User Story / Work Package

↓

Implementation

↓

Code Review

↓

Testing

↓

Documentation Update

↓

Commit

↓

Sprint Validation

Documentation is updated as part of the Definition of Done.

---

## Documentation Governance

The consolidated documentation is the official source of truth.

Historical sprint documents remain available for traceability.

Audit documents preserve the evolution of the program.

Baseline documents define the current official architecture.

When conflicts exist, precedence is:

1. Approved Baseline
2. Architecture Decision Records
3. Architecture Documentation
4. Product Documentation
5. Technical Documentation
6. Sprint Documentation
7. Historical Audit Records

---

# Program Evolution Roadmap

AgencyOS evolution is organized into successive maturity stages.

Stage 1

Commercial Management

↓

Stage 2

Operational Management

↓

Stage 3

Operational Planning

↓

Stage 4

Operational Intelligence

↓

Stage 5

Decision Support

↓

Stage 6

Decision Intelligence

↓

Stage 7

Goal-Oriented Operational Planning

Future evolution may include autonomous planning assistance while preserving deterministic business governance.

---

# Cross-Program Relationship

| Aspect | AgencyOS Product | AgencyOS AI Factory |
|--------|------------------|---------------------|
| Primary Goal | Operational Planning Platform | AI-Assisted Engineering Platform |
| Scope | Product | Development Process |
| Deliverable | Customer Value | Engineering Productivity |
| Governance | Product Management | Technical Leadership |
| Repository | AgencyOS | AgencyOS AI Factory (future) |
| Dependency | Independent | Supports Product Delivery |

The two programs evolve independently while sharing architectural principles and governance standards.

---

# Architecture Summary

AgencyOS is built around three strategic concepts:

Business Domains

Represent operational reality.

Operational Intelligence

Measures operational feasibility through deterministic calculations.

Decision Intelligence

Generates, evaluates, optimizes and explains execution strategies based on business goals and operational constraints.

These concepts establish AgencyOS as a Goal-Oriented Operational Planning Platform rather than a traditional ERP, CRM or Project Management system.

---

# Official Baseline Statement

This document defines the official Program Architecture for AgencyOS Baseline 1.0.

It supersedes previous architectural summaries and consolidates the architectural decisions established throughout Phase 1, Sprints 0–10 and the subsequent program audit.

Future architectural evolution shall extend this baseline without violating its fundamental principles:

- Capacity First
- Goal-Oriented Planning
- Deterministic Core
- Explainable Decisions
- Layered Architecture
- Independent Business Domains
- AI-Assisted Engineering
- Human Governance

---
---

# Architecture Decisions

The following Architecture Decision Records (ADRs) are part of the frozen AgencyOS MVP 1.0 architecture and complement the architectural principles defined in this document.

## ADR-006 – AI Factory

Defines the AI Factory architecture responsible for orchestrating AI providers, prompts, execution flows, explainability, and future extensibility.

## ADR-007 – Company Decision Profiles

Defines Company Decision Profiles as the mechanism for representing organization-specific business preferences used by the Decision Engine.

## ADR-008 – Documentation Update Workflow

Defines the official documentation governance process, ensuring that Product, Architecture, Functional Specification, Engineering Specification, ADRs, Baseline and Decision Log remain synchronized throughout the project.

## ADR-009 – Audit and History Strategy

Defines the platform audit strategy.

Architecture decisions:

- Audit is centralized.
- Audit data is stored in the AuditEvents repository.
- Domain Events are business notifications.
- Audit Events are persistence records.
- Event Sourcing is not adopted.
- Audit Events are generated only after successful transaction commit.
- Historical queries are executed exclusively against the centralized audit repository.

This strategy guarantees complete traceability while preserving a conventional transactional architecture.

## ADR-010 – Aggregate Boundary Strategy

Defines the Aggregate Root boundaries for the AgencyOS domain model.

Commercial Domain

Aggregate Roots

- Lead
- Client
- Contract

Child Entities

- Contact (owned by Client)

Operations Domain

Aggregate Roots

- Mission
- Task

Repositories exist only for Aggregate Roots.

Transactional consistency is guaranteed inside each Aggregate boundary.

Relationships between Aggregates occur exclusively through identity references.

This decision enforces Domain-Driven Design aggregate consistency.

## ADR-011 – Archive and Soft Delete Policy

Defines the persistence lifecycle strategy.

Business Archive

- Status = Archived
- ArchivedAt populated
- Entity preserved for history
- Entity remains queryable according to authorization rules

Soft Delete

- Technical operation only
- Reserved for exceptional maintenance scenarios
- Not used by business workflows
- Not exposed through public APIs

Business operations shall archive entities instead of deleting them.

## ADR-012 – Enumeration Persistence Strategy

Defines enumeration persistence across the platform.

Enumerations are implemented as:

- C# Enums
- Entity Framework Core Enum Conversion
- Database smallint columns

The platform does not create lookup tables for enumerations.

No enumeration seed data is required.

APIs expose enumeration names while persistence stores integer values.

This strategy minimizes complexity while maintaining strong typing and forward compatibility.

---

# Architectural Principles Reinforced

The following principles are considered mandatory for all MVP implementations.

- Clean Architecture
- Domain-Driven Design
- SOLID Principles
- Aggregate consistency
- Explicit transactional boundaries
- Immutable Domain Events
- Centralized Audit
- Business Archive instead of physical deletion
- Strongly typed enumerations
- Repository per Aggregate Root
- No Event Sourcing
- Architecture-first implementation
- Documentation-driven development

These principles are mandatory for every Engineering Specification and every implementation generated during AgencyOS MVP 1.0.
Version: 2.0

Status: Official Baseline

Owner: AgencyOS Program

Last Review: 2026-07