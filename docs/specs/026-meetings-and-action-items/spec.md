# Meetings and action items

**Feature Branch**: `026-meetings-and-action-items`  
**Status**: Draft for review before implementation  
**Tracker**: 84.19 and 84.20 in `docs/TODO.md`  
**Depends on**: 84.8 platform baseline and 84.18 entity feature-gap review; spec 014 event invite and RSVP patterns.

## Purpose and scope

EC governance needs a place to manage meetings and archive their minutes. The governance page already tells members that minutes are archived, but no meeting or minutes entity exists. This specification covers meeting records, invitees and RSVP, ordered agenda, minutes, and meeting action items. Admin and EC roles manage the workflow through Web and Mobile.

Tasks and notes belong to a meeting action item or EC work item, not to a general task manager. Standalone notes are out of scope unless the 84.18 review establishes a distinct user need. Existing event registration and invitation behavior should be reused where it fits.

## Requirements

- Authorized Admin and EC roles can create and manage meeting records.
- A meeting records its schedule and state, invitees, RSVP responses, an ordered agenda, and minutes.
- Minutes are retained and exposed only to audiences authorized by the association's governance rules.
- Action items are linked to a meeting or an EC work item and carry an owner, due date, and status.
- The system does not create a general-purpose task manager or standalone notes module.
- Web and Mobile support the meeting-management actions assigned to their roles; the API enforces authorization.
- Existing event invite and RSVP behavior is reused where its contracts match the meeting workflow.
- Data changes are auditable using existing audit conventions. API changes are recorded in `docs/API_CONTRACT_REGISTRY.md`.

## Decisions requiring the 84.18 review

The 84.18 gap table must confirm the users, visibility rules for minutes, and whether action items are meeting-only or also usable as EC work items. The tracker already rules out standalone notes and a general task manager unless the review finds a real user need. Record those outcomes here before implementation.

## Acceptance

1. An authorized Admin or EC role can create a meeting and manage its invitees, agenda, minutes, and action items.
2. Invitees can record an RSVP through the selected Web/Mobile surfaces.
3. Unauthorized users cannot read restricted minutes or mutate meeting records.
4. Action items retain their meeting/EC-work context, owner, due date, and status.
5. Tests cover authorization, RSVP transitions, agenda ordering, minute visibility, and action-item lifecycle.
6. Persistence changes apply to SQLite and PostgreSQL; API and client contracts are documented.

## Task breakdown

See [tasks.md](./tasks.md). Tracker status and acceptance remain in `docs/TODO.md`.
