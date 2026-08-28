import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StepUpService } from '../../core/services/step-up.service';

/**
 * 7.13: Global host for the admin step-up (2FA) challenge. Rendered once at the app root
 * alongside <app-toast>, so any gated request anywhere can raise it without each page
 * wiring up its own dialog.
 */
@Component({
    selector: 'app-step-up-dialog',
    standalone: true,
    imports: [FormsModule],
    templateUrl: './step-up-dialog.html',
    styleUrl: './step-up-dialog.scss'
})
export class StepUpDialog {
    stepUp = inject(StepUpService);
    code = '';

    isCodeValid(): boolean {
        return /^\d{6}$/.test((this.code ?? '').trim());
    }

    submit(): void {
        if (!this.isCodeValid() || this.stepUp.verifying()) return;
        this.stepUp.submitCode(this.code.trim());
        this.code = '';
    }
}
