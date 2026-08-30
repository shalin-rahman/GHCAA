import { Component, ViewEncapsulation, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification.service';
import { LandingBanner } from './sections/banner/banner';
import { LandingPurpose } from './sections/purpose/purpose';
import { LandingJobsPreview } from './sections/jobs-preview/jobs-preview';
import { LandingMembership } from './sections/membership/membership';
import { LandingEventsPreview } from './sections/events-preview/events-preview';
import { LandingNewsPreview } from './sections/news-preview/news-preview';
import { LandingEcPreview } from './sections/ec-preview/ec-preview';
import { LandingGalleryPreview } from './sections/gallery-preview/gallery-preview';
import { LandingRecentMembersPreview } from './sections/recent-members-preview/recent-members-preview';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    LandingBanner,
    LandingPurpose,
    LandingJobsPreview,
    LandingMembership,
    LandingEventsPreview,
    LandingNewsPreview,
    LandingGalleryPreview,
    LandingEcPreview,
    LandingRecentMembersPreview
  ],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
  encapsulation: ViewEncapsulation.None
})
export class Landing implements OnInit {
  private notify = inject(NotificationService);

  ngOnInit() {
    // Welcome toast removed from public landing as per request
  }
}

