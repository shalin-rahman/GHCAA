import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ModalHeaderComponent } from '../modal-header/modal-header.component';

/**
 * 82.45: global host for the confirm dialog, rendered once at the app root next to
 * <app-step-up-dialog> and <app-toast>, so any admin action can raise it via
 * ConfirmDialogService.confirm() instead of the native window.confirm().
 */
@Component({
    selector: 'app-confirm-dialog',
    standalone: true,
    imports: [ModalHeaderComponent],
    templateUrl: './confirm-dialog.html',
    styleUrl: './confirm-dialog.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialog {
    dialog = inject(ConfirmDialogService);

    confirm(): void {
        this.dialog.resolve(true);
    }

    cancel(): void {
        this.dialog.resolve(false);
    }
}
