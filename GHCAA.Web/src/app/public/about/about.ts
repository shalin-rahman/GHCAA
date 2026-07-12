import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about.html',
  styleUrl: './about.scss'
})
export class About {
  orgConfigService = inject(OrgConfigService);
}

