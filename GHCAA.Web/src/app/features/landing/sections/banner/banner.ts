import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'landing-banner',
    standalone: true,
    imports: [RouterLink],
    templateUrl: './banner.html',
    styleUrl: './banner.scss',
})
export class LandingBanner { }
