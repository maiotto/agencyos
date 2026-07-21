# AgencyOS Program Architecture

Version: 1.1

Status: Active

Date: 2026-07-21

---

# Overview

AgencyOS is organized as two independent strategic programs that share vision but evolve with separate delivery dependencies.

AgencyOS Program

│

├── Program A

│     AgencyOS Product

│

└── Program B

      AgencyOS AI Factory

---

# Program A – AgencyOS Product

## Vision

AI-First Operational Decision Platform for service organizations.

## Mission

Transform sold contracts into the best execution strategy.

## Core Business Flow

Lead

↓

Client

↓

Client Contract

↓

Delivery Strategy

↓

Mission

↓

Tasks

↓

Execution Resources

↓

Capacity Planning

↓

AI Recommendations

↓

Manager Decision

↓

Execution

↓

Continuous Replanning

---

## Layered Architecture

AgencyOS.Api

↓

AgencyOS.Application

↓

AgencyOS.Domain

AgencyOS.Infrastructure

AgencyOS.Shared

Dependencies always point inward.

The Domain layer never depends on Infrastructure.

---

## Domain Modules

### Commercial Domain

Manages the commercial lifecycle from opportunity to contract.

Entities: Lead, Client, ClientContact, ClientContract, Mission

### Operational Domain

Transforms contracts into executable work.

Entities: Task, ExecutionResource, Assignment

### Analytical Domain

Calculates operational intelligence from execution data.

Services:

- CapacityCalculatorService
- WorkloadCalculatorService
- AvailabilityEngineService
- AllocationConflictDetectionService

Calculation chain:

Execution Data → Capacity → Workload → Availability → Conflict Detection

### Decision Domain

Generates, evaluates, ranks and explains delivery strategies.

Services:

- DeliveryStrategyBuilderService
- DeliveryStrategyEvaluatorService
- DeliveryStrategyRankingService
- DeliveryStrategyExplanationService

Decision pipeline:

Contract + Mission + Tasks → Builder → Evaluator → Ranking → Explanation

---

## Decision Engine Architecture

The Decision Engine is the core differentiator of AgencyOS.

It does not make final decisions. It generates alternatives, measures them, ranks them and explains trade-offs.

### Stage 1 – Strategy Builder

Generates all valid execution strategy candidates from active Execution Resources and applicable policies.

Does not evaluate or rank.

### Stage 2 – Strategy Evaluator

Calculates objective operational metrics for each strategy using Capacity, Workload, Availability and Conflict Detection engines.

Does not rank or recommend.

### Stage 3 – Strategy Ranking

Orders evaluated strategies using configurable Company Decision Profiles.

Applies min-max normalization and weighted scoring.

Does not generate explanations.

### Stage 4 – Strategy Explanation

Produces structured, deterministic explanations of ranking results.

Uses reason codes and evaluation metrics.

Does not use LLM generation during the MVP.

---

## Company Decision Profiles

Ranking priorities are defined by Company Decision Profiles.

During the MVP, profiles are stored in application configuration and loaded through `ICompanyDecisionProfileRepository`.

See ADR-007.

---

## API Surface

### Commercial and Operational

| Route | Purpose |
|-------|---------|
| missions | Mission management |
| leads | Lead management |
| clients | Client management |
| contacts | Client contact management |
| contracts | Client contract management |
| tasks | Task management |
| execution-resources | Execution resource management |
| assignments | Resource assignment management |

### Analytical Engines

| Route | Purpose |
|-------|---------|
| capacity | Capacity calculation |
| workload | Workload calculation |
| availability | Availability calculation |
| allocation-conflicts | Allocation conflict detection |

### Decision Engine

| Route | Method | Purpose |
|-------|--------|---------|
| delivery-strategies/build | POST | Generate strategy candidates |
| delivery-strategies/{contractId} | GET | Retrieve strategies for a contract |
| delivery-strategies/evaluate | POST | Evaluate strategies with metrics |
| delivery-strategies/rank | POST | Rank strategies by decision profile |
| delivery-strategies/{strategyId}/explanation | GET | Explain a ranked strategy |

---

## Calculation Pattern

Pure business calculations are implemented as static classes in the Application layer.

Services orchestrate data access, logging and validation.

Examples:

- CapacityCalculation
- WorkloadCalculation
- AvailabilityCalculation
- AllocationConflictCalculation
- DeliveryStrategyGeneration
- DeliveryStrategyEvaluationCalculation
- DeliveryStrategyRankingCalculation
- DeliveryStrategyExplanationCalculation

This pattern ensures deterministic, testable logic without infrastructure dependencies.

---

## Database Governance

Schema is managed exclusively through Supabase SQL migrations.

Entity Framework Core is used for data access only.

Entity Framework migrations are not permitted.

---

# Program B – AgencyOS AI Factory

## Vision

AI executes. Humans govern.

## Purpose

Independent engineering platform that accelerates AgencyOS delivery through AI-assisted development workflows.

AgencyOS AI Factory is not part of the AgencyOS MVP product scope.

See ADR-006.

---

## Agent Organization

### Management

- Product Owner Agent
- Scrum Master Agent
- Tech Lead Agent

### Architecture

- Solution Architect Agent

### Engineering

- Backend Agent
- Frontend Agent
- Database Agent
- DevOps Agent

### Quality

- QA Agent
- Code Review Agent
- Documentation Agent

### Knowledge Base

System prompts, execution guides, user story prompts and review checklists in `/prompts`.

---

## Engineering Workflow

### User Story Implementation

Backend Agent executes individual User Stories following:

- prompts/system/00_Project_Context.md
- prompts/system/01_Engineering_Principles.md
- prompts/system/02_Coding_Standards.md
- prompts/system/03_Definition_of_Done.md
- prompts/system/04_Tech_Stack.md
- prompts/system/05_Agent_Execution_Guide.md

Per-story prompts:

- US-XXX (requirements)
- EXEC-US-XXX (execution instructions)
- REVIEW-US-XXX (review checklist)

### Sprint Documentation Consolidation

After sprint completion, Documentation Agent executes the Documentation Update workflow.

See ADR-008 and prompts/system/06_Documentation_Update_Guide.md.

This workflow updates Sprint Register, Decision Log, Baseline, Architecture documents and ADRs without modifying source code.

---

## Governance Boundary

Until the AgencyOS MVP is delivered, the AI Factory shall not require changes to:

- Product Architecture
- Domain Model
- Product Roadmap
- Product Backlog
- Product Schedule

AgencyOS AI Factory accelerates AgencyOS.

AgencyOS never slows down to accommodate AI Factory.

---

## Evolution Roadmap

Phase 0 – Knowledge

Phase 1 – Product Owner Automation

Phase 2 – Scrum Master Automation

Phase 3 – Architecture Automation

Phase 4 – Engineering Automation

Phase 5 – Complete AI Factory Orchestration

---

# Cross-Program Relationship

| Aspect | Program A (Product) | Program B (AI Factory) |
|--------|---------------------|------------------------|
| Deliverable | Operational Decision Platform | Engineering Automation Platform |
| MVP Scope | Yes | No |
| Repository | AgencyOS | AgencyOS-AI-Factory (future) |
| Governance | Product Owner | Tech Lead |
| Documentation | docs/ | prompts/ |

Both programs share the AgencyOS vision.

Delivery dependencies remain independent.
