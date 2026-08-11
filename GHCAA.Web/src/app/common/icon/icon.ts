import { Component, Input } from '@angular/core';

/**
 * Central inline-SVG icon system (TODO 30.3).
 *
 * One shared component renders a small, hand-drawn line-icon set from a semantic
 * `name` key. Because every path/shape uses `stroke="currentColor"` (set on the root
 * <svg>), icons automatically inherit the surrounding text color — correct in both
 * light and dark theme with zero extra CSS, unlike emoji glyphs or Font Awesome's
 * own font/color handling.
 *
 * Usage: <app-icon name="dashboard"></app-icon>
 */
@Component({
  selector: 'app-icon',
  standalone: true,
  templateUrl: './icon.html'
})
export class Icon {
  @Input() name!: string;
  @Input() size = 20;
  @Input() strokeWidth = 2;
}
