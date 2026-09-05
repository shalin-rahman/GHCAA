import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignService } from '../../core/services/campaign.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { Campaign, CampaignHonourRoll, CreatePledgePayload } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

// TODO 37.3: fundraising campaigns + donor honour roll. One component for both the list
// (/campaigns) and a single campaign's page (/campaigns/:slug), same convention the Events
// component already uses for its own list/:id routes.
@Component({
    selector: 'app-campaigns',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, LogoSpinnerComponent],
    templateUrl: './campaigns.html',
    styleUrl: './campaigns.scss'
})
export class Campaigns implements OnInit {
    private campaignService = inject(CampaignService);
    private route = inject(ActivatedRoute);
    private notify = inject(NotificationService);
    private auth = inject(AuthService);

    loading = signal(true);
    slug = signal<string | null>(null);

    campaigns = signal<Campaign[]>([]);
    campaign = signal<Campaign | null>(null);
    honourRoll = signal<CampaignHonourRoll | null>(null);

    isMember = () => !!this.auth.currentUser();

    pledgeAmount = 0;
    donorName = '';
    donorEmail = '';
    donorPhone = '';
    isAnonymous = false;
    message = '';
    submitting = signal(false);

    ngOnInit() {
        const slug = this.route.snapshot.paramMap.get('slug');
        this.slug.set(slug);
        if (slug) {
            this.loadDetail(slug);
        } else {
            this.loadList();
        }
    }

    private loadList() {
        this.loading.set(true);
        this.campaignService.getPublicCampaigns().subscribe({
            next: (data) => { this.campaigns.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    private loadDetail(slug: string) {
        this.loading.set(true);
        this.campaignService.getBySlug(slug).subscribe({
            next: (c) => {
                this.campaign.set(c);
                this.campaignService.getHonourRoll(slug).subscribe({
                    next: (roll) => { this.honourRoll.set(roll); this.loading.set(false); },
                    error: () => this.loading.set(false)
                });
            },
            error: () => this.loading.set(false)
        });
    }

    progressPercent(c: Campaign): number {
        if (!c.targetAmount) return 0;
        return Math.min(100, Math.round((c.amountReceived / c.targetAmount) * 100));
    }

    submitPledge() {
        const slug = this.slug();
        if (!slug || this.pledgeAmount <= 0) {
            this.notify.error('Enter a pledge amount greater than zero.');
            return;
        }
        if (!this.isMember() && !this.donorName.trim()) {
            this.notify.error('Please enter your name.');
            return;
        }

        const dto: CreatePledgePayload = {
            amount: this.pledgeAmount,
            donorName: this.donorName || undefined,
            donorEmail: this.donorEmail || undefined,
            donorPhone: this.donorPhone || undefined,
            isAnonymous: this.isAnonymous,
            message: this.message || undefined
        };

        this.submitting.set(true);
        this.campaignService.createPledge(slug, dto).subscribe({
            next: () => {
                this.notify.success('Thank you — your pledge has been recorded. An admin will confirm receipt once payment is received.');
                this.pledgeAmount = 0;
                this.donorName = '';
                this.donorEmail = '';
                this.donorPhone = '';
                this.isAnonymous = false;
                this.message = '';
                this.submitting.set(false);
                this.loadDetail(slug);
            },
            error: () => this.submitting.set(false)
        });
    }
}
