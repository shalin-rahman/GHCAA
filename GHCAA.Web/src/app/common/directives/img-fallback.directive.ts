import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';

// 30.9: ROOT CAUSE fix for recurring "cover image missing / images not showing" reports —
// no <img> in the app had an error fallback, so a 404/missing photo path rendered the
// browser's broken-image icon. Apply `appImgFallback` (optionally with a per-context
// placeholder path via the input) to every <img> that renders a user/content-supplied path.
const DEFAULT_FALLBACK = '/assets/placeholders/image-placeholder.svg';

@Directive({
  selector: 'img[appImgFallback]',
  standalone: true
})
export class ImgFallbackDirective {
  private el = inject(ElementRef<HTMLImageElement>);

  /** Placeholder to swap to on error; defaults to a generic image icon if not supplied. */
  @Input('appImgFallback') fallbackSrc = '';

  // Guards against an infinite error loop if the placeholder itself ever fails to load.
  private swapped = false;

  @HostListener('error')
  onError(): void {
    if (this.swapped) return;
    this.swapped = true;
    this.el.nativeElement.src = this.fallbackSrc || DEFAULT_FALLBACK;
  }
}
