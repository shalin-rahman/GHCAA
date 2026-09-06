import { ComponentFixture, TestBed } from '@angular/core/testing';
import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { MemberRequests } from './requests';
import { FamilyLinkService } from '../../core/services/family-link.service';
import { MentorshipService } from '../../core/services/mentorship.service';
import { NetworkingService } from '../../core/services/networking.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { of, throwError } from 'rxjs';

describe('MemberRequests Component', () => {
    let component: MemberRequests;
    let fixture: ComponentFixture<MemberRequests>;
    let familyLinkServiceMock: any;
    let mentorshipServiceMock: any;
    let networkingServiceMock: any;
    let notificationServiceMock: any;

    beforeEach(async () => {
        familyLinkServiceMock = {
            getReceived: vi.fn().mockReturnValue(of([])),
            getSent: vi.fn().mockReturnValue(of([])),
            getFamily: vi.fn().mockReturnValue(of([])),
            send: vi.fn().mockReturnValue(of({})),
            respond: vi.fn().mockReturnValue(of({})),
            cancel: vi.fn().mockReturnValue(of({})),
            remove: vi.fn().mockReturnValue(of({})),
            search: vi.fn().mockReturnValue(of([]))
        };
        mentorshipServiceMock = {
            getReceived: vi.fn().mockReturnValue(of([])),
            getSent: vi.fn().mockReturnValue(of([])),
            send: vi.fn().mockReturnValue(of({})),
            respond: vi.fn().mockReturnValue(of({}))
        };
        networkingServiceMock = {
            searchMembers: vi.fn().mockReturnValue(of({ items: [] }))
        };
        notificationServiceMock = createNotificationServiceMock();
        const authServiceMock = { currentUser: vi.fn().mockReturnValue({ memberId: 1 }) };

        await TestBed.configureTestingModule({
            imports: [MemberRequests],
            providers: [
                { provide: FamilyLinkService, useValue: familyLinkServiceMock },
                { provide: MentorshipService, useValue: mentorshipServiceMock },
                { provide: NetworkingService, useValue: networkingServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: AuthService, useValue: authServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(MemberRequests);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load both received and sent lists for both tabs on init', () => {
        expect(familyLinkServiceMock.getReceived).toHaveBeenCalled();
        expect(familyLinkServiceMock.getSent).toHaveBeenCalled();
        expect(mentorshipServiceMock.getReceived).toHaveBeenCalled();
        expect(mentorshipServiceMock.getSent).toHaveBeenCalled();
    });

    it('should default to the family tab', () => {
        expect(component.activeTab()).toBe('family');
    });

    it('switching tabs closes an open send form', () => {
        component.openSendForm();
        expect(component.showSendForm()).toBe(true);
        component.setTab('mentorship');
        expect(component.showSendForm()).toBe(false);
        expect(component.activeTab()).toBe('mentorship');
    });

    it('sendRequest warns instead of calling the API when nothing is selected', () => {
        component.selectedMember = null;
        component.sendRequest();
        expect(notificationServiceMock.warning).toHaveBeenCalled();
        expect(familyLinkServiceMock.send).not.toHaveBeenCalled();
    });

    it('sendRequest on the family tab calls FamilyLinkService.send with the selected member', () => {
        component.activeTab.set('family');
        component.selectedMember = { membershipNumber: 'GHC-2024-0001', fullName: 'Test Member' };
        component.relationship = 'Sibling';

        component.sendRequest();

        expect(familyLinkServiceMock.send).toHaveBeenCalledWith('GHC-2024-0001', 'Sibling', undefined);
    });

    it('sendRequest on the mentorship tab calls MentorshipService.send with the selected member id', () => {
        component.activeTab.set('mentorship');
        component.selectedMember = { id: 42, fullName: 'Test Mentor' };

        component.sendRequest();

        expect(mentorshipServiceMock.send).toHaveBeenCalledWith(42, undefined, undefined);
    });

    it('respondFamily reloads the lists after a successful response', () => {
        component.respondFamily(5, true);
        expect(familyLinkServiceMock.respond).toHaveBeenCalledWith(5, true);
        expect(notificationServiceMock.success).toHaveBeenCalled();
    });

    it('respondFamily shows an error notification when the API call fails', () => {
        familyLinkServiceMock.respond.mockReturnValue(throwError(() => new Error('boom')));
        component.respondFamily(5, true);
        expect(notificationServiceMock.error).toHaveBeenCalled();
    });
});
