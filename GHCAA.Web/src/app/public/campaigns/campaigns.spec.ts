import { createNotificationServiceMock } from '../../core/testing/testing-utils';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Campaigns } from './campaigns';
import { CampaignService } from '../../core/services/campaign.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { of } from 'rxjs';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { Campaign, CreatePledgePayload } from '../../core/models/business.models';

// TODO 37.3: fundraising campaigns + donor honour roll.
describe('Campaigns Component', () => {
    let component: Campaigns;
    let fixture: ComponentFixture<Campaigns>;
    let campaignServiceMock: any;
    let notificationServiceMock: any;

    const buildCampaign = (overrides: Partial<Campaign> = {}): Campaign => ({
        id: 1,
        title: 'New Library Wing',
        slug: 'new-library-wing',
        story: 'Help us build it.',
        targetAmount: 1000,
        amountReceived: 0,
        startsOn: new Date().toISOString(),
        isActive: true,
        ...overrides
    });

    beforeEach(async () => {
        campaignServiceMock = {
            getPublicCampaigns: vi.fn().mockReturnValue(of([])),
            getBySlug: vi.fn().mockReturnValue(of(buildCampaign())),
            getHonourRoll: vi.fn().mockReturnValue(of({ targetAmount: 1000, totalReceived: 0, progressPercent: 0, donorCount: 0, tiers: [], untiered: [] })),
            createPledge: vi.fn().mockReturnValue(of({}))
        };
        notificationServiceMock = createNotificationServiceMock();

        // A slug is present so the component loads the detail view — submitPledge() needs
        // this.slug() set, the same way it would be on the real /campaigns/:slug route.
        const activatedRouteMock = { snapshot: { paramMap: { get: vi.fn().mockReturnValue('new-library-wing') } } };
        const authServiceMock = { currentUser: vi.fn().mockReturnValue(null) };

        await TestBed.configureTestingModule({
            imports: [Campaigns],
            providers: [
                provideRouter([]),
                { provide: CampaignService, useValue: campaignServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock },
                { provide: AuthService, useValue: authServiceMock }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Campaigns);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        fixture.detectChanges();
        expect(component).toBeTruthy();
    });

    describe('progressPercent (progress-bar arithmetic)', () => {
        it('computes the raised/target ratio as a whole-number percentage', () => {
            fixture.detectChanges();
            const campaign = buildCampaign({ amountReceived: 250, targetAmount: 1000 });
            expect(component.progressPercent(campaign)).toBe(25);
        });

        it('clamps at 100 when a campaign has been over-funded', () => {
            fixture.detectChanges();
            const campaign = buildCampaign({ amountReceived: 1500, targetAmount: 1000 });
            expect(component.progressPercent(campaign)).toBe(100);
        });

        it('returns 0 rather than dividing by zero when there is no target', () => {
            fixture.detectChanges();
            const campaign = buildCampaign({ amountReceived: 500, targetAmount: 0 });
            expect(component.progressPercent(campaign)).toBe(0);
        });
    });

    describe('anonymous-checkbox behaviour', () => {
        it('sends isAnonymous:true through to the pledge payload when the box is checked', () => {
            fixture.detectChanges();
            component.pledgeAmount = 500;
            component.donorName = 'Karim Rahman';
            component.isAnonymous = true;

            component.submitPledge();

            const dto: CreatePledgePayload = campaignServiceMock.createPledge.mock.calls[0][1];
            expect(dto.isAnonymous).toBe(true);
            // The name is still sent to the server for the admin's own records — anonymity is a
            // display rule enforced by the honour-roll projection, not a refusal to collect it.
            expect(dto.donorName).toBe('Karim Rahman');
        });

        it('sends isAnonymous:false by default', () => {
            fixture.detectChanges();
            component.pledgeAmount = 500;
            component.donorName = 'Anisur Islam';

            component.submitPledge();

            const dto: CreatePledgePayload = campaignServiceMock.createPledge.mock.calls[0][1];
            expect(dto.isAnonymous).toBe(false);
        });

        it('refuses to submit with no amount', () => {
            fixture.detectChanges();
            component.pledgeAmount = 0;
            component.donorName = 'Anisur Islam';

            component.submitPledge();

            expect(campaignServiceMock.createPledge).not.toHaveBeenCalled();
            expect(notificationServiceMock.error).toHaveBeenCalled();
        });
    });
});
