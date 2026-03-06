import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCommService, EmailTemplate, EmailLog } from '../../core/services/admin-comm.service';
import { NotificationService } from '../../core/services/notification.service';
import { ActivatedRoute } from '@angular/router';
import { getAcademicYears } from '../../core/constants/app.constants';

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
    logs = signal<EmailLog[]>([]);
    loading = signal(true);
    sending = signal(false);
    activeTab = signal('send'); // 'send', 'templates', 'logs'

    // Editing Template
    editingTemplate = signal<EmailTemplate | null>(null);
    isSaving = signal(false);

    // Manual Message Mode
    isManualMessage = false;

    // Send Form
    sendOptions = {
        method: 'batch', // 'batch', 'type', 'custom'
        target: '',
        targetYears: [] as number[],
        targetTypes: [] as string[],
        templateCode: '',
        customSubject: '',
        customBody: ''
    };

    years: number[] = getAcademicYears();
    membershipTypes = [
        { value: 'Founding', label: 'Founding Member' },
        { value: 'Executive', label: 'Executive Committee' },
        { value: 'General', label: 'General Member' },
        { value: 'Associate', label: 'Associate Member' },
        { value: 'Honorary', label: 'Honorary Member' },
        { value: 'Advisory', label: 'Advisory Member' }
    ];



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

    setTab(tab: string) {
        this.activeTab.set(tab);
        if (tab === 'logs') {
            this.loadLogs();
        } else if (tab === 'templates') {
            this.loadTemplates();
        }
    }

    loadTemplates() {
        this.loading.set(true);
        this.commService.getTemplates().subscribe({
            next: (data: EmailTemplate[]) => {
                this.templates.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.loading.set(false);
                this.notify.error('Failed to load email templates.');
                console.error('Error loading templates:', err);
            }
        });
    }

    loadLogs() {
        this.loading.set(true);
        this.commService.getLogs().subscribe({
            next: (data) => {
                this.logs.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.loading.set(false);
                this.notify.error('Failed to load email logs.');
                console.error('Error loading logs:', err);
            }
        });
    }

    switchToManual() {
        this.isManualMessage = true;
        setTimeout(() => this.initBroadcastEditor(), 100);
    }

    editTemplate(template: EmailTemplate) {
        console.log('Editing template:', template.code, 'Body length:', template.body?.length);
        this.editingTemplate.set({ ...template });
        // Use timeout to ensure DOM is updated before initializing Quill
        setTimeout(() => {
            const content = this.editingTemplate()?.body || '';
            this.initEditor('template-editor', content, (html) => {
                const t = this.editingTemplate();
                if (t) t.body = html;
            });
        }, 200);
    }

    private initEditor(elementId: string, initialContent: string, onChange: (html: string) => void) {
        const editorDiv = document.getElementById(elementId);
        if (editorDiv && (window as any).Quill) {
            // Clear any previous Quill instances or content
            editorDiv.innerHTML = '';

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

            if (initialContent) {
                quill.root.innerHTML = initialContent;
            }

            quill.on('text-change', () => {
                const html = quill.root.innerHTML;
                onChange(html);
            });
        } else {
            console.error('Editor DIV not found or Quill not loaded:', elementId);
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
        const isNew = !template.id;

        this.commService.saveTemplate(template).subscribe({
            next: () => {
                this.notify.success(isNew ? 'Template created successfully.' : 'Template updated successfully.');
                this.isSaving.set(false);
                this.editingTemplate.set(null);
                this.loadTemplates();
            },
            error: () => {
                this.notify.error('Failed to save template.');
                this.isSaving.set(false);
            }
        });
    }

    addNewTemplate() {
        this.editingTemplate.set({
            id: 0,
            code: '',
            description: '',
            subject: '',
            body: ''
        });
        setTimeout(() => this.initEditor('template-editor', '', (html) => {
            const t = this.editingTemplate();
            if (t) t.body = html;
        }), 100);
    }

    cancelEdit() {
        this.editingTemplate.set(null);
    }

    selectAllYears() {
        this.sendOptions.targetYears = [...this.years];
    }

    clearYears() {
        this.sendOptions.targetYears = [];
    }

    selectAllTypes() {
        this.sendOptions.targetTypes = this.membershipTypes.map(t => t.value);
    }

    clearTypes() {
        this.sendOptions.targetTypes = [];
    }

    deleteTemplate(template: EmailTemplate) {
        if (confirm(`Are you sure you want to delete the template '${template.code}'?`)) {
            this.commService.deleteTemplate(template.id).subscribe({
                next: () => {
                    this.notify.success('Template deleted successfully.');
                    this.loadTemplates();
                },
                error: () => this.notify.error('Failed to delete template.')
            });
        }
    }

    toggleSelection(item: any, list: any[]) {
        const index = list.indexOf(item);
        if (index > -1) {
            list.splice(index, 1);
        } else {
            list.push(item);
        }
    }

    isSelected(item: any, list: any[]): boolean {
        return list.indexOf(item) > -1;
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

        if (this.sendOptions.method === 'batch' && this.sendOptions.targetYears.length === 0) {
            this.notify.warning('Please select at least one batch year.');
            return;
        }

        if (this.sendOptions.method === 'type' && this.sendOptions.targetTypes.length === 0) {
            this.notify.warning('Please select at least one membership type.');
            return;
        }

        this.sending.set(true);

        let obs;
        if (this.isManualMessage) {
            obs = this.commService.sendCustom({
                subject: this.sendOptions.customSubject,
                body: this.sendOptions.customBody,
                targetMethod: this.sendOptions.method,
                targetValues: this.sendOptions.method === 'batch'
                    ? this.sendOptions.targetYears.map(y => y.toString())
                    : (this.sendOptions.method === 'type' ? this.sendOptions.targetTypes : []),
                emails: this.sendOptions.method === 'custom'
                    ? this.sendOptions.target.split(',').map(e => e.trim()).filter(e => e.length > 0)
                    : []
            });
        } else {
            if (this.sendOptions.method === 'batch') {
                obs = this.commService.sendBatch({
                    passingYears: this.sendOptions.targetYears,
                    templateCode: this.sendOptions.templateCode
                });
            } else if (this.sendOptions.method === 'type') {
                obs = this.commService.sendType({
                    membershipTypes: this.sendOptions.targetTypes,
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
                // Switch to logs to see if it's sending
                setTimeout(() => this.setTab('logs'), 1000);
            },
            error: () => {
                this.notify.error('Failed to initiate broadcast.');
                this.sending.set(false);
            }
        });
    }
}
