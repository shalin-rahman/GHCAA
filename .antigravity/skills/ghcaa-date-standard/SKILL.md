---
name: ghcaa-date-standard
description: Use this skill whenever you are working with dates, inputs, or API formatting in any part of the GHCAA ecosystem.
---

# GHCAA Date Standardization

**Two formats, two roles — never mix them:**
- **Wire format = ISO-8601** (`yyyy-MM-dd` for date-only, full ISO for timestamps). This is what crosses the API boundary in BOTH directions.
- **Display/input format = `dd-MM-yyyy`.** This is what users see and type. It NEVER goes on the wire.

The rule: convert `dd-MM-yyyy` → ISO the moment you build an outgoing request body; convert ISO → `dd-MM-yyyy` the moment you render or populate an input. (Superseded the previous all-layers-`dd-mm-yyyy` standard, which put an unparseable format on the wire and broke `new Date(...)` / `DateTime.parse(...)`.)

## 1. API Standards (C#)
- `DateFormatConverter` (registered globally in `System.Text.Json`) **WRITES ISO-8601** (`yyyy-MM-ddTHH:mm:ss.fff`) and **READS both** ISO-8601 (primary) and legacy `dd-MM-yyyy` (exact-match fallback — unambiguous, ISO's 4-digit year never matches the `dd` slot).
- Models use plain `DateTime`; the serializer handles the string conversion. No per-property attributes needed.
- **Reference**: `GHCAA.API/Utils/DateFormatConverter.cs`.

## 2. Web Standards (Angular)
- **Display**: `| date:'dd-MM-yyyy'` (DatePipe parses ISO natively) or `toDisplayDate(value)`.
- **Input**: `type="text"`, `placeholder="dd-mm-yyyy"`, `pattern="^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-\d{4}$"` (constant `DATE_REGEX`).
- **SENDING to the API**: ALWAYS wrap date fields in `toWireDate(value)` when building the request body — this converts `dd-MM-yyyy` → `yyyy-MM-dd` via a string swap (no timezone shift on date-only fields). Never send a raw `dd-MM-yyyy` string or `new Date(...).toISOString()` for a date-only value (the latter shifts the day across timezones).
- For validation/comparison of a `dd-MM-yyyy` input, use `parseDisplayDate(value)` (NOT `new Date('dd-MM-yyyy')`, which is `Invalid Date`).
- **Reference**: `src/app/core/utils/date.util.ts` (`toDisplayDate` / `toWireDate` / `parseDisplayDate`), `src/app/core/constants/app.constants.ts` (`DATE_FORMAT` / `DATE_REGEX`).

## 3. Mobile Standards (Flutter)
- **Display**: `AppUtils.formatDate(value)` → renders `dd-MM-yyyy` (parses ISO first, then `dd-MM-yyyy` fallback).
- **Parse**: `AppUtils.parseDate(String)` (ISO-first).
- **SENDING to the API**: ALWAYS use `AppUtils.toWire(value)` → returns `yyyy-MM-dd`. Never put `AppUtils.formatDate(...)` (which is `dd-MM-yyyy`) or a raw typed string into an outgoing request body.
- Input fields use a `showDatePicker` or `TextInputType.datetime` with a `dd-MM-yyyy` hint.
- **Reference**: `lib/core/utils/app_utils.dart` (`formatDate` / `parseDate` / `toWire`).

## 4. Testing Standards (Playwright)
- Test data typed into `dd-MM-yyyy` INPUT fields uses `"DD-MM-YYYY"`. **Example**: `await page.fill('#dob', '15-05-1990');`
- Assertions against API responses expect ISO-8601.

## Decision Tree
- **Display / populating an input?** ISO → `dd-MM-yyyy` via `toDisplayDate` (web) / `AppUtils.formatDate` (mobile) / DatePipe `'dd-MM-yyyy'`.
- **Building an outgoing API request?** `dd-MM-yyyy` → ISO via `toWireDate` (web) / `AppUtils.toWire` (mobile). This is the step most often forgotten.
- **Validating/comparing a typed date?** `parseDisplayDate` (web) / `AppUtils.parseDate` (mobile). Never `new Date('dd-MM-yyyy')`.
- **Reading an API date?** It is ISO — `new Date(x)` / `DateTime.parse(x)` work directly.
