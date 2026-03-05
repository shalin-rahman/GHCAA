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

    // Editing Template
    editingTemplate = signal<EmailTemplate | null>(null);
    isSaving = signal(false);

    // Manual Message Mode
    isManualMessage = false;

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
                if (params['method'] === 'custom') {
                    this.isManualMessage = true;
                    setTimeout(() => this.initBroadcastEditor(), 200);
                }
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

    switchToManual() {
        this.isManualMessage = true;
        setTimeout(() => this.initBroadcastEditor(), 100);
    }

    editTemplate(template: EmailTemplate) {
        this.editingTemplate.set({ ...template });
        // Use timeout to ensure DOM is updated before initializing Quill
        setTimeout(() => this.initEditor('template-editor', this.editingTemplate()?.body || '', (html) => {
            const t = this.editingTemplate();
            if (t) t.body = html;
        }), 100);
    }

    private initEditor(elementId: string, initialContent: string, onChange: (html: string) => void) {
        const editorDiv = document.getElementById(elementId);
        if (editorDiv && (window as any).Quill) {
            const quill = new (window as any).Quill(`#${elementId}`, {
                theme: 'snow',
                modules: {
                    toolbar: [
                        [{ 'header': [1, 2, 3, false] }],
                        ['bold', 'italic', 'underline', 'strike'],
                        [{ 'color': [] }, { 'background': [] }],
                        ['link', 'image'],
                        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                        ['clean']
                    ]
                }
            });

            quill.root.innerHTML = initialContent;

            quill.on('text-change', () => {
                onChange(quill.root.innerHTML);
            });
        }
    }

    initBroadcastEditor() {
        if (!this.isManualMessage) return;
        this.initEditor('broadcast-editor', this.sendOptions.customBody, (html) => {
            this.sendOptions.customBody = html;
        });
    }

    saveTemplate() {
        const template = this.editingTemplate();
        if (!template) return;

        this.isSaving.set(true);
        this.commService.saveTemplate(template).subscribe({
            next: () => {
                this.notify.success('Template updated successfully.');
                this.isSaving.set(false);
                this.editingTemplate.set(null);
                this.loadTemplates();
            },
            error: () => {
                this.notify.error('Failed to update template.');
                this.isSaving.set(false);
            }
        });
    }

    cancelEdit() {
        this.editingTemplate.set(null);
    }

    sendMessage() {
        if (!this.isManualMessage && !this.sendOptions.templateCode) {
            this.notify.warning('Please select an email template.');
            return;
        }

        if (this.isManualMessage && !this.sendOptions.customSubject) {
            this.notify.warning('Please provide a subject line.');
            return;
        }

        this.sending.set(true);

        let obs;
        if (this.isManualMessage) {
            // Manual message can be sent to batch or type too if we expand API, 
            // but currently API sendBatch/sendType expects a templateCode.
            // We'll treat all manual as "custom" targeting the specific list 
            // OR if it's batch/type we need to handle it.

            // For now, let's assume sendCustom handles the manual logic.
            // If the user picked Batch + Manual, we might need a different API.
            // However, typical behavior is manual to custom list.

            const emailList = this.sendOptions.method === 'custom'
                ? this.sendOptions.target.split(',').map(e => e.trim()).filter(e => e.length > 0)
                : []; // If batch + manual, we'd need email list from batch

            obs = this.commService.sendCustom({
                emails: emailList,
                subject: this.sendOptions.customSubject,
                body: this.sendOptions.customBody,
                // Add context if it's batch/type manual
                targetMethod: this.sendOptions.method,
                targetValue: this.sendOptions.target
            });
        } else {
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
                    templateCode: this.sendOptions.templateCode
                });
            }
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
