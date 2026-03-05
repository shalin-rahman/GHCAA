import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'landing-cta-banner',
    standalone: true,
    imports: [RouterLink],
    templateUrl: './cta-banner.html',
    styleUrl: './cta-banner.scss',
})
export class LandingCtaBanner { }
