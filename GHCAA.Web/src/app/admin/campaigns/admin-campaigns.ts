import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CampaignService } from '../../core/services/campaign.service';
import { NotificationService } from '../../core/services/notification.service';
import { Campaign, CampaignPledge, DonorRecognitionTier } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { SearchBarComponent } from '../../common/search-bar/search-bar.component';
import { getPledgeStatusLabel, getPledgeStatusClass } from '../../core/constants/app.constants';
import { OrgConfigService } from '../../core/services/org-config.service';
import { formatCurrencyAmount } from '../../core/utils/currency.util';
import { AppCurrencyPipe } from '../../core/pipes/app-currency.pipe';

// TODO 37.3: admin — create/edit campaigns, confirm pledge receipts, manage donor tiers.
@Component({
    selector: 'app-admin-campaigns',
    standalone: true,
    imports: [CommonModule, FormsModule, LogoSpinnerComponent, PageHeaderComponent, SearchBarComponent, AppCurrencyPipe],
    templateUrl: './admin-campaigns.html',
    styleUrl: './admin-campaigns.scss'
})
export class AdminCampaigns implements OnInit {
    private campaignService = inject(CampaignService);
    private notify = inject(NotificationService);
    orgConfig = inject(OrgConfigService);
    getPledgeStatusLabel = getPledgeStatusLabel;
    getPledgeStatusClass = getPledgeStatusClass;

    loading = signal(true);
    campaigns = signal<Campaign[]>([]);
    searchTerm = signal('');

    filteredCampaigns = computed(() => {
        const term = this.searchTerm().toLowerCase();
        if (!term) return this.campaigns();
        return this.campaigns().filter(c => c.title.toLowerCase().includes(term));
    });

    showForm = signal(false);
    editingId = signal<number | null>(null);
    form = {
        title: '', slug: '', story: '', coverImagePath: '',
        targetAmount: 0, startsOn: '', endsOn: '', isActive: true, isArchived: false
    };

    selectedCampaign = signal<Campaign | null>(null);
    pledges = signal<CampaignPledge[]>([]);
    pledgesLoading = signal(false);
    saving = signal(false);
    confirmingReceipt = signal(false);

    tiers = signal<DonorRecognitionTier[]>([]);
    newTier = { name: '', minimumAmount: 0, description: '' };

    ngOnInit() {
        this.loadCampaigns();
        this.loadTiers();
    }

    private loadCampaigns() {
        this.loading.set(true);
        this.campaignService.getAllForAdmin().subscribe({
            next: (data) => { this.campaigns.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    private loadTiers() {
        this.campaignService.getTiers().subscribe({ next: (data) => this.tiers.set(data) });
    }

    openCreateForm() {
        this.editingId.set(null);
        this.form = { title: '', slug: '', story: '', coverImagePath: '', targetAmount: 0, startsOn: '', endsOn: '', isActive: true, isArchived: false };
        this.showForm.set(true);
    }

    openEditForm(c: Campaign) {
        this.editingId.set(c.id);
        this.form = {
            title: c.title,
            slug: c.slug,
            story: c.story,
            coverImagePath: c.coverImagePath || '',
            targetAmount: c.targetAmount,
            startsOn: c.startsOn ? new Date(c.startsOn).toISOString().slice(0, 10) : '',
            endsOn: c.endsOn ? new Date(c.endsOn).toISOString().slice(0, 10) : '',
            isActive: c.isActive,
            isArchived: c.isArchived ?? false
        };
        this.showForm.set(true);
    }

    submitCampaign() {
        if (this.saving()) return;
        if (!this.form.title || !this.form.slug || !this.form.story || this.form.targetAmount <= 0 || !this.form.startsOn) {
            this.notify.error('Please complete all required fields.');
            return;
        }

        const payload = {
            title: this.form.title,
            slug: this.form.slug,
            story: this.form.story,
            coverImagePath: this.form.coverImagePath || undefined,
            targetAmount: this.form.targetAmount,
            startsOn: this.form.startsOn,
            endsOn: this.form.endsOn || undefined,
            isActive: this.form.isActive
        };

        const editingId = this.editingId();
        const request = editingId
            ? this.campaignService.updateCampaign({ ...payload, id: editingId, isArchived: this.form.isArchived })
            : this.campaignService.createCampaign(payload);

        this.saving.set(true);
        request.subscribe({
            next: () => {
                this.saving.set(false);
                this.notify.success(editingId ? 'Campaign updated.' : 'Campaign created.');
                this.showForm.set(false);
                this.loadCampaigns();
            },
            error: (err) => {
                this.saving.set(false);
                this.notify.error(err?.error?.message || 'Could not save the campaign.');
            }
        });
    }

    viewPledges(c: Campaign) {
        this.selectedCampaign.set(c);
        this.pledgesLoading.set(true);
        this.campaignService.getPledgesForAdmin(c.id).subscribe({
            next: (data) => { this.pledges.set(data); this.pledgesLoading.set(false); },
            error: () => this.pledgesLoading.set(false)
        });
    }

    closeDetail() {
        this.selectedCampaign.set(null);
        this.pledges.set([]);
    }

    confirmReceipt(pledge: CampaignPledge) {
        if (this.confirmingReceipt()) return;
        const formattedAmount = formatCurrencyAmount(pledge.amount, this.orgConfig.config()?.currency);
        const amount = prompt(`Amount received for ${pledge.donorName}'s pledge of ${formattedAmount}?`, String(pledge.amount));
        if (!amount) return;
        const parsed = parseFloat(amount);
        if (isNaN(parsed) || parsed <= 0) {
            this.notify.error('Enter a valid amount.');
            return;
        }

        this.confirmingReceipt.set(true);
        this.campaignService.confirmReceipt(pledge.id, parsed).subscribe({
            next: () => {
                this.confirmingReceipt.set(false);
                this.notify.success('Receipt confirmed — recorded in the financial ledger.');
                const campaign = this.selectedCampaign();
                if (campaign) this.viewPledges(campaign);
                this.loadCampaigns();
            },
            error: () => {
                this.confirmingReceipt.set(false);
                this.notify.error('Could not confirm receipt.');
            }
        });
    }

    addTier() {
        if (!this.newTier.name || this.newTier.minimumAmount < 0) {
            this.notify.error('Enter a tier name and a minimum amount.');
            return;
        }
        this.campaignService.createTier({ ...this.newTier, description: this.newTier.description || undefined }).subscribe({
            next: () => {
                this.notify.success('Donor tier added.');
                this.newTier = { name: '', minimumAmount: 0, description: '' };
                this.loadTiers();
            },
            error: () => this.notify.error('Could not add the tier.')
        });
    }
}
