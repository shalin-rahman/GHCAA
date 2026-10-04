import { Pipe, PipeTransform } from '@angular/core';

/** Spaces out a stored name such as "ElectionOfficial" or "superAdmin" for display. */
export function humanize(value: string | null | undefined): string {
  if (!value) return '';
  return value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')
    .replace(/^./, c => c.toUpperCase());
}

@Pipe({
  name: 'humanize',
  standalone: true
})
export class HumanizePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return humanize(value);
  }
}
