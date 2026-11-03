import { Pipe, PipeTransform } from '@angular/core';

export interface BatchSource {
  batchLabel?: string | null;
  passingYear?: number | string | null;
  ghcLastCertificatePassingYear?: number | string | null;
}

/**
 * The server builds batchLabel from every college record, earliest first. Older API builds and
 * members without a college record send no label, so fall back to the single year and then to
 * the given placeholder. A year of 0 or a blank string counts as missing.
 */
export function batchText(member: BatchSource | null | undefined, fallback = 'N/A'): string {
  if (!member) return fallback;
  const label = typeof member.batchLabel === 'string' ? member.batchLabel.trim() : '';
  if (label) return label;
  for (const year of [member.passingYear, member.ghcLastCertificatePassingYear]) {
    const text = year == null ? '' : String(year).trim();
    if (text && text !== '0') return text;
  }
  return fallback;
}

/** True when the label already carries degree and subject, so the screen should not repeat them. */
export function hasBatchLabel(member: BatchSource | null | undefined): boolean {
  return typeof member?.batchLabel === 'string' && member.batchLabel.trim().length > 0;
}

@Pipe({
  name: 'batch',
  standalone: true
})
export class BatchPipe implements PipeTransform {
  transform(member: BatchSource | null | undefined, fallback = 'N/A'): string {
    return batchText(member, fallback);
  }
}
