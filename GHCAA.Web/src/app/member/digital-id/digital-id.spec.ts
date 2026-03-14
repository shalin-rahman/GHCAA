import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DigitalId } from './digital-id';
import { ProfileService } from '../../core/services/profile.service';
import { of } from 'rxjs';

describe('DigitalId Component', () => {
    let component: DigitalId;
    let fixture: ComponentFixture<DigitalId>;
    let profileServiceMock: any;

    beforeEach(async () => {
        profileServiceMock = {
            getProfile: vi.fn().mockReturnValue(of({ fullName: 'Test Member', memberId: 'M123' }))
        };

        await TestBed.configureTestingModule({
            imports: [DigitalId],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(DigitalId);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load profile for ID card', () => {
        expect(profileServiceMock.getProfile).toHaveBeenCalled();
    });
});
