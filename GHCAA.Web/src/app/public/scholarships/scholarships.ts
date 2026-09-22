import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { NotificationService } from '../../core/services/notification.service';
import { ScholarshipService } from '../../core/services/scholarship.service';
import { CreateScholarshipApplication, ScholarshipCall, ScholarshipFund, ScholarshipStatus } from '../../core/models/business.models';

@Component({
    selector: 'app-scholarships',
    standalone: true,
    imports: [CommonModule, FormsModule, LoadingPanelComponent],
    templateUrl: './scholarships.html',
    styleUrl: './scholarships.scss'
})
export class Scholarships implements OnInit {
    private service = inject(ScholarshipService);
    private notify = inject(NotificationService);
    loading = signal(true);
    calls = signal<ScholarshipCall[]>([]);
    funds = signal<ScholarshipFund[]>([]);
    selectedCall = signal<ScholarshipCall | null>(null);
    status = signal<ScholarshipStatus | null>(null);
    submitting = signal(false);
    lookingUp = signal(false);
    referenceCode = '';
    statusEmail = '';
    application: CreateScholarshipApplication = {
        applicantName: '', applicantEmail: '', applicantPhone: '', institutionName: '', class: '',
        guardianName: '', householdIncome: 0, needStatement: '', meritStatement: ''
    };

    ngOnInit() {
        this.service.getPublicCalls().subscribe({
            next: calls => { this.calls.set(calls); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
        this.service.getPublicFunds().subscribe({ next: funds => this.funds.set(funds), error: () => undefined });
    }

    chooseCall(call: ScholarshipCall) {
        this.selectedCall.set(call);
        document.getElementById('scholarship-application')?.scrollIntoView({ behavior: 'smooth' });
    }

    submitApplication() {
        const call = this.selectedCall();
        if (!call || !this.application.applicantName.trim() || !this.application.applicantEmail.trim()) {
            this.notify.error('Choose a call and enter your name and email.');
            return;
        }
        this.submitting.set(true);
        this.service.submitApplication(call.id, this.application).subscribe({
            next: result => {
                this.notify.success(`Application submitted. Reference: ${result.referenceCode}`);
                this.application = { applicantName: '', applicantEmail: '', applicantPhone: '', institutionName: '', class: '', guardianName: '', householdIncome: 0, needStatement: '', meritStatement: '' };
                this.selectedCall.set(null);
                this.submitting.set(false);
            },
            error: () => this.submitting.set(false)
        });
    }

    lookupStatus() {
        if (!this.referenceCode.trim() || !this.statusEmail.trim()) {
            this.notify.error('Enter your reference code and email.');
            return;
        }
        this.lookingUp.set(true);
        this.status.set(null);
        this.service.getStatus(this.referenceCode.trim(), this.statusEmail.trim()).subscribe({
            next: result => { this.status.set(result); this.lookingUp.set(false); },
            error: () => this.lookingUp.set(false)
        });
    }
}
