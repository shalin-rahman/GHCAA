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
            // 58.7: hide the whole section when there's nothing to show, not just on error.
            next: (result) => {
                const items = result?.items || [];
                this.members.set(items);
                this.isVisible.set(items.length > 0);
            },
            error: () => {
                this.members.set([]);
                this.isVisible.set(false);
            }
        });
    }
}
