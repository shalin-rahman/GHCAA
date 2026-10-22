import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AdminElectionContent } from './admin-election-content';
import { ElectionContentService } from '../../core/services/election-content.service';
import { NotificationService } from '../../core/services/notification.service';
import { ElectionContentDto } from '../../core/models/election.models';

function row(over: Partial<ElectionContentDto> = {}): ElectionContentDto {
    return {
        id: 1, key: 'regulations', kind: 'Page', format: 'Prose', group: 'Rules',
        title: 'Election Regulations', summary: null, body: null,
        displayOrder: 1, isPublished: true, lastModified: null,
        ...over
    };
}

// FR-25
describe('AdminElectionContent', () => {
    let service: any;
    let notify: { error: ReturnType<typeof vi.fn>; success: ReturnType<typeof vi.fn> };

    function create(rows: ElectionContentDto[] = [row(), row({ id: 2, key: 'ER-01', kind: 'Form', format: null, title: 'Nomination Paper', group: 'Forms' })]) {
        service = {
            adminList: vi.fn().mockReturnValue(of(rows)),
            adminGet: vi.fn().mockReturnValue(of(row({ body: '# {{org.fullName}} rules' }))),
            create: vi.fn().mockReturnValue(of(row())),
            update: vi.fn().mockReturnValue(of(row()))
        };
        notify = { error: vi.fn(), success: vi.fn() };
        TestBed.configureTestingModule({
            imports: [AdminElectionContent],
            providers: [
                { provide: ElectionContentService, useValue: service },
                { provide: NotificationService, useValue: notify }
            ]
        });
        const fixture = TestBed.createComponent(AdminElectionContent);
        fixture.detectChanges();
        return fixture.componentInstance;
    }

    it('lists every row and filters by kind and search text', () => {
        const component = create();

        expect(component.filtered().length).toBe(2);
        component.kindFilter.set('Form');
        expect(component.filtered().map(r => r.key)).toEqual(['ER-01']);
        component.kindFilter.set('');
        component.search.set('regul');
        expect(component.filtered().map(r => r.key)).toEqual(['regulations']);
    });

    it('fetches the raw body with its tokens when an entry is opened', () => {
        const component = create();

        component.openEdit(row());

        expect(service.adminGet).toHaveBeenCalledWith(1);
        expect(component.form().body).toBe('# {{org.fullName}} rules');
        expect(component.loadingBody()).toBe(false);
    });

    it('closes the editor and says why when the body cannot be loaded', () => {
        const component = create();
        service.adminGet.mockReturnValue(throwError(() => ({ error: { detail: 'Not found' } })));

        component.openEdit(row());

        expect(notify.error).toHaveBeenCalledWith('Not found');
        expect(component.showModal()).toBe(false);
    });

    it('clears the format when the kind changes to a form', () => {
        const component = create();
        component.openNew();

        component.setKind('Form');
        expect(component.form().format).toBeNull();
        component.setKind('Page');
        expect(component.form().format).toBe('Prose');
    });

    it('creates a new page with empty group and summary sent as null', () => {
        const component = create();
        component.openNew();
        component.form.update(f => ({ ...f, key: 'conduct', title: 'Code of Conduct', body: '# Conduct' }));

        component.save();

        expect(service.create).toHaveBeenCalledWith(expect.objectContaining({
            key: 'conduct', kind: 'Page', format: 'Prose', group: null, summary: null
        }));
        expect(notify.success).toHaveBeenCalled();
        expect(service.adminList).toHaveBeenCalledTimes(2);
    });

    it('updates an existing entry by id', () => {
        const component = create();
        component.openEdit(row());

        component.save();

        expect(service.update).toHaveBeenCalledWith(1, expect.objectContaining({ key: 'regulations' }));
    });

    it('refuses a form whose body is not JSON', () => {
        const component = create();
        component.openNew();
        component.setKind('Form');
        component.form.update(f => ({ ...f, key: 'ER-99', title: 'Test', body: 'not json' }));

        component.save();

        expect(service.create).not.toHaveBeenCalled();
        expect(notify.error).toHaveBeenCalledWith('A form body must be valid JSON.');
    });

    it('refuses a key with spaces and a missing title', () => {
        const component = create();
        component.openNew();
        component.form.update(f => ({ ...f, key: 'bad key', title: 'x', body: 'x' }));
        component.save();
        expect(notify.error).toHaveBeenCalledWith('Key may hold only letters, digits and hyphens.');

        component.form.update(f => ({ ...f, key: 'ok', title: ' ' }));
        component.save();
        expect(notify.error).toHaveBeenCalledWith('Key, title and body are required.');
        expect(service.create).not.toHaveBeenCalled();
    });

    it('shows the server message when a save fails', () => {
        const component = create();
        service.create.mockReturnValue(throwError(() => ({ error: { detail: 'Key already used' } })));
        component.openNew();
        component.form.update(f => ({ ...f, key: 'conduct', title: 'x', body: 'x' }));

        component.save();

        expect(notify.error).toHaveBeenCalledWith('Key already used');
        expect(component.isSaving()).toBe(false);
        expect(component.showModal()).toBe(true);
    });
});
