import { Directive, ElementRef, inject } from '@angular/core';

// Repeated list and grid images load on scroll. Do not use it on hero, logo or header images.
// The attributes are set in the constructor so they are in place before the src binding runs.
@Directive({
  selector: 'img[appLazyImg]',
  standalone: true
})
export class LazyImgDirective {
  constructor() {
    const img = inject<ElementRef<HTMLImageElement>>(ElementRef).nativeElement;
    if (!img.hasAttribute('loading')) img.setAttribute('loading', 'lazy');
    if (!img.hasAttribute('decoding')) img.setAttribute('decoding', 'async');
  }
}
