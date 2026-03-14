import { Component, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LandingBanner } from './sections/banner/banner';
import { LandingPurpose } from './sections/purpose/purpose';
import { LandingJobsPreview } from './sections/jobs-preview/jobs-preview';
import { LandingMembership } from './sections/membership/membership';
import { LandingEventsPreview } from './sections/events-preview/events-preview';
import { LandingNewsPreview } from './sections/news-preview/news-preview';
import { LandingEcPreview } from './sections/ec-preview/ec-preview';
import { LandingGalleryPreview } from './sections/gallery-preview/gallery-preview';
import { LandingCtaBanner } from './sections/cta-banner/cta-banner';

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
    LandingCtaBanner
  ],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
  encapsulation: ViewEncapsulation.None
})
export class Landing { }

