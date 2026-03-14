import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Profile } from './profile';
import { ProfileService } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';

describe('Profile Component', () => {
    let component: Profile;
    let fixture: ComponentFixture<Profile>;
    let profileServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        profileServiceMock = {
            getProfile: vi.fn().mockReturnValue(of({ fullName: 'Test', academicHistory: [], professionalHistory: [] })),
            updateProfile: vi.fn().mockReturnValue(of({ success: true })),
            uploadPhoto: vi.fn().mockReturnValue(of({ photoPath: 'test.jpg' }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        await TestBed.configureTestingModule({
            imports: [Profile],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Profile);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load profile on init', () => {
        expect(profileServiceMock.getProfile).toHaveBeenCalled();
    });

    it('should add academic records', () => {
        const initial = component.profile.academicHistory.length;
        component.addAcademicRecord();
        expect(component.profile.academicHistory.length).toBe(initial + 1);
    });
});
