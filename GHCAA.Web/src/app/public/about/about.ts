import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { APP_CONFIG } from '../../core/constants/app.constants';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about.html',
  styleUrl: './about.scss'
})
export class About { 
  appConfig = APP_CONFIG;
}

