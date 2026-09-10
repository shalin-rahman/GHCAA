import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { LoadingPanelComponent } from './loading-panel';

@Component({
  standalone: true,
  imports: [LoadingPanelComponent],
  template: '<app-loading-panel [compact]="true" label="Loading members" />',
})
class HostComponent {}

describe('LoadingPanelComponent', () => {
  it('renders an accessible compact loading status', async () => {
    const fixture: ComponentFixture<HostComponent> = TestBed.createComponent(HostComponent);
    fixture.detectChanges();

    const panel = fixture.nativeElement.querySelector('.loading-panel') as HTMLElement;
    expect(panel.getAttribute('role')).toBe('status');
    expect(panel.getAttribute('aria-busy')).toBe('true');
    expect(panel.classList.contains('loading-panel--compact')).toBe(true);
    expect(panel.textContent).toContain('Loading members');
  });
});
