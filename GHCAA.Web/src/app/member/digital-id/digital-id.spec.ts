import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DigitalId } from './digital-id';
import { ProfileService } from '../../core/services/profile.service';
import { of } from 'rxjs';
import { MEMBERSHIP_TYPES } from '../../core/constants/app.constants';

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

    // 35.1 regression guard: the printed ID card once labelled MembershipType 6 as "Life",
    // a value that does not exist in GHCAA.Domain/Enums.cs, where 6 is Guest.
    it('should label the last MembershipType as Guest, never Life', () => {
        expect(component.getMembershipName(6)).toBe('Guest Member');
        expect(component.getMembershipName(6)).not.toContain('Life');
        expect(component.getMembershipName('Guest')).toBe('Guest Member');
    });

    it('should label every MembershipType ordinal from the shared constant', () => {
        expect(MEMBERSHIP_TYPES.map((_, i) => component.getMembershipName(i))).toEqual(MEMBERSHIP_TYPES);
    });
});
