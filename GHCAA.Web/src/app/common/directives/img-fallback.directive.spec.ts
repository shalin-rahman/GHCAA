import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImgFallbackDirective } from './img-fallback.directive';

@Component({
    standalone: true,
    imports: [ImgFallbackDirective],
    template: `
        <img id="default" src="/broken.jpg" appImgFallback>
        <img id="custom" src="/broken.jpg" [appImgFallback]="'/assets/placeholders/avatar-placeholder.svg'">
    `
})
class HostComponent {}

describe('ImgFallbackDirective', () => {
    let fixture: ComponentFixture<HostComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({ imports: [HostComponent] }).compileComponents();
        fixture = TestBed.createComponent(HostComponent);
        fixture.detectChanges();
    });

    function imgEl(id: string): HTMLImageElement {
        return fixture.nativeElement.querySelector(`#${id}`) as HTMLImageElement;
    }

    it('swaps to the default placeholder on error when no fallback is given', () => {
        const img = imgEl('default');
        img.dispatchEvent(new Event('error'));
        expect(img.src).toContain('/assets/placeholders/image-placeholder.svg');
    });

    it('swaps to the provided fallback when one is given', () => {
        const img = imgEl('custom');
        img.dispatchEvent(new Event('error'));
        expect(img.src).toContain('/assets/placeholders/avatar-placeholder.svg');
    });

    it('only swaps once, guarding against a placeholder that itself 404s', () => {
        const img = imgEl('default');
        img.dispatchEvent(new Event('error'));
        img.src = '/still-broken.jpg';
        img.dispatchEvent(new Event('error'));
        expect(img.src).toContain('/still-broken.jpg');
    });
});
