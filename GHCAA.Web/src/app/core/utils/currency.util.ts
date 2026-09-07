import { formatNumber } from '@angular/common';
import { OrgCurrency } from '../models/org-config.model';

/**
 * Formats a number against the active OrgConfig currency instead of a hardcoded symbol/code.
 * Shared by AppCurrencyPipe (template use) and any TS-side formatting that can't use a pipe.
 * currency can be null/undefined while OrgConfig hasn't loaded yet — falls back to the bare
 * number rather than guessing a symbol.
 */
export function formatCurrencyAmount(
  value: number | null | undefined,
  currency: OrgCurrency | null | undefined,
  digitsInfo = '1.0-0',
  display: 'symbol' | 'code' = 'symbol'
): string {
  if (value === null || value === undefined || isNaN(value)) return '';

  const amount = formatNumber(value, 'en-US', digitsInfo);
  const label = display === 'code'
    ? (currency?.code ? `${currency.code} ` : '')
    : (currency?.symbol ?? '');

  return `${label}${amount}`;
}
