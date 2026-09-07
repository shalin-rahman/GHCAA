import { Pipe, PipeTransform, inject } from '@angular/core';
import { OrgConfigService } from '../services/org-config.service';
import { formatCurrencyAmount } from '../utils/currency.util';

/**
 * `{{ value | appCurrency }}` -> "৳1,234" (or whatever OrgConfig.currency is for this
 * institution). Replaces every hardcoded ৳/BDT literal across the app (62.51) so a
 * non-BDT profile shows its own symbol/code without a template change.
 *
 * Optional args match the built-in `number`/`currency` pipes' feel:
 *   {{ value | appCurrency:'1.2-2' }}          -> "৳1,234.50"
 *   {{ value | appCurrency:'1.0-0':'code' }}   -> "BDT 1,234"
 */
@Pipe({ name: 'appCurrency', standalone: true, pure: false })
export class AppCurrencyPipe implements PipeTransform {
  private orgConfig = inject(OrgConfigService);

  transform(value: number | null | undefined, digitsInfo = '1.0-0', display: 'symbol' | 'code' = 'symbol'): string {
    return formatCurrencyAmount(value, this.orgConfig.config()?.currency, digitsInfo, display);
  }
}
