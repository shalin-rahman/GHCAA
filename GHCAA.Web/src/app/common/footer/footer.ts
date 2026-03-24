import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UpperCasePipe } from '@angular/common';
import { APP_CONFIG } from '../../core/constants/app.constants';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink, UpperCasePipe],
  templateUrl: './footer.html',
  styleUrl: './footer.scss'
})
export class AppFooter {
  currentYear = new Date().getFullYear();
  appConfig = APP_CONFIG;
}
