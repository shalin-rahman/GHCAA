import { ChangeDetectionStrategy, Component, Input, Output, EventEmitter } from '@angular/core';

/**
 * 82.46: the title + close-button row every modal opens with, hand-rolled identically in 17
 * templates before this. A header that carries more than a title projects it: [lead] for a
 * banner or avatar above or before the title, the default slot for a sub-line or badges under
 * it, and [aside] for extra controls just before the close button. Leave the title out when the
 * projected content brings its own heading, as an editable name does.
 */
@Component({
    selector: 'app-modal-header',
    standalone: true,
    template: `
        <div class="modal-header">
            <ng-content select="[lead]"></ng-content>
            <div class="modal-header-main">
                @if (title) {
                    <h3 [id]="headingId" [class]="headingClass">{{ title }}</h3>
                }
                <ng-content></ng-content>
            </div>
            <ng-content select="[aside]"></ng-content>
            <button type="button" class="close-btn" (click)="closed.emit()" [attr.aria-label]="closeLabel">&times;</button>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModalHeaderComponent {
    @Input() title?: string;
    /** Set when the modal wrapper needs aria-labelledby to point at this heading. */
    @Input() headingId?: string;
    @Input() headingClass?: string;
    @Input() closeLabel = 'Close';
    @Output() closed = new EventEmitter<void>();
}
