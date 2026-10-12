import { firstValueFrom } from 'rxjs';
import { ELECTION_APPROVAL_ACTION_LABELS, ElectionApprovalDto } from '../models/election.models';
import { ConfirmDialogService } from '../services/confirm-dialog.service';

// The officials dashboard and the admin elections screen both ask this before a reject.
export function confirmRejectApproval(dialog: ConfirmDialogService, approval: ElectionApprovalDto): Promise<boolean> {
    return firstValueFrom(dialog.confirm({
        title: 'Reject request',
        message: `Reject "${ELECTION_APPROVAL_ACTION_LABELS[approval.action]}" asked for by ${approval.requestedBy}? It will not happen.`,
        confirmLabel: 'Reject',
        danger: true
    }));
}
