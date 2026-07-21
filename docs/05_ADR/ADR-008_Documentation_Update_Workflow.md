# ADR-008

Title

Documentation Update Workflow

Status

Accepted

Date

2026-07-21

---

## Context

AgencyOS development is executed through AI-assisted User Story implementation across multiple sprints.

Each sprint delivers multiple User Stories that introduce services, APIs, architectural patterns and engineering decisions.

Implementation agents focus on source code delivery.

Without a governed documentation step, project documentation, ADRs, sprint records and agent guides drift from the implemented architecture.

Sprint 8 completed the Decision Engine (US-014 through US-017) and required consolidated documentation updates across multiple project artifacts.

---

## Decision

A Documentation Update workflow is established as a separate governed step executed after sprint completion.

The workflow is executed by the Documentation Agent, not the Backend Agent.

The workflow is defined in prompts/system/06_Documentation_Update_Guide.md.

The workflow updates only documentation files. Source code must not be modified.

The workflow must not change approved architecture decisions, backlog items or roadmap scope.

Required outputs after each documentation consolidation:

- Sprint Register updated with sprint completion
- Decision Log updated with sprint architectural decisions
- AgencyOS Baseline updated to reflect current architecture
- Program Architecture updated when architecture evolves
- ADRs created or updated when new decisions are formalized
- Agent Execution Guide updated when process changes

---

## Workflow Trigger

Execute after all User Stories in a sprint reach Definition of Done.

Execute when explicitly requested by the Product Owner or Tech Lead.

---

## Workflow Scope

### In Scope

- docs/07_Project_Management/Sprint_Register.md
- docs/07_Project_Management/Decision_Log.md
- docs/07_Project_Management/AgencyOS_Baseline_v1.0.md
- docs/02_Architecture/Program_Architecture.md
- docs/05_ADR/
- prompts/system/05_Agent_Execution_Guide.md
- prompts/system/06_Documentation_Update_Guide.md

### Out of Scope

- Source code
- Database migrations
- Backlog modifications
- Roadmap modifications (except reflecting approved decisions already taken)
- Architecture decision changes

---

## Agent Responsibilities

### Backend Agent

- Implements User Stories
- Updates Swagger and HTTP tests
- Does not perform sprint-level documentation consolidation

### Documentation Agent

- Analyzes completed User Story implementations
- Consolidates outcomes into project documentation
- Creates ADRs for formalized decisions
- Does not modify source code

---

## Alternatives Considered

### Documentation Updated Per User Story

Rejected because it fragments sprint-level architectural narrative and increases agent context usage across every story.

### Manual Documentation Only

Rejected because AI-assisted development requires synchronized agent guides and architecture references to maintain implementation consistency.

### Backend Agent Updates Documentation

Rejected because mixing implementation and consolidation increases scope creep risk and reduces documentation quality.

---

## Consequences

Positive

- Documentation remains aligned with implemented architecture.
- Sprint outcomes are recorded consistently.
- Agent guides reflect current engineering process.
- Backend Agents maintain focused implementation scope.

Negative

- Requires an additional agent execution step after each sprint.
- Documentation consolidation depends on accurate analysis of completed implementations.

---

## Related Decisions

- DEC-008-007 in Decision Log
- ADR-006 AgencyOS AI Factory
- ADR-007 Company Decision Profiles

---

## References

- prompts/system/06_Documentation_Update_Guide.md
- prompts/system/05_Agent_Execution_Guide.md
- docs/07_Project_Management/Sprint_Register.md
