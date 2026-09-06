import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

/**
 * Shared "notify members" checkbox for admin create/edit forms (82.52).
 *
 * Each form that can trigger a member notification (EC assignment/removal,
 * events, news, etc.) gets one of these instead of its own one-off checkbox
 * markup, so a future shared notification-composer component can replace
 * every usage in one place rather than rewriting each form.
 *
 * Usage:
 *   <app-notify-toggle
 *       [checked]="notifyMember()"
 *       (checkedChange)="notifyMember.set($event)"></app-notify-toggle>
 */
@Component({
  selector: 'app-notify-toggle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <label class="notify-toggle">
      <input
        type="checkbox"
        [ngModel]="checked"
        (ngModelChange)="onChange($event)" />
      <span>{{ label }}</span>
    </label>
  `,
  styles: [
    `
      .notify-toggle {
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
        cursor: pointer;
        font-size: 0.9rem;
      }
    `,
  ],
})
export class NotifyToggleComponent {
  @Input() checked = false;
  @Input() label = 'Notify affected member';
  @Output() checkedChange = new EventEmitter<boolean>();

  onChange(v: boolean): void {
    this.checked = v;
    this.checkedChange.emit(v);
  }
}
