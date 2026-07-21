# US-004 – Client Contact Management

Sprint: 5 – Commercial Foundation

Status: Ready for Implementation

Priority: High

---

# Objective

Implement the complete Client Contact Management module.

Contacts represent people associated with Clients and are used throughout the commercial and operational lifecycle.

A Client may have multiple Contacts.

---

# Business Value

Client Contacts are responsible for communication during the commercial process and project execution.

Future AI workflows may use Contact information for notifications, approvals and collaboration.

---

# User Story

As a Commercial Manager,

I want to register and manage Client Contacts,

So that every Client has one or more identified business contacts.

---

# Acceptance Criteria

## Create Contact

The system shall allow creating a Contact containing:

- Client
- First Name
- Last Name
- Job Title
- Department
- Email
- Phone
- Mobile
- Preferred Contact Method
- Is Primary Contact
- Status
- Notes

---

## Update Contact

Contact information can be updated.

---

## List Contacts

Return paginated list.

Support filtering by:

- Client
- Status
- Department
- Primary Contact

Support ordering by:

- First Name
- Last Name
- Client

---

## View Contact

Return complete Contact information.

---

## Deactivate Contact

Contact becomes Inactive.

Historical information must be preserved.

---

## Primary Contact

Only one Primary Contact is allowed per Client.

If another Contact becomes Primary, the previous Primary Contact must automatically lose this designation.

---

# Business Rules

BR-001

Every Contact must belong to one Client.

BR-002

Email is mandatory.

BR-003

Email must be unique within the same Client.

BR-004

Only one Primary Contact is allowed per Client.

BR-005

Inactive Contacts cannot become Primary.

BR-006

Physical deletion is not allowed.

---

# Contact Status

- Active
- Inactive

---

# Preferred Contact Method

- Email
- Phone
- Mobile
- WhatsApp

---

# Required API Endpoints

GET /contacts

GET /contacts/{id}

POST /contacts

PUT /contacts/{id}

DELETE /contacts/{id}

PATCH /contacts/{id}/primary

---

# Expected Layers

AgencyOS.Domain

- ClientContact Entity

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

- Client
- First Name
- Email
- Status

Optional

- Last Name
- Job Title
- Department
- Phone
- Mobile
- Notes

---

# Logging

Generate logs for

- Create
- Update
- Deactivate
- Primary Contact Changed

---

# Error Handling

404

Contact not found

400

Validation errors

409

Duplicate Email

500

Unexpected error

---

# Out of Scope

Notifications

Email integration

Calendar integration

AI workflows

Decision Engine

---

# Definition of Done

✔ CRUD complete

✔ Swagger updated

✔ HTTP file updated

✔ Validation implemented

✔ Logging implemented

✔ Build without warnings

✔ Build without errors

✔ Review approved