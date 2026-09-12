import {
  DATE_FORMATS,
  DATE_FORMAT_REGEX,
  DEFAULT_DATE_FORMAT,
  DateFormat,
  normalizeDateFormat
} from '../constants/app.constants';

/** ISO/Date/any parseable value -> the configured display format. */
export function toDisplayDate(value: any, format: DateFormat = DEFAULT_DATE_FORMAT): string {
  if (!value) return '';
  const date = parseDateValue(value);
  if (isNaN(date.getTime())) return typeof value === 'string' ? value : '';
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  return normalizeDateFormat(format) === DATE_FORMATS.MDY
    ? `${month}/${day}/${year}`
    : `${day}-${month}-${year}`;
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
  const format = detectDisplayFormat(str);
  if (format) {
    const parts = format === DATE_FORMATS.DMY ? str.split('-') : str.split('/');
    const [first, second, yyyy] = parts;
    const dd = format === DATE_FORMATS.DMY ? first : second;
    const mm = format === DATE_FORMATS.DMY ? second : first;
    return `${yyyy}-${mm}-${dd}`;
  }
  const d = parseDateValue(str);         // already ISO or another parseable value
  if (isNaN(d.getTime())) return null;
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

/** An event's lifecycle state computed from its own dates, independent of the admin's manual
 *  IsActive publish flag (which only controls whether it's hidden). 'Unpublished' always wins —
 *  an admin who's hidden an event doesn't want it showing as Upcoming/Ongoing/Ended anywhere. */
export type EventLifecycleStatus = 'Unpublished' | 'Upcoming' | 'Ongoing' | 'Ended';

export function getEventStatus(event: { isActive: boolean; startDate: string | Date; endDate: string | Date }): EventLifecycleStatus {
  if (!event.isActive) return 'Unpublished';
  const now = new Date();
  const start = new Date(event.startDate);
  const end = new Date(event.endDate);
  if (now < start) return 'Upcoming';
  if (now > end) return 'Ended';
  return 'Ongoing';
}

/** Label + `.status-badge` state class (styles.scss) for getEventStatus's result. */
export function getEventStatusMeta(event: { isActive: boolean; startDate: string | Date; endDate: string | Date }): { label: string; class: string } {
  switch (getEventStatus(event)) {
    case 'Unpublished': return { label: 'Unpublished', class: 'inactive' };
    case 'Upcoming': return { label: 'Upcoming', class: 'pending' };
    case 'Ongoing': return { label: 'Ongoing', class: 'active' };
    case 'Ended': return { label: 'Ended', class: 'terminated' };
  }
}

/** dd-MM-yyyy (or ISO) -> Date for validation/comparison. null if unparseable. */
export function parseDisplayDate(value: any, format?: DateFormat): Date | null {
  if (!value) return null;
  const str = String(value).trim();
  const detectedFormat = format ? normalizeDateFormat(format) : detectDisplayFormat(str);
  if (detectedFormat && DATE_FORMAT_REGEX[detectedFormat].test(str)) {
    const parts = detectedFormat === DATE_FORMATS.DMY ? str.split('-') : str.split('/');
    const dd = Number(detectedFormat === DATE_FORMATS.DMY ? parts[0] : parts[1]);
    const mm = Number(detectedFormat === DATE_FORMATS.DMY ? parts[1] : parts[0]);
    const yyyy = Number(parts[2]);
    const d = new Date(yyyy, mm - 1, dd);
    return d.getFullYear() === yyyy && d.getMonth() === mm - 1 && d.getDate() === dd ? d : null;
  }
  const d = parseDateValue(str);
  return isNaN(d.getTime()) ? null : d;
}

function detectDisplayFormat(value: string): DateFormat | null {
  if (DATE_FORMAT_REGEX[DATE_FORMATS.DMY].test(value)) return DATE_FORMATS.DMY;
  if (DATE_FORMAT_REGEX[DATE_FORMATS.MDY].test(value)) return DATE_FORMATS.MDY;
  return null;
}

function parseDateValue(value: any): Date {
  if (value instanceof Date) return value;
  const str = String(value).trim();
  const isoDate = /^(\d{4})-(\d{2})-(\d{2})(?:$|T)/.exec(str);
  if (isoDate) return new Date(Number(isoDate[1]), Number(isoDate[2]) - 1, Number(isoDate[3]));
  return new Date(str);
}
