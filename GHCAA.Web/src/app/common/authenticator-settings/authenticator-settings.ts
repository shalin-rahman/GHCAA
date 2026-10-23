import { Component, OnInit, inject, signal } from '@angular/core';
import { filter, switchMap } from 'rxjs/operators';
import {
    AuthenticatorSetup, AuthenticatorStatus, StepUpService
} from '../../core/services/step-up.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { NotificationService } from '../../core/services/notification.service';

/**
 * 95.6: lets a signed-in user set up an authenticator app for step-up, see how many recovery
 * codes are left, get new ones, or remove the app. Recovery codes are shown once and never
 * again, so the list stays on screen until the user says they have saved it.
 */
@Component({
    selector: 'app-authenticator-settings',
    standalone: true,
    templateUrl: './authenticator-settings.html',
    styleUrl: './authenticator-settings.scss'
})
export class AuthenticatorSettings implements OnInit {
    private stepUp = inject(StepUpService);
    private confirmDialog = inject(ConfirmDialogService);
    private notify = inject(NotificationService);

    readonly status = signal<AuthenticatorStatus | null>(null);
    readonly setup = signal<AuthenticatorSetup | null>(null);
    readonly recoveryCodes = signal<string[] | null>(null);
    readonly busy = signal(false);
    readonly errorMessage = signal<string | null>(null);
    code = '';
    password = '';

    ngOnInit(): void {
        this.loadStatus();
    }

    loadStatus(): void {
        this.stepUp.authenticatorStatus().subscribe({
            next: status => this.status.set(status),
            error: () => this.errorMessage.set('Could not load the authenticator app settings.')
        });
    }

    isCodeValid(): boolean {
        return /^\d{6}$/.test((this.code ?? '').trim());
    }

    startSetup(): void {
        if (this.busy()) return;
        const withPassword = !!this.status()?.setupNeedsPassword;
        if (withPassword && !this.password) return;
        this.busy.set(true);
        this.errorMessage.set(null);
        const request = withPassword
            ? this.stepUp.setupAuthenticatorWithPassword(this.password)
            : this.stepUp.setupAuthenticator();
        request.subscribe({
            next: setup => {
                this.busy.set(false);
                this.code = '';
                this.password = '';
                this.setup.set(setup);
            },
            error: err => {
                this.busy.set(false);
                if (withPassword) this.errorMessage.set(err?.error?.detail || 'Could not start the setup.');
                this.loadStatus();
            }
        });
    }

    confirmSetup(): void {
        if (!this.isCodeValid() || this.busy()) return;
        this.busy.set(true);
        this.errorMessage.set(null);
        this.stepUp.confirmAuthenticator(this.code.trim()).subscribe({
            next: res => {
                this.busy.set(false);
                this.code = '';
                this.setup.set(null);
                this.recoveryCodes.set(res.codes);
                this.loadStatus();
            },
            error: err => {
                this.busy.set(false);
                this.errorMessage.set(err?.error?.detail || 'That code does not match. Try the newest code.');
            }
        });
    }

    cancelSetup(): void {
        this.setup.set(null);
        this.code = '';
        this.errorMessage.set(null);
    }

    regenerateCodes(): void {
        if (this.busy()) return;
        this.confirmDialog.confirm({
            title: 'Get new recovery codes',
            message: 'Your old recovery codes will stop working. Save the new ones somewhere safe.',
            confirmLabel: 'Get new codes'
        }).pipe(
            filter(Boolean),
            switchMap(() => {
                this.busy.set(true);
                return this.stepUp.regenerateRecoveryCodes();
            })
        ).subscribe({
            next: res => {
                this.busy.set(false);
                this.recoveryCodes.set(res.codes);
                this.loadStatus();
            },
            error: () => this.busy.set(false)
        });
    }

    remove(): void {
        if (this.busy()) return;
        this.confirmDialog.confirm({
            title: 'Remove authenticator app',
            message: 'Protected actions will ask for a code by email only. Your recovery codes will stop working.',
            confirmLabel: 'Remove',
            danger: true
        }).pipe(
            filter(Boolean),
            switchMap(() => {
                this.busy.set(true);
                return this.stepUp.removeAuthenticator();
            })
        ).subscribe({
            next: () => {
                this.busy.set(false);
                this.recoveryCodes.set(null);
                this.notify.success('Authenticator app removed.');
                this.loadStatus();
            },
            error: () => this.busy.set(false)
        });
    }

    copy(text: string): void {
        navigator.clipboard?.writeText(text).then(
            () => this.notify.success('Copied.'),
            () => this.notify.error('Could not copy. Select the text and copy it by hand.')
        );
    }

    doneWithCodes(): void {
        this.recoveryCodes.set(null);
    }
}
