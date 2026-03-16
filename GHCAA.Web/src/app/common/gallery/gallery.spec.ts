import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Gallery } from './gallery';
import { GalleryService } from '../../core/services/gallery.service';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';

describe('Gallery Component', () => {
    let component: Gallery;
    let fixture: ComponentFixture<Gallery>;
    let galleryServiceMock: any;

    beforeEach(async () => {
        galleryServiceMock = {
            getGalleries: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [Gallery],
            providers: [
                { provide: GalleryService, useValue: galleryServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Gallery);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load galleries on init', () => {
        expect(galleryServiceMock.getGalleries).toHaveBeenCalled();
    });
});
