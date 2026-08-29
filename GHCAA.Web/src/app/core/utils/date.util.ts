import { DATE_REGEX } from '../constants/app.constants';

/** ISO/Date/any parseable value -> 'dd-MM-yyyy' for display. '' for empty; raw string if unparseable. */
export function toDisplayDate(value: any): string {
  if (!value) return '';
  const date = new Date(value);
  if (isNaN(date.getTime())) return typeof value === 'string' ? value : '';
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  return `${day}-${month}-${date.getFullYear()}`;
}

/** dd-MM-yyyy user input (or Date/ISO) -> 'yyyy-MM-dd' ISO date for the API wire. null for empty.
 *  Uses a plain string swap for dd-MM-yyyy so there is NO timezone shift on date-only fields. */
export function toWireDate(value: any): string | null {
  if (value === null || value === undefined || value === '') return null;
  if (value instanceof Date) {
    if (isNaN(value.getTime())) return null;
    const y = value.getFullYear();
    const mo = String(value.getMonth() + 1).padStart(2, '0');
    const da = String(value.getDate()).padStart(2, '0');
    return `${y}-${mo}-${da}`;
  }
  const str = String(value).trim();
  if (DATE_REGEX.test(str)) {            // dd-MM-yyyy -> yyyy-MM-dd, no Date/TZ involved
    const [dd, mm, yyyy] = str.split('-');
    return `${yyyy}-${mm}-${dd}`;
  }
  const d = new Date(str);               // already ISO or other parseable
  if (isNaN(d.getTime())) return str;
  const y = d.getFullYear();
  const mo = String(d.getMonth() + 1).padStart(2, '0');
  const da = String(d.getDate()).padStart(2, '0');
  return `${y}-${mo}-${da}`;
}

/** An EC (or any) period's start/end -> "YYYY" (no end date) or "YYYY - YYYY". Always the real
 *  stored years — no "Present"/"ongoing" label, regardless of the isActive flag. */
export function formatPeriodRange(period: { startDate: string | Date; endDate?: string | Date | null; isActive: boolean }): string {
  const startYear = new Date(period.startDate).getFullYear();
  if (!period.endDate) return `${startYear}`;
  const endYear = new Date(period.endDate).getFullYear();
  return startYear === endYear ? `${startYear}` : `${startYear} - ${endYear}`;
}

/** dd-MM-yyyy (or ISO) -> Date for validation/comparison. null if unparseable. */
export function parseDisplayDate(value: any): Date | null {
  if (!value) return null;
  const str = String(value).trim();
  if (DATE_REGEX.test(str)) {
    const [dd, mm, yyyy] = str.split('-').map(Number);
    const d = new Date(yyyy, mm - 1, dd);
    return isNaN(d.getTime()) ? null : d;
  }
  const d = new Date(str);
  return isNaN(d.getTime()) ? null : d;
}
