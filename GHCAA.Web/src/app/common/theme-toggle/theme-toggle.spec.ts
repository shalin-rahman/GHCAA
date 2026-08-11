import { TestBed, ComponentFixture } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ThemeToggle } from './theme-toggle';
import { ThemeService } from '../../core/services/theme.service';
import { API_ENDPOINTS } from '../../core/constants/app.constants';

describe('ThemeToggle', () => {
    let fixture: ComponentFixture<ThemeToggle>;
    let component: ThemeToggle;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        localStorage.clear();

        TestBed.configureTestingModule({
            imports: [ThemeToggle, HttpClientTestingModule],
            providers: [ThemeService]
        });

        fixture = TestBed.createComponent(ThemeToggle);
        component = fixture.componentInstance;
        httpMock = TestBed.inject(HttpTestingController);

        // ThemeService's constructor fires loadActiveSpecialTheme().
        httpMock.expectOne(`${API_ENDPOINTS.THEMES}/active`).flush({});
        fixture.detectChanges();
    });

    afterEach(() => {
        httpMock.verify();
        localStorage.clear();
    });

    it('creates', () => {
        expect(component).toBeTruthy();
    });

    // The vitest setup does not resolve templateUrl, so the rendered DOM is empty here —
    // these drive the same handler the template's (click) is bound to.
    it('flips the theme', () => {
        const before = component.theme.theme();
        component.theme.toggleTheme();
        expect(component.theme.theme()).not.toBe(before);
    });

    it('persists the choice so it survives a reload', () => {
        component.theme.toggleTheme();
        fixture.detectChanges(); // ThemeService writes localStorage from an effect
        expect(localStorage.getItem('ghcaa_theme')).toBe(component.theme.theme());
    });

    it('labels the action the user is about to take, not the current theme', () => {
        component.theme.theme.set('light');
        expect(component.label()).toBe('Switch to dark mode');
        component.theme.theme.set('dark');
        expect(component.label()).toBe('Switch to light mode');
    });
});
