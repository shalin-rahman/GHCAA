import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCommService, EmailTemplate } from '../../core/services/admin-comm.service';
import { NotificationService } from '../../core/services/notification.service';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'app-admin-comm',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './admin-comm.html',
    styleUrl: './admin-comm.scss'
})
export class AdminComm implements OnInit {
    private commService = inject(AdminCommService);
    private notify = inject(NotificationService);
    private route = inject(ActivatedRoute);

    templates = signal<EmailTemplate[]>([]);
    loading = signal(true);
    sending = signal(false);
    activeTab = signal('send'); // 'send' or 'templates'

    // Send Form
    sendOptions = {
        method: 'batch', // 'batch', 'type', 'custom'
        target: '',
        templateCode: '',
        customSubject: '',
        customBody: ''
    };

    years: number[] = [];

    constructor() {
        const currentYear = new Date().getFullYear();
        for (let i = currentYear; i >= 1950; i--) {
            this.years.push(i);
        }
    }

    ngOnInit() {
        this.loadTemplates();

        // Handle pre-filled target from Registry/Individual contact
        this.route.queryParams.subscribe(params => {
            if (params['target']) {
                this.sendOptions.target = params['target'];
            }
            if (params['method']) {
                this.sendOptions.method = params['method'];
            }
        });
    }

    loadTemplates() {
        this.loading.set(true);
        this.commService.getTemplates().subscribe({
            next: (data: EmailTemplate[]) => {
                this.templates.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    sendMessage() {
        this.sending.set(true);
        let obs;

        if (this.sendOptions.method === 'batch') {
            obs = this.commService.sendBatch({
                passingYear: Number(this.sendOptions.target),
                templateCode: this.sendOptions.templateCode
            });
        } else if (this.sendOptions.method === 'type') {
            obs = this.commService.sendType({
                membershipType: this.sendOptions.target,
                templateCode: this.sendOptions.templateCode
            });
        } else {
            const emailList = this.sendOptions.target
                ? this.sendOptions.target.split(',').map(e => e.trim()).filter(e => e.length > 0)
                : [];

            obs = this.commService.sendCustom({
                emails: emailList,
                subject: this.sendOptions.customSubject,
                body: this.sendOptions.customBody
            });
        }

        obs.subscribe({
            next: () => {
                this.notify.success('Communication broadcast initiated successfully.');
                this.sending.set(false);
            },
            error: () => {
                this.notify.error('Failed to initiate broadcast.');
                this.sending.set(false);
            }
        });
    }
}
