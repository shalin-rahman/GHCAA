import { describe, it, expect } from 'vitest';
import { formatCurrencyAmount } from './currency.util';

describe('formatCurrencyAmount', () => {
  const bdt = { code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka' };
  const usd = { code: 'USD', symbol: '$', name: 'US Dollar' };

  it('prefixes the amount with the config symbol by default', () => {
    expect(formatCurrencyAmount(1234, bdt)).toBe('৳1,234');
  });

  it('switches symbol with the currency when a different profile is active', () => {
    expect(formatCurrencyAmount(1234, usd)).toBe('$1,234');
  });

  it('shows the currency code with a trailing space when display is "code"', () => {
    expect(formatCurrencyAmount(1234, bdt, '1.0-0', 'code')).toBe('BDT 1,234');
  });

  it('respects a custom digitsInfo for decimals', () => {
    expect(formatCurrencyAmount(1234.5, bdt, '1.2-2')).toBe('৳1,234.50');
  });

  it('falls back to the bare number when currency has not loaded yet', () => {
    expect(formatCurrencyAmount(1234, null)).toBe('1,234');
    expect(formatCurrencyAmount(1234, undefined, '1.0-0', 'code')).toBe('1,234');
  });

  it('returns an empty string for null/undefined/NaN values', () => {
    expect(formatCurrencyAmount(null, bdt)).toBe('');
    expect(formatCurrencyAmount(undefined, bdt)).toBe('');
    expect(formatCurrencyAmount(NaN, bdt)).toBe('');
  });

  it('formats zero correctly rather than treating it as falsy', () => {
    expect(formatCurrencyAmount(0, bdt)).toBe('৳0');
  });
});
