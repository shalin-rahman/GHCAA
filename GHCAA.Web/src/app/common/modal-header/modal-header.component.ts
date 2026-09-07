import { ChangeDetectionStrategy, Component, Input, Output, EventEmitter } from '@angular/core';

/**
 * 82.46: the title + close-button row every modal opens with, hand-rolled identically in 17
 * templates before this. Covers the plain case (a heading and a close button); a modal whose
 * header carries more than that (a photo, a badge, an editable title) keeps its own markup.
 */
@Component({
    selector: 'app-modal-header',
    standalone: true,
    template: `
        <div class="modal-header">
            <h3 [id]="headingId" [class]="headingClass">{{ title }}</h3>
            <button type="button" class="close-btn" (click)="closed.emit()" [attr.aria-label]="closeLabel">&times;</button>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModalHeaderComponent {
    @Input({ required: true }) title!: string;
    /** Set when the modal wrapper needs aria-labelledby to point at this heading. */
    @Input() headingId?: string;
    @Input() headingClass?: string;
    @Input() closeLabel = 'Close';
    @Output() closed = new EventEmitter<void>();
}
