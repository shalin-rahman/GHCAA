import { of } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { ElectionApprovalDto } from '../models/election.models';
import { confirmRejectApproval } from './election-approval.util';

describe('confirmRejectApproval', () => {
    const approval = { id: 1, action: 'Declare', requestedBy: 'Asha' } as ElectionApprovalDto;

    it('asks with the action label and the requester, as a danger prompt', async () => {
        const dialog = { confirm: vi.fn().mockReturnValue(of(true)) };

        const result = await confirmRejectApproval(dialog as any, approval);

        expect(result).toBe(true);
        const options = dialog.confirm.mock.calls[0][0];
        expect(options.message).toContain('Declare results');
        expect(options.message).toContain('Asha');
        expect(options.danger).toBe(true);
    });

    it('returns false when the dialog is declined', async () => {
        const dialog = { confirm: vi.fn().mockReturnValue(of(false)) };

        expect(await confirmRejectApproval(dialog as any, approval)).toBe(false);
    });
});
