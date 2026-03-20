import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProfileService } from './profile.service';
import { MemberProfile } from '../models/business.models';
import { API_ENDPOINTS } from '../constants/app.constants';
import { AuthService } from './auth.service';
import { vi } from 'vitest';

describe('ProfileService', () => {
    let service: ProfileService;
    let httpMock: HttpTestingController;

    let authServiceMock: any;

    const mockProfile: MemberProfile = {
        id: 1,
        fullName: 'Test User',
        email: 'test@test.com',
        mobileNo: '0123456789',
        status: 'Active',
        membershipType: 'Executive',
        category: 'None',
        gender: 'Male',
        bloodGroup: 'APositive',
    
        presentAddress: 'Dhaka',
        permanentAddress: 'Dhaka',
        isMobilePublic: true,
        isEmailPublic: true,
        isAddressPublic: true,
        academicHistory: [],
        professionalHistory: []
    };


    beforeEach(() => {
        authServiceMock = {
            currentUser: vi.fn().mockReturnValue({ role: 'Member' })
        };

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                ProfileService,
                { provide: AuthService, useValue: authServiceMock }
            ]
        });
        service = TestBed.inject(ProfileService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should get profile', () => {
        service.getProfile().subscribe(res => {
            expect(res).toEqual(mockProfile);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.PROFILE);
        expect(req.request.method).toBe('GET');
        req.flush(mockProfile);
    });

    it('should return mock profile for SuperAdmin', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'SuperAdmin' });
        service.getProfile().subscribe(res => {
            expect(res.fullName).toBe('System Administrator');
            expect(res.id).toBe(0);
        });
        httpMock.expectNone(API_ENDPOINTS.PROFILE);
    });

    it('should update profile', () => {
        service.updateProfile(mockProfile).subscribe(res => {
            expect(res.success).toBe(true);
        });
        const req = httpMock.expectOne(API_ENDPOINTS.PROFILE);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(mockProfile);
        req.flush({ success: true });
    });

    it('should get ID card', () => {
        service.getIDCard().subscribe(res => {
            expect(res.dataUri).toBe('data:image');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.PROFILE}/id-card`);
        expect(req.request.method).toBe('GET');
        req.flush({ dataUri: 'data:image' });
    });

    it('should get certificate', () => {
        service.getCertificate().subscribe(res => {
            expect(res.dataUri).toBe('data:image');
        });
        const req = httpMock.expectOne(`${API_ENDPOINTS.PROFILE}/certificate`);
        expect(req.request.method).toBe('GET');
        req.flush({ dataUri: 'data:image' });
    });
});
