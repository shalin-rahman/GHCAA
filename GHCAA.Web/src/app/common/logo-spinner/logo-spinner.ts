import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-logo-spinner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './logo-spinner.html',
  styleUrl: './logo-spinner.scss'
})
export class LogoSpinnerComponent {
  /** Diameter of the logo seal in pixels; the spinning ring scales with it. */
  @Input() size = 160;
  /** Shows the outward ripple pulse behind the ring; disable in tight spaces (e.g. inline buttons). */
  @Input() ripple = true;
  /** Optional caption shown under the spinner. */
  @Input() label = '';
}
