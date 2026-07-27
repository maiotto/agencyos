# US-102 — Manage Holidays

Release: 1.1

Status: Implemented

---

## Objective

Allow Administrators to manage Holidays as the official source of non-working days used with Working Calendars for Planning.

---

## Holiday Types

- National
- State
- Municipal
- Company

---

## Business Rules

BR-201..BR-213 (see work package).

---

## API

Base route: `/holidays`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/holidays` | List / search |
| GET | `/holidays/filter` | Explicit filter endpoint |
| GET | `/holidays/{id}` | Get by id |
| POST | `/holidays` | Create (Inactive) |
| PUT | `/holidays/{id}` | Update |
| POST | `/holidays/{id}/activate` | Activate |
| POST | `/holidays/{id}/deactivate` | Deactivate |
| DELETE | `/holidays/{id}` | Delete inactive |

### Working Calendar integration

| Method | Path | Description |
|--------|------|-------------|
| GET | `/working-calendars/operational-day` | Calendar weekday + active holidays |
| GET | `/working-calendars/{id}/holidays` | Active holidays in calendar range |

---

## Notes

- Capacity Engine is not modified in this story.
- Only Active holidays affect Planning (BR-207).
