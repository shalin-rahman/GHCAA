import { Component, ChangeDetectionStrategy, Input, inject, signal } from '@angular/core';
import { AdminService } from '../../../core/services/admin.service';
import { HANDOVER_CHECK_ITEMS, HANDOVER_CHECK_LABELS, HandoverCheckDto, HandoverCheckItem } from '../../../core/models/business.models';

// 37.14m, ER-34. What the outgoing committee handed over for this term. A tick records who set it and when.
// A decorator input rather than input(), because the vitest JIT run does not wire signal inputs.
@Component({
    selector: 'app-handover-checklist',
    standalone: true,
    templateUrl: './handover-checklist.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HandoverChecklistComponent {
    private adminService = inject(AdminService);

    private readonly period = signal<number | null>(null);

    @Input({ required: true }) set periodId(value: number) {
        if (value === this.period()) return;
        this.period.set(value);
        this.checks.set([]);
        this.adminService.getHandoverChecks(value).subscribe({
            next: checks => { if (value === this.period()) this.checks.set(checks); }
        });
    }

    readonly items = HANDOVER_CHECK_ITEMS;
    readonly labels = HANDOVER_CHECK_LABELS;
    checks = signal<HandoverCheckDto[]>([]);
    busyItem = signal<HandoverCheckItem | null>(null);

    isChecked(item: HandoverCheckItem): boolean {
        return this.checks().find(check => check.item === item)?.isChecked ?? false;
    }

    setCheck(item: HandoverCheckItem, isChecked: boolean): void {
        const periodId = this.period();
        if (periodId === null || this.busyItem() !== null) return;
        this.busyItem.set(item);
        this.adminService.setHandoverCheck(periodId, item, isChecked).subscribe({
            next: checks => {
                if (periodId === this.period()) this.checks.set(checks);
                this.busyItem.set(null);
            },
            error: () => this.busyItem.set(null)
        });
    }
}
