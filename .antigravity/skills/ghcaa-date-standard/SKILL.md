---
name: ghcaa-date-standard
description: Use this skill whenever you are working with dates, inputs, or API formatting in any part of the GHCAA ecosystem.
---

# GHCAA Date Standardization (dd-mm-yyyy)

Enforces the strict `dd-mm-yyyy` date format standard across all platforms to prevent ISO/browser-specific formatting issues.

## 1. API Standards (C#)
- Use `DateFormatConverter` in `System.Text.Json` settings for all `DateTime` properties.
- Models should accept/return `DateTime`, but the JSON serializer handles the string conversion.
- **Reference**: `GHCAA.API/Utils/DateFormatConverter.cs`.

## 2. Web Standards (Angular)
- Use `type="text"` for date inputs instead of `type="date"`.
- Implement pattern validation: `pattern="^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-\d{4}$"`.
- Use a placeholder: `placeholder="dd-mm-yyyy"`.
- **Reference**: `src/app/core/constants/app.constants.ts`.

## 3. Mobile Standards (Flutter)
- Use `AppUtils.parseDate(String)` and `AppUtils.formatDate(DateTime)` for all UI-to-Model conversions.
- Input fields should use `TextInputType.datetime` with a custom formatter or hint.
- **Reference**: `lib/core/utils/app_utils.dart`.

## 4. Testing Standards (Playwright)
- All test data in `.spec.ts` files must use the string format `"DD-MM-YYYY"`.
- **Example**: `await page.fill('#dob', '15-05-1990');`

## Decision Tree
- **Is the date for display?** Use `dd-mm-yyyy` pipe or helper.
- **Is the date for an input?** Use `type="text"` with pattern `\d{2}-\d{2}-\d{4}`.
- **Is the date from the API?** Ensure it was parsed using the standard converter.
