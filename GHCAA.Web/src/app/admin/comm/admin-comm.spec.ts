import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminComm } from './admin-comm';
import { AdminCommService, MessageChannels } from '../../core/services/admin-comm.service';
import { NotificationService } from '../../core/services/notification.service';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

describe('AdminComm Component', () => {
    let component: AdminComm;
    let fixture: ComponentFixture<AdminComm>;
    let commServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        commServiceMock = {
            getTemplates: vi.fn().mockReturnValue(of([])),
            getLogs: vi.fn().mockReturnValue(of([])),
            saveTemplate: vi.fn().mockReturnValue(of({})),
            deleteTemplate: vi.fn().mockReturnValue(of({})),
            sendBatch: vi.fn().mockReturnValue(of({})),
            sendType: vi.fn().mockReturnValue(of({})),
            sendCustom: vi.fn().mockReturnValue(of({}))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn(),
            warning: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [AdminComm],
            providers: [
                { provide: AdminCommService, useValue: commServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ActivatedRoute, useValue: { queryParams: of({}) } }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(AdminComm);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should default a new template to the Email channel', () => {
        component.addNewTemplate();
        expect(component.editingTemplate()?.channel).toBe(MessageChannels.Email);
    });

    // Note: this project's vitest config strips every templateUrl to an empty template for all
    // specs (see vitest.config.ts's angular-resource-stripper plugin), so DOM-structure assertions
    // against admin-comm.html are not possible here — these tests check the underlying state
    // (editingTemplate/templates signals, insertVariable's cursor logic) that the template renders from.

    it('should toggle the editing template between channels', () => {
        component.addNewTemplate();
        expect(component.editingTemplate()?.channel).toBe(MessageChannels.Email);

        component.editingTemplate.update(t => t ? { ...t, channel: MessageChannels.Sms } : t);
        expect(component.editingTemplate()?.channel).toBe(MessageChannels.Sms);
    });

    it('should insert a variable placeholder into the SMS body at the cursor', () => {
        component.addNewTemplate();
        component.editingTemplate.update(t => t ? { ...t, channel: MessageChannels.Sms, body: 'AB' } : t);

        component.insertVariable('FullName');

        expect(component.editingTemplate()?.body).toContain('{{FullName}}');
    });

    it('should not throw when inserting a variable into an Email template with no rendered editor', () => {
        component.addNewTemplate();
        // bodyEditor ViewChild is never populated under the stripped test template (no real
        // <app-rich-text-editor> is rendered), so this only proves insertVariable's optional-chain
        // guard holds — real cursor insertion is covered by RichTextEditor's own insertAtCursor.
        expect(() => component.insertVariable('OrgName')).not.toThrow();
    });

    it('should carry the channel through into the loaded templates list', () => {
        component.templates.set([
            { id: 1, channel: MessageChannels.Sms, code: 'SMS1', subject: '', body: 'B', description: 'D' },
            { id: 2, channel: MessageChannels.Email, code: 'EMAIL1', subject: 'S', body: 'B', description: 'D' }
        ]);

        expect(component.filteredTemplates().map(t => t.channel)).toEqual([MessageChannels.Sms, MessageChannels.Email]);
    });
});
