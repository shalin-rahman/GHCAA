import { Component, inject, signal, OnInit, computed, ViewChild, ElementRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AppDatePipe } from '../../core/pipes/app-date.pipe';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AdminCommService, EmailTemplate, EmailLog, TEMPLATE_VARIABLES, MessageChannels } from '../../core/services/admin-comm.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ActivatedRoute } from '@angular/router';
import { LOOKUP_GROUPS } from '../../core/constants/app.constants';
import { LookupService, LookupOption } from '../../core/services/lookup.service';
import { RichTextEditor } from '../../common/rich-text-editor/rich-text-editor';
import { LoadingPanelComponent } from '../../common/loading-panel/loading-panel';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { ModalHeaderComponent } from '../../common/modal-header/modal-header.component';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';

@Component({
    selector: 'app-admin-comm',
    standalone: true,
    imports: [CommonModule, AppDatePipe, FormsModule, RichTextEditor, LoadingPanelComponent, SearchBarComponent, ModalHeaderComponent, PageHeaderComponent],
    templateUrl: './admin-comm.html',
    styleUrl: './admin-comm.scss'
})
export class AdminComm implements OnInit {
    private commService = inject(AdminCommService);
    private notify = inject(NotificationService);
    private confirmDialog = inject(ConfirmDialogService);
    private route = inject(ActivatedRoute);
    private destroyRef = inject(DestroyRef);
    private lookupService = inject(LookupService);

    @ViewChild(RichTextEditor) bodyEditor?: RichTextEditor;
    @ViewChild('smsBodyInput') smsBodyInput?: ElementRef<HTMLTextAreaElement>;

    readonly templateVariables = TEMPLATE_VARIABLES;
    readonly channels = MessageChannels;

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

    // 82.42: sourced from /lookups/PassingYear via LookupService. A signal (not a plain array)
    // because filteredYears below is a computed() that only reruns off signal reads.
    years = signal<number[]>([]);
    // 62.33: sourced from LOOKUP_GROUPS.MembershipType, filled in ngOnInit
    membershipTypes: LookupOption[] = [];

    yearSearch = signal('');
    filteredYears = computed(() => {
        const q = this.yearSearch().toLowerCase();
        return this.years().filter(y => y.toString().includes(q));
    });

    templateSearch = signal('');
    filteredTemplates = computed(() => {
        const q = this.templateSearch().toLowerCase();
        return this.templates().filter(t =>
            t.code.toLowerCase().includes(q) ||
            t.description.toLowerCase().includes(q) ||
            t.subject.toLowerCase().includes(q)
        );
    });

    logSearch = signal('');
    memberLogId = signal<number | null>(null);
    filteredLogs = computed(() => {
        const q = this.logSearch().toLowerCase();
        return this.logs().filter(l =>
            l.recipientEmail.toLowerCase().includes(q) ||
            l.subject.toLowerCase().includes(q) ||
            l.status.toLowerCase().includes(q)
        );
    });



    ngOnInit() {
        this.loadTemplates();
        this.lookupService.getAcademicYears().subscribe(years => this.years.set(years));
        this.lookupService.getOptions(LOOKUP_GROUPS.MembershipType).subscribe(opts => this.membershipTypes = opts);

        // Handle pre-filled target from Registry/Individual contact
        this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
            if (params['target']) {
                this.sendOptions.target = params['target'];
            }
            if (params['method']) {
                this.sendOptions.method = params['method'];
                if (params['method'] === 'custom') {
                    this.isManualMessage = true;
                }
            }
            const memberId = Number(params['memberId']);
            this.memberLogId.set(Number.isInteger(memberId) && memberId > 0 ? memberId : null);
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
        if (this.memberLogId()) {
            this.commService.getMemberLogs(this.memberLogId()!).subscribe({
                next: data => this.finishLogLoad(data.items),
                error: err => this.failLogLoad(err)
            });
            return;
        }
        this.commService.getLogs().subscribe({
            next: data => this.finishLogLoad(data),
            error: err => this.failLogLoad(err)
        });
    }

    private finishLogLoad(data: EmailLog[]): void {
        this.logs.set(data);
        this.loading.set(false);
    }

    private failLogLoad(err: unknown): void {
        this.loading.set(false);
        this.notify.error('Failed to load email logs.');
        console.error('Error loading logs:', err);
    }

    switchToManual() {
        this.isManualMessage = true;
    }

    editTemplate(template: EmailTemplate) {
        this.editingTemplate.set({ ...template });
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
            channel: this.channels.Email,
            code: '',
            description: '',
            subject: '',
            body: '',
            variables: '[]'
        });
    }

    cancelEdit() {
        this.editingTemplate.set(null);
    }

    // Plain method (not a computed signal): editingTemplate().body is mutated in place by
    // ngModel, which never marks the editingTemplate signal dirty, so a computed() here would
    // cache a stale count. A plain method re-evaluates on every change-detection tick instead.
    smsSegmentCount(): number {
        const len = this.editingTemplate()?.body?.length || 0;
        return len === 0 ? 0 : Math.ceil(len / 160);
    }

    /** Inserts a `{{VarName}}` placeholder at the cursor of whichever body editor is active for the current channel. */
    insertVariable(varName: string) {
        const template = this.editingTemplate();
        if (!template) return;
        const placeholder = `{{${varName}}}`;

        if (template.channel === this.channels.Sms) {
            const el = this.smsBodyInput?.nativeElement;
            const current = template.body || '';
            if (el) {
                const start = el.selectionStart ?? current.length;
                const end = el.selectionEnd ?? current.length;
                template.body = current.slice(0, start) + placeholder + current.slice(end);
                const cursor = start + placeholder.length;
                setTimeout(() => el.setSelectionRange(cursor, cursor));
            } else {
                template.body = current + placeholder;
            }
        } else {
            this.bodyEditor?.insertAtCursor(placeholder);
        }
    }

    selectAllYears() {
        this.sendOptions.targetYears = [...this.years()];
    }

    selectFilteredYears() {
        const filtered = this.filteredYears();
        const current = new Set(this.sendOptions.targetYears);
        filtered.forEach(y => current.add(y));
        this.sendOptions.targetYears = Array.from(current);
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

    async deleteTemplate(template: EmailTemplate) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete template',
            message: `Are you sure you want to delete the template '${template.code}'?`,
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.commService.deleteTemplate(template.id).subscribe({
            next: () => {
                this.notify.success('Template deleted successfully.');
                this.loadTemplates();
            },
            error: () => this.notify.error('Failed to delete template.')
        });
    }

    toggleSelection(item: any, listName: 'targetYears' | 'targetTypes') {
        const list = this.sendOptions[listName] as any[];
        const index = list.indexOf(item);
        if (index > -1) {
            this.sendOptions[listName] = list.filter(i => i !== item) as any;
        } else {
            this.sendOptions[listName] = [...list, item] as any;
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
