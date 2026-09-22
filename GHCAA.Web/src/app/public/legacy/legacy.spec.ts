import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LegacyPage } from './legacy';
import { ArchiveService } from '../../core/services/archive.service';

describe('LegacyPage', () => {
    let fixture: ComponentFixture<LegacyPage>;
    let component: LegacyPage;
    let archiveService: { getPublicCollections: ReturnType<typeof vi.fn>; getPublicItem: ReturnType<typeof vi.fn> };

    beforeEach(async () => {
        archiveService = {
            getPublicCollections: vi.fn().mockReturnValue(of([])),
            getPublicItem: vi.fn().mockReturnValue(of({ id: 4, narrator: 'Amina' }))
        };
        await TestBed.configureTestingModule({
            imports: [LegacyPage],
            providers: [
                provideRouter([]),
                { provide: ArchiveService, useValue: archiveService },
                { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } }
            ]
        }).compileComponents();
        fixture = TestBed.createComponent(LegacyPage);
        component = fixture.componentInstance;
    });

    it('loads collections and forwards the search value', () => {
        fixture.detectChanges();
        component.search = 'founders';
        component.loadCollections();
        expect(archiveService.getPublicCollections).toHaveBeenLastCalledWith('founders');
        expect(component.loading()).toBe(false);
        expect(component.error()).toBe(false);
    });

    it('exposes a retryable error state when the public endpoint fails', () => {
        archiveService.getPublicCollections.mockReturnValueOnce(throwError(() => new Error('offline')));
        fixture.detectChanges();
        expect(component.error()).toBe(true);
        expect(component.loading()).toBe(false);
    });
});
