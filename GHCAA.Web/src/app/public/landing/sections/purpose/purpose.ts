import { Component } from '@angular/core';
import { APP_CONFIG } from '../../../../core/constants/app.constants';

@Component({
    selector: 'landing-purpose',
    standalone: true,
    templateUrl: './purpose.html',
    styleUrl: './purpose.scss',
})
export class LandingPurpose {
    appConfig = APP_CONFIG;
}

