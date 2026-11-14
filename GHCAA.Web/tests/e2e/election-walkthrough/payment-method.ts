import { call, field } from './api';

// The membership and event walkthroughs pay by cash. This makes sure a cash method with no
// gateway is enabled, creating or switching it on through the SuperAdmin payment settings when
// needed, so a run does not depend on what an admin left enabled. It stays on afterwards; it is a
// normal manual method an admin can switch off again under Payment settings.
export const CASH_METHOD = {
  Method: 'CashOnHand',
  Gateway: 'None',
  DisplayName: 'Cash (manual receipt)',
  Description: 'Paid in cash to the treasurer.',
  Instructions: 'Pay the treasurer in cash and enter the receipt number you are given.',
  IsEnabled: true,
  RequiresReceipt: false,
  RequiresReference: true,
  SortOrder: 90,
};

/** Returns the public view of the cash method and what had to be done to get it. */
export async function ensureCashMethod(superAdminToken: string): Promise<{ method: any; action: 'found' | 'enabled' | 'created' }> {
  const all = (await call<any[]>('GET', '/payment-config/admin/all', superAdminToken)).body;
  const existing = all.find(c => field(c, 'Method') === CASH_METHOD.Method && field(c, 'Gateway') === CASH_METHOD.Gateway);
  let action: 'found' | 'enabled' | 'created' = 'found';
  if (!existing) {
    await call('POST', '/payment-config/admin', superAdminToken, CASH_METHOD);
    action = 'created';
  } else if (!field(existing, 'IsEnabled')) {
    await call('POST', `/payment-config/admin/${field(existing, 'Id')}/toggle`, superAdminToken);
    action = 'enabled';
  }
  const active = (await call<any[]>('GET', '/payment-config/active')).body;
  const method = active.find(c => field(c, 'Method') === CASH_METHOD.Method && !field(c, 'IsOnline'));
  if (!method) throw new Error('The cash payment method is still not listed as active after setting it up.');
  return { method, action };
}
