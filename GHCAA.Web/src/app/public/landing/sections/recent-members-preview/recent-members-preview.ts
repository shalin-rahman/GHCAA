import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NetworkingService } from '../../../../core/services/networking.service';
import { ImgFallbackDirective } from '../../../../common/directives/img-fallback.directive';

@Component({
    selector: 'landing-recent-members-preview',
    standalone: true,
    imports: [CommonModule, ImgFallbackDirective],
    templateUrl: './recent-members-preview.html',
    styleUrl: './recent-members-preview.scss',
})
export class LandingRecentMembersPreview implements OnInit {
    private networking = inject(NetworkingService);
    members = signal<any[]>([]);
    isVisible = signal(true);

    ngOnInit() {
        // 55.3: api/networking/search is already [AllowAnonymous] (same one the public Directory
        // page uses) — no new backend endpoint needed for this preview.
        this.networking.getRecentlyJoined(8).subscribe({
            next: (result) => this.members.set(result?.items || []),
            error: () => {
                this.members.set([]);
                this.isVisible.set(false);
            }
        });
    }
}
