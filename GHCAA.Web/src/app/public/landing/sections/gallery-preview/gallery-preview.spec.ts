import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LandingGalleryPreview } from './gallery-preview';
import { GalleryService } from '../../../../core/services/gallery.service';
import { of, throwError } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { EventGallery } from '../../../../core/models/business.models';

describe('LandingGalleryPreview Component', () => {
    let component: LandingGalleryPreview;
    let fixture: ComponentFixture<LandingGalleryPreview>;
    let galleryServiceMock: any;

    const albumWithPhotos = (id: number, photoCount: number): EventGallery => ({
        id,
        title: `Album ${id}`,
        photos: Array.from({ length: photoCount }, (_, i) => ({ id: id * 100 + i, photoPath: `/p/${id}-${i}.jpg` })) as any
    } as any);

    beforeEach(async () => {
        galleryServiceMock = {
            getGalleries: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [LandingGalleryPreview, RouterTestingModule],
            providers: [
                { provide: GalleryService, useValue: galleryServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(LandingGalleryPreview);
        component = fixture.componentInstance;
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it('should create', () => {
        fixture.detectChanges();
        expect(component).toBeTruthy();
    });

    it('should call getGalleries with the silent flag', () => {
        fixture.detectChanges();
        expect(galleryServiceMock.getGalleries).toHaveBeenCalledWith(true);
    });

    it('should only include albums that have at least one photo, capped at 8', () => {
        const withPhotos = Array.from({ length: 6 }, (_, i) => albumWithPhotos(i + 1, 1));
        const empty = { id: 100, title: 'Empty', photos: [] } as any;
        galleryServiceMock.getGalleries.mockReturnValue(of([...withPhotos, empty, ...Array.from({ length: 5 }, (_, i) => albumWithPhotos(i + 10, 2))]));

        fixture.detectChanges();

        const albums = component.albums();
        expect(albums.length).toBe(8);
        expect(albums.every(a => a.photos.length > 0)).toBe(true);
    });

    it('should hide section and clear galleries on error', () => {
        galleryServiceMock.getGalleries.mockReturnValue(throwError(() => new Error('Server error')));

        fixture.detectChanges();

        expect(component.isVisible()).toBe(false);
        expect(component.galleries()).toEqual([]);
    });

    it('currentPhoto should return the photo at the current slide index, wrapping by photo count', () => {
        const album = albumWithPhotos(1, 3);
        galleryServiceMock.getGalleries.mockReturnValue(of([album]));
        fixture.detectChanges();

        expect(component.currentPhoto(album)?.photoPath).toBe('/p/1-0.jpg');
    });

    it('currentPhoto should return undefined for an album with no photos', () => {
        const empty = { id: 1, title: 'Empty', photos: [] } as any;
        expect(component.currentPhoto(empty)).toBeUndefined();
    });

    it('should cycle slideIndex forward for multi-photo albums only, on a timer', () => {
        vi.useFakeTimers();
        const multi = albumWithPhotos(1, 3);
        const single = albumWithPhotos(2, 1);
        galleryServiceMock.getGalleries.mockReturnValue(of([multi, single]));

        fixture.detectChanges();

        expect(component.currentPhoto(multi)?.photoPath).toBe('/p/1-0.jpg');
        expect(component.currentPhoto(single)?.photoPath).toBe('/p/2-0.jpg');

        vi.advanceTimersByTime(3000);

        expect(component.currentPhoto(multi)?.photoPath).toBe('/p/1-1.jpg');
        // Single-photo album never advances since count > 1 gates the index bump.
        expect(component.currentPhoto(single)?.photoPath).toBe('/p/2-0.jpg');
    });

    it('should not start a cycling timer when no album has more than one photo', () => {
        vi.useFakeTimers();
        const setIntervalSpy = vi.spyOn(global, 'setInterval');
        galleryServiceMock.getGalleries.mockReturnValue(of([albumWithPhotos(1, 1)]));

        fixture.detectChanges();

        expect(setIntervalSpy).not.toHaveBeenCalled();
    });

    it('should clear the cycling timer on destroy', () => {
        vi.useFakeTimers();
        const clearIntervalSpy = vi.spyOn(global, 'clearInterval');
        galleryServiceMock.getGalleries.mockReturnValue(of([albumWithPhotos(1, 2)]));

        fixture.detectChanges();
        component.ngOnDestroy();

        expect(clearIntervalSpy).toHaveBeenCalled();
    });

    it('viewFull should open the given path in a new tab', () => {
        fixture.detectChanges();
        const openSpy = vi.spyOn(window, 'open').mockImplementation(() => null);

        component.viewFull('/some/photo.jpg');

        expect(openSpy).toHaveBeenCalledWith('/some/photo.jpg', '_blank', 'noopener');
    });
});
