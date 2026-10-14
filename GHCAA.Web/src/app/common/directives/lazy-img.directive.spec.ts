import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LazyImgDirective } from './lazy-img.directive';

@Component({
    standalone: true,
    imports: [LazyImgDirective],
    template: `
        <img id="plain" src="/a.jpg" appLazyImg>
        <img id="eager" src="/b.jpg" loading="eager" decoding="sync" appLazyImg>
        <img id="bound" [src]="path" appLazyImg>
        <img id="untouched" src="/c.jpg">
    `
})
class HostComponent {
    path = '/d.jpg';
}

describe('LazyImgDirective', () => {
    let fixture: ComponentFixture<HostComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({ imports: [HostComponent] }).compileComponents();
        fixture = TestBed.createComponent(HostComponent);
        fixture.detectChanges();
    });

    function imgEl(id: string): HTMLImageElement {
        return fixture.nativeElement.querySelector(`#${id}`) as HTMLImageElement;
    }

    it('sets lazy loading and async decoding', () => {
        expect(imgEl('plain').getAttribute('loading')).toBe('lazy');
        expect(imgEl('plain').getAttribute('decoding')).toBe('async');
    });

    it('keeps values the template already sets', () => {
        expect(imgEl('eager').getAttribute('loading')).toBe('eager');
        expect(imgEl('eager').getAttribute('decoding')).toBe('sync');
    });

    it('works with a bound src', () => {
        expect(imgEl('bound').getAttribute('loading')).toBe('lazy');
        expect(imgEl('bound').getAttribute('src')).toBe('/d.jpg');
    });

    it('leaves images without the directive alone', () => {
        expect(imgEl('untouched').hasAttribute('loading')).toBe(false);
    });
});
