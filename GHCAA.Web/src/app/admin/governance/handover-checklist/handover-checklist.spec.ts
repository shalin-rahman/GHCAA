import { TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { HandoverChecklistComponent } from './handover-checklist';
import { AdminService } from '../../../core/services/admin.service';
import { HANDOVER_CHECK_ITEMS, HandoverCheckDto } from '../../../core/models/business.models';

// TODO 37.14m.
describe('HandoverChecklistComponent', () => {
    let adminMock: any;
    const tick = (item: HandoverCheckDto['item'], isChecked = true): HandoverCheckDto =>
        ({ item, isChecked, setByUserId: 1, setAt: '2026-10-21T08:00:00Z' });

    const create = async (periodId = 5) => {
        adminMock = {
            getHandoverChecks: vi.fn().mockReturnValue(of([tick('Constitution')])),
            setHandoverCheck: vi.fn().mockReturnValue(of([tick('Constitution'), tick('MinuteBooks')]))
        };
        await TestBed.configureTestingModule({
            imports: [HandoverChecklistComponent],
            providers: [{ provide: AdminService, useValue: adminMock }]
        }).compileComponents();
        const fixture = TestBed.createComponent(HandoverChecklistComponent);
        fixture.componentRef.setInput('periodId', periodId);
        fixture.detectChanges();
        return { fixture, component: fixture.componentInstance };
    };

    it('loads the ticks for the term and lists every item', async () => {
        const { fixture, component } = await create();
        expect(adminMock.getHandoverChecks).toHaveBeenCalledWith(5);
        expect(component.isChecked('Constitution')).toBe(true);
        expect(component.isChecked('MinuteBooks')).toBe(false);
        expect(fixture.nativeElement.querySelectorAll('input[type=checkbox]').length).toBe(HANDOVER_CHECK_ITEMS.length);
    });

    it('saves a tick and shows the list the server returns', async () => {
        const { component } = await create();
        component.setCheck('MinuteBooks', true);
        expect(adminMock.setHandoverCheck).toHaveBeenCalledWith(5, 'MinuteBooks', true);
        expect(component.isChecked('MinuteBooks')).toBe(true);
        expect(component.busyItem()).toBeNull();
    });

    it('ignores a second tick while one is saving, and frees up after an error', async () => {
        const { component } = await create();
        const reply = new Subject<HandoverCheckDto[]>();
        adminMock.setHandoverCheck.mockReturnValue(reply);
        component.setCheck('MinuteBooks', true);
        component.setCheck('Constitution', false);
        expect(adminMock.setHandoverCheck).toHaveBeenCalledTimes(1);

        reply.error(new Error('403'));
        expect(component.busyItem()).toBeNull();
    });

    it('reloads when the selected term changes and drops a late reply for the old term', async () => {
        const { fixture, component } = await create();
        const late = new Subject<HandoverCheckDto[]>();
        adminMock.getHandoverChecks.mockReturnValueOnce(late).mockReturnValueOnce(of([]));

        fixture.componentRef.setInput('periodId', 6);
        fixture.detectChanges();
        expect(adminMock.getHandoverChecks).toHaveBeenLastCalledWith(6);
        expect(component.isChecked('Constitution')).toBe(false);

        fixture.componentRef.setInput('periodId', 7);
        fixture.detectChanges();
        expect(adminMock.getHandoverChecks).toHaveBeenLastCalledWith(7);

        late.next([tick('Constitution')]);
        expect(component.isChecked('Constitution')).toBe(false);
    });
});
