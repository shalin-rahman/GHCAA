import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CampaignService } from '../../core/services/campaign.service';
import { CampaignPledge } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { getPledgeStatusLabel, getPledgeStatusClass } from '../../core/constants/app.constants';

// TODO 37.3: a member's own pledges and giving history.
@Component({
    selector: 'app-giving',
    standalone: true,
    imports: [CommonModule, RouterLink, LogoSpinnerComponent],
    templateUrl: './giving.html',
    styleUrl: './giving.scss'
})
export class Giving implements OnInit {
    private campaignService = inject(CampaignService);
    getPledgeStatusLabel = getPledgeStatusLabel;
    getPledgeStatusClass = getPledgeStatusClass;

    loading = signal(true);
    pledges = signal<CampaignPledge[]>([]);

    ngOnInit() {
        this.campaignService.getMyPledges().subscribe({
            next: (data) => { this.pledges.set(data); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    totalGiven(): number {
        return this.pledges().reduce((sum, p) => sum + p.amountReceived, 0);
    }
}
