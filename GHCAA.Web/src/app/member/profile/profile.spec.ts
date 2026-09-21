import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { Profile } from './profile';
import { ProfileService } from '../../core/services/profile.service';
import { NotificationService } from '../../core/services/notification.service';
import { LookupService } from '../../core/services/lookup.service';
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

        notificationServiceMock = createNotificationServiceMock();

        const lookupServiceMock = {
            getOptions: vi.fn().mockReturnValue(of([])),
            getAcademicYears: vi.fn().mockReturnValue(of([]))
        };

        await TestBed.configureTestingModule({
            imports: [Profile],
            providers: [
                { provide: ProfileService, useValue: profileServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: LookupService, useValue: lookupServiceMock }
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

    describe('institutional academic record protection', () => {
        beforeEach(() => {
            component.profile.academicHistory = [
                { institutionName: 'Govt. Haraganga College', isOrgProfile: true },
                { institutionName: 'Govt. Haraganga College', isOrgProfile: false }
            ];
            component.orgConfig.config.set({
                branding: { institutionName: 'Govt. Haraganga College' }
            } as any);
        });

        it('protects only the first matching configured institution record', () => {
            expect(component.isInstitutionalAcademicRecord(0)).toBe(true);
            expect(component.isInstitutionalAcademicRecord(1)).toBe(false);
        });

        it('allows correction of a mismatched first record', () => {
            component.profile.academicHistory[0].institutionName = 'Other College';
            expect(component.isInstitutionalAcademicRecord(0)).toBe(false);
        });
    });

    describe('32.3 regression: profile response field mapping', () => {
        function loadWith(rawResponse: any) {
            TestBed.resetTestingModule();
            const mock = { getProfile: vi.fn().mockReturnValue(of(rawResponse)) };
            TestBed.configureTestingModule({
                imports: [Profile],
                providers: [
                    { provide: ProfileService, useValue: mock },
                    { provide: NotificationService, useValue: notificationServiceMock },
                    { provide: LookupService, useValue: { getOptions: vi.fn().mockReturnValue(of([])), getAcademicYears: vi.fn().mockReturnValue(of([])) } }
                ]
            });
            const f = TestBed.createComponent(Profile);
            f.detectChanges();
            return f.componentInstance;
        }

        it('surfaces flat DTO fields that are not part of the old hardcoded whitelist', () => {
            const c = loadWith({
                id: 201,
                fullName: 'Test Member',
                designation: 'Software Engineer',
                organizationName: 'Acme Corp',
                professionalSector: 'IT',
                location: 'Dhaka',
                passingYear: 2020,
                degree: 'BSc',
                subject: 'CSE',
                profileCompletionPercentage: 75,
                categoryBadge: 'Gold',
                academicHistory: [],
                professionalHistory: []
            });

            expect(c.profile.designation).toBe('Software Engineer');
            expect(c.profile.organizationName).toBe('Acme Corp');
            expect(c.profile.professionalSector).toBe('IT');
            expect(c.profile.profileCompletionPercentage).toBe(75);
            expect(c.profile.categoryBadge).toBe('Gold');
        });

        it('normalizes PascalCase keys (including the NID special case) to camelCase', () => {
            const c = loadWith({
                Id: 201,
                FullName: 'Test Member',
                NID: '1234567890',
                Designation: 'Manager',
                AcademicHistory: [],
                ProfessionalHistory: []
            });

            expect(c.profile.fullName).toBe('Test Member');
            expect(c.profile.nid).toBe('1234567890');
            expect(c.profile.designation).toBe('Manager');
        });
    });
});
