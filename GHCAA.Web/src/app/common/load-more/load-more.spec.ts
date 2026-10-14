import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { LoadMoreComponent } from './load-more';

// NFR-P1: list pages fetch the next page only when the reader gets near the end.

@Component({
  standalone: true,
  imports: [LoadMoreComponent],
  template: '<app-load-more [hasMore]="hasMore()" [loading]="loading()" (more)="calls = calls + 1" />',
})
class HostComponent {
  hasMore = signal(true);
  loading = signal(false);
  calls = 0;
}

describe('LoadMoreComponent', () => {
  let fixture: ComponentFixture<HostComponent>;
  let trigger: ((entries: Partial<IntersectionObserverEntry>[]) => void) | undefined;
  const disconnect = vi.fn();
  const observe = vi.fn();

  beforeEach(() => {
    trigger = undefined;
    disconnect.mockClear();
    observe.mockClear();
    vi.stubGlobal('IntersectionObserver', class {
      constructor(cb: (entries: Partial<IntersectionObserverEntry>[]) => void) { trigger = cb; }
      observe() { observe(); }
      unobserve() {}
      disconnect() { disconnect(); }
    });
    fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
  });

  afterEach(() => vi.unstubAllGlobals());

  function button(): HTMLButtonElement | null {
    return fixture.nativeElement.querySelector('button');
  }

  it('shows the button and emits on click while more pages exist', () => {
    button()!.click();
    expect(fixture.componentInstance.calls).toBe(1);
  });

  it('emits when it scrolls into view', () => {
    trigger!([{ isIntersecting: true }]);
    expect(fixture.componentInstance.calls).toBe(1);
  });

  it('does not emit while a page is loading or when nothing is left', () => {
    fixture.componentInstance.loading.set(true);
    fixture.detectChanges();
    trigger!([{ isIntersecting: true }]);
    expect(button()).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Loading more');

    fixture.componentInstance.loading.set(false);
    fixture.componentInstance.hasMore.set(false);
    fixture.detectChanges();
    trigger!([{ isIntersecting: true }]);
    expect(button()).toBeNull();
    expect(fixture.componentInstance.calls).toBe(0);
  });

  it('checks again when a page finishes loading, so a footer still in view asks for the next', () => {
    fixture.componentInstance.loading.set(true);
    fixture.detectChanges();
    expect(observe).toHaveBeenCalledTimes(1);

    fixture.componentInstance.loading.set(false);
    fixture.detectChanges();
    expect(observe).toHaveBeenCalledTimes(2);
  });

  it('stops observing on destroy', () => {
    fixture.destroy();
    expect(disconnect).toHaveBeenCalled();
  });
});
