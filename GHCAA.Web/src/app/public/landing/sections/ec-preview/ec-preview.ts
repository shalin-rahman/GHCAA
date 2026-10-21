import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../../../core/services/networking.service';
import { getCurrentECLabel } from '../../../../core/constants/app.constants';
import { formatPeriodRange } from '../../../../core/utils/date.util';
import { ImgFallbackDirective } from '../../../../common/directives/img-fallback.directive';
import { LazyImgDirective } from '../../../../common/directives/lazy-img.directive';

@Component({
    selector: 'landing-ec-preview',
    standalone: true,
    imports: [CommonModule, ImgFallbackDirective, LazyImgDirective],
    templateUrl: './ec-preview.html',
    styleUrl: './ec-preview.scss',
})
export class LandingEcPreview implements OnInit {
    private networking = inject(NetworkingService);
    committee = signal<any[]>([]);
    isVisible = signal(true);
    activePeriodTitle = signal<string | null>(null);
    activePeriodDateRange = signal<string | null>(null);

    ngOnInit() {
        this.networking.getCommittee({}, true).subscribe({
            next: (data) => {
                // Members with no committee record have no label and are left out.
                const validCommittee = (data || []).filter((m: any) => !!getCurrentECLabel(m.ecHistory));
                this.committee.set(validCommittee);
            },
            error: () => {
                this.committee.set([]);
                this.isVisible.set(false);
            }
        });

        this.networking.getPeriods().subscribe({
            next: (periods) => {
                const active = (periods || []).find((p: any) => p.isActive);
                this.activePeriodTitle.set(active?.title ?? null);
                this.activePeriodDateRange.set(active?.startDate ? formatPeriodRange(active) : null);
            },
            error: () => {
                this.activePeriodTitle.set(null);
                this.activePeriodDateRange.set(null);
            }
        });
    }

    getRoleName(member: any) {
        return getCurrentECLabel(member.ecHistory);
    }
}


