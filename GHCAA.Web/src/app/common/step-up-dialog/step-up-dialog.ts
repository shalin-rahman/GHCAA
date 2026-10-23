import { Component, inject } from '@angular/core';
import { StepUpService } from '../../core/services/step-up.service';
import { ModalHeaderComponent } from '../modal-header/modal-header.component';

/**
 * 7.13: Global host for the admin step-up (2FA) challenge. Rendered once at the app root
 * alongside <app-toast>, so any gated request anywhere can raise it without each page
 * wiring up its own dialog.
 */
@Component({
    selector: 'app-step-up-dialog',
    standalone: true,
    // No FormsModule here: this dialog loads with the app shell, and FormsModule alone
    // added about 50 kB to the initial bundle for one input.
    imports: [ModalHeaderComponent],
    templateUrl: './step-up-dialog.html',
    styleUrl: './step-up-dialog.scss'
})
export class StepUpDialog {
    stepUp = inject(StepUpService);
    code = '';

    // App mode also takes a recovery code, which is 10 letters or digits with an optional dash.
    isCodeValid(): boolean {
        const code = (this.code ?? '').trim();
        if (/^\d{6}$/.test(code)) return true;
        return this.stepUp.method() === 'app' && /^[A-Za-z0-9]{5}-?[A-Za-z0-9]{5}$/.test(code);
    }

    switchMethod(): void {
        this.code = '';
        this.stepUp.useMethod(this.stepUp.method() === 'app' ? 'email' : 'app');
    }

    submit(): void {
        if (!this.isCodeValid() || this.stepUp.verifying()) return;
        this.stepUp.submitCode(this.code.trim());
        this.code = '';
    }
}
