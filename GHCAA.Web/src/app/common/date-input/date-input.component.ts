import {
  Component,
  ElementRef,
  Input,
  ViewChild,
  booleanAttribute,
  computed,
  effect,
  forwardRef,
  inject,
  signal,
  untracked
} from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator
} from '@angular/forms';
import { Icon } from '../icon/icon';
import { OrgConfigService } from '../../core/services/org-config.service';
import { DateFormat } from '../../core/constants/app.constants';
import {
  dateInputPattern,
  dateInputPlaceholder,
  parseDisplayDate,
  toDisplayDate,
  toWireDate
} from '../../core/utils/date.util';

/** 'display' keeps the typed text in the org format; 'iso' emits yyyy-MM-dd. */
export type DateInputValueFormat = 'display' | 'iso';

const ISO_DATE_TIME = /^(\d{4}-\d{2}-\d{2})(?:[T ](\d{2}:\d{2}))?/;

/**
 * Date field that shows and accepts the date format set in OrgConfig, with a calendar button.
 *
 * A native datetime-local input keeps an empty value until both the day and the time are set,
 * so a picked day looked like it did not take. Here the day and the time are separate, and a
 * picked day with no time gets 00:00.
 *
 * Usage:
 *   <app-date-input formControlName="effectiveDate" inputId="fee-from"></app-date-input>
 *   <app-date-input [(ngModel)]="form.startsOn" name="startsOn" valueFormat="iso"></app-date-input>
 *   <app-date-input formControlName="startDate" withTime></app-date-input>
 *
 * With withTime the value is 'yyyy-MM-ddTHH:mm', the same shape a datetime-local input gives.
 * A bad date sets the 'pattern' error, so existing errors?.['pattern'] messages keep working.
 */
@Component({
  selector: 'app-date-input',
  exportAs: 'appDateInput',
  standalone: true,
  imports: [Icon],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AppDateInputComponent), multi: true },
    { provide: NG_VALIDATORS, useExisting: forwardRef(() => AppDateInputComponent), multi: true }
  ],
  template: `
    <div class="dpi" [class.dpi-with-time]="withTime">
      <div class="dpi-field">
        <input
          type="text"
          class="dpi-text"
          [class]="inputClass"
          autocomplete="off"
          [attr.id]="inputId || null"
          [attr.aria-label]="ariaLabel || null"
          [attr.aria-describedby]="ariaDescribedby || null"
          [attr.aria-required]="required || null"
          [attr.pattern]="pattern()"
          [placeholder]="placeholder()"
          [value]="text()"
          [readOnly]="readonly"
          [disabled]="disabled()"
          (input)="onTextInput($any($event.target).value)"
          (blur)="onTouched()" />
        @if (!readonly) {
          <button
            type="button"
            class="dpi-btn"
            [disabled]="disabled()"
            [attr.aria-label]="'Open calendar' + (ariaLabel ? ' for ' + ariaLabel : '')"
            (click)="openPicker()">
            <app-icon name="calendar" [size]="16"></app-icon>
          </button>
        }
        <input
          #nativePicker
          type="date"
          class="dpi-native"
          tabindex="-1"
          aria-hidden="true"
          [value]="isoDate()"
          (change)="onPicked($any($event.target).value)" />
      </div>
      @if (withTime) {
        <input
          type="time"
          class="dpi-time"
          [class]="inputClass"
          [attr.aria-label]="(ariaLabel || 'Date') + ' time'"
          [value]="time()"
          [readOnly]="readonly"
          [disabled]="disabled()"
          (input)="onTimeInput($any($event.target).value)"
          (blur)="onTouched()" />
      }
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }
      .dpi {
        display: flex;
        gap: 0.5rem;
        align-items: stretch;
      }
      .dpi-field {
        position: relative;
        flex: 1 1 auto;
        min-width: 0;
      }
      :host .dpi-text {
        width: 100%;
        padding-right: 2.5rem;
      }
      .dpi-btn {
        position: absolute;
        top: 50%;
        right: 0.5rem;
        transform: translateY(-50%);
        display: inline-flex;
        align-items: center;
        justify-content: center;
        padding: 0.25rem;
        border: none;
        border-radius: var(--radius-sm);
        background: transparent;
        color: var(--text-muted);
        cursor: pointer;
      }
      .dpi-btn:hover:not(:disabled),
      .dpi-btn:focus-visible {
        color: var(--accent-color);
      }
      .dpi-btn:disabled {
        cursor: not-allowed;
        opacity: 0.5;
      }
      /* Kept rendered (not display:none) because showPicker() anchors the popup to it. */
      :host .dpi-native {
        position: absolute;
        right: 0;
        bottom: 0;
        width: 1px;
        height: 1px;
        margin: 0;
        padding: 0;
        border: 0;
        opacity: 0;
        pointer-events: none;
      }
      :host .dpi-time {
        flex: 0 0 9rem;
        width: 9rem;
      }
      @media (max-width: 480px) {
        .dpi-with-time {
          flex-wrap: wrap;
        }
        :host .dpi-time {
          flex: 1 1 100%;
          width: 100%;
        }
      }
    `
  ]
})
export class AppDateInputComponent implements ControlValueAccessor, Validator {
  @Input() valueFormat: DateInputValueFormat = 'display';
  @Input({ transform: booleanAttribute }) withTime = false;
  @Input({ transform: booleanAttribute }) readonly = false;
  @Input({ transform: booleanAttribute }) required = false;
  @Input() inputId = '';
  @Input() ariaLabel = '';
  @Input() ariaDescribedby = '';
  @Input() inputClass = '';

  @ViewChild('nativePicker', { static: true }) private nativePicker!: ElementRef<HTMLInputElement>;

  private readonly orgConfig = inject(OrgConfigService);
  readonly format = this.orgConfig.dateFormat;
  readonly placeholder = computed(() => dateInputPlaceholder(this.format()));
  readonly pattern = computed(() => dateInputPattern(this.format()));

  readonly text = signal('');
  readonly time = signal('');
  readonly disabled = signal(false);
  readonly isoDate = computed(() => {
    const date = parseDisplayDate(this.text(), this.format());
    return date ? toWireDate(date) ?? '' : '';
  });

  private onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};
  private onValidatorChange: () => void = () => {};
  private shownFormat: DateFormat = this.format();
  private lastEmitted = '';

  constructor() {
    // The config can load after the form is filled. Redraw the text in the new format.
    effect(() => {
      const format = this.format();
      untracked(() => {
        if (format === this.shownFormat) return;
        this.shownFormat = format;
        const date = parseDisplayDate(this.text());
        if (date) {
          this.text.set(toDisplayDate(date, format));
          this.emitIfChanged();
        }
        this.onValidatorChange();
      });
    });
  }

  writeValue(value: unknown): void {
    let datePart: unknown = value;
    let time = '';
    if (typeof value === 'string') {
      const iso = ISO_DATE_TIME.exec(value.trim());
      if (iso) {
        datePart = iso[1];
        time = iso[2] ?? '';
      }
    } else if (value instanceof Date && !isNaN(value.getTime())) {
      time = `${String(value.getHours()).padStart(2, '0')}:${String(value.getMinutes()).padStart(2, '0')}`;
    }
    const date = datePart ? parseDisplayDate(datePart) : null;
    this.shownFormat = this.format();
    this.text.set(date ? toDisplayDate(date, this.shownFormat) : datePart ? String(datePart) : '');
    this.time.set(this.withTime && date ? time || '00:00' : '');
    this.lastEmitted = this.modelValue();
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  registerOnValidatorChange(fn: () => void): void {
    this.onValidatorChange = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  validate(_control: AbstractControl): ValidationErrors | null {
    const text = this.text().trim();
    if (!text || parseDisplayDate(text, this.format())) return null;
    return { pattern: { requiredPattern: this.pattern(), actualValue: text } };
  }

  onTextInput(value: string): void {
    this.text.set(value);
    this.defaultTime();
    this.emit();
  }

  onTimeInput(value: string): void {
    this.time.set(value);
    this.emit();
  }

  onPicked(iso: string): void {
    if (!iso) return;
    this.text.set(toDisplayDate(iso, this.format()));
    this.defaultTime();
    this.emit();
    this.onTouched();
  }

  openPicker(): void {
    if (this.readonly || this.disabled()) return;
    const picker = this.nativePicker.nativeElement;
    try {
      picker.showPicker();
    } catch {
      // Older browsers have no showPicker(); a click opens the picker in most of them.
      picker.click();
    }
  }

  private defaultTime(): void {
    if (this.withTime && !this.time() && parseDisplayDate(this.text(), this.format())) {
      this.time.set('00:00');
    }
  }

  private modelValue(): string {
    const text = this.text().trim();
    if (!text) return '';
    if (this.valueFormat === 'display' && !this.withTime) return text;
    const date = parseDisplayDate(text, this.format());
    // Hand back the raw text so the control holds something and the pattern error shows.
    if (!date) return text;
    const iso = toWireDate(date) ?? '';
    return this.withTime ? `${iso}T${this.time() || '00:00'}` : iso;
  }

  private emit(): void {
    this.lastEmitted = this.modelValue();
    this.onChange(this.lastEmitted);
  }

  private emitIfChanged(): void {
    if (this.modelValue() !== this.lastEmitted) this.emit();
  }
}
