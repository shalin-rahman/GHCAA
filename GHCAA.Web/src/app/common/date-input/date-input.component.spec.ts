import { Component, WritableSignal, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppDateInputComponent } from './date-input.component';
import { OrgConfigService } from '../../core/services/org-config.service';
import { DATE_FORMATS, DATE_FORMAT_PLACEHOLDERS, DateFormat } from '../../core/constants/app.constants';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, AppDateInputComponent],
  template: `
    <form [formGroup]="form">
      <app-date-input formControlName="day" inputId="day" ariaLabel="Day"></app-date-input>
      <app-date-input formControlName="isoDay" inputId="iso-day" valueFormat="iso"></app-date-input>
      <app-date-input formControlName="start" inputId="start" withTime></app-date-input>
    </form>
  `
})
class HostComponent {
  form = new FormGroup({
    day: new FormControl<string>('', Validators.required),
    isoDay: new FormControl<string>(''),
    start: new FormControl<string>('', Validators.required)
  });
}

// 15 June 2027 in each configured format.
const SAMPLE: Record<DateFormat, string> = {
  [DATE_FORMATS.DMY]: '15-06-2027',
  [DATE_FORMATS.MDY]: '06/15/2027'
};

describe.each(Object.values(DATE_FORMATS))('AppDateInputComponent under %s', (format: DateFormat) => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;
  let dateFormat: WritableSignal<DateFormat>;

  beforeEach(async () => {
    dateFormat = signal<DateFormat>(format);
    await TestBed.configureTestingModule({
      imports: [HostComponent],
      providers: [{ provide: OrgConfigService, useValue: { dateFormat } }]
    }).compileComponents();
    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  function text(id: string): HTMLInputElement {
    return fixture.nativeElement.querySelector(`#${id}`) as HTMLInputElement;
  }

  function field(id: string): HTMLElement {
    return text(id).closest('app-date-input') as HTMLElement;
  }

  function type(id: string, value: string): void {
    const input = text(id);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function pick(id: string, iso: string): void {
    const native = field(id).querySelector('input[type="date"]') as HTMLInputElement;
    native.value = iso;
    native.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  it('takes the placeholder and pattern from the configured format', () => {
    const input = text('day');
    expect(input.placeholder).toBe(DATE_FORMAT_PLACEHOLDERS[format]);
    expect(new RegExp(`^(?:${input.getAttribute('pattern')})$`).test(SAMPLE[format])).toBe(true);
  });

  it('shows an ISO prefill in the configured format', () => {
    host.form.patchValue({ day: '2027-06-15T00:00:00', isoDay: '2027-06-15', start: '2027-06-15T10:30' });
    fixture.detectChanges();
    expect(text('day').value).toBe(SAMPLE[format]);
    expect(text('iso-day').value).toBe(SAMPLE[format]);
    expect(text('start').value).toBe(SAMPLE[format]);
    expect((field('start').querySelector('input[type="time"]') as HTMLInputElement).value).toBe('10:30');
  });

  it('keeps typed text in display mode and sends ISO in iso mode', () => {
    type('day', SAMPLE[format]);
    type('iso-day', SAMPLE[format]);
    expect(host.form.value.day).toBe(SAMPLE[format]);
    expect(host.form.value.isoDay).toBe('2027-06-15');
    expect(host.form.controls.day.valid).toBe(true);
  });

  it('flags a date in the wrong shape or an impossible day with a pattern error', () => {
    type('day', '2027/15/06');
    expect(host.form.controls.day.errors?.['pattern']).toBeTruthy();
    type('day', format === DATE_FORMATS.DMY ? '31-02-2027' : '02/31/2027');
    expect(host.form.controls.day.errors?.['pattern']).toBeTruthy();
  });

  it('writes a picked calendar day into the field and the form', () => {
    pick('day', '2027-06-15');
    expect(text('day').value).toBe(SAMPLE[format]);
    expect(host.form.value.day).toBe(SAMPLE[format]);
    expect(host.form.controls.day.touched).toBe(true);
  });

  // Regression: with datetime-local a picked day stayed empty until a time was also chosen.
  it('keeps a picked day in a date-and-time field without a time', () => {
    pick('start', '2027-06-15');
    expect(host.form.value.start).toBe('2027-06-15T00:00');
    expect(host.form.controls.start.valid).toBe(true);
    expect((field('start').querySelector('input[type="time"]') as HTMLInputElement).value).toBe('00:00');
  });

  it('combines the day and the chosen time', () => {
    pick('start', '2027-06-15');
    const time = field('start').querySelector('input[type="time"]') as HTMLInputElement;
    time.value = '18:45';
    time.dispatchEvent(new Event('input'));
    expect(host.form.value.start).toBe('2027-06-15T18:45');
  });

  it('opens the native calendar from the button', () => {
    const native = field('day').querySelector('input[type="date"]') as HTMLInputElement;
    const showPicker = vi.fn();
    (native as any).showPicker = showPicker;
    (field('day').querySelector('button') as HTMLButtonElement).click();
    expect(showPicker).toHaveBeenCalled();
  });

  it('redraws the text when the configured format changes', () => {
    type('day', SAMPLE[format]);
    const other = format === DATE_FORMATS.DMY ? DATE_FORMATS.MDY : DATE_FORMATS.DMY;
    dateFormat.set(other);
    fixture.detectChanges();
    expect(text('day').value).toBe(SAMPLE[other]);
    expect(host.form.value.day).toBe(SAMPLE[other]);
    expect(text('day').placeholder).toBe(DATE_FORMAT_PLACEHOLDERS[other]);
  });

  it('leaves an empty field to the required validator', () => {
    type('day', '');
    expect(host.form.controls.day.errors).toEqual({ required: true });
  });
});
