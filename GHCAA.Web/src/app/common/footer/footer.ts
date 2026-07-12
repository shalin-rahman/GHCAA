import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, UpperCasePipe } from '@angular/common';
import { OrgConfigService } from '../../core/services/org-config.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink, UpperCasePipe, CommonModule],
  templateUrl: './footer.html',
  styleUrl: './footer.scss'
})
export class AppFooter {
  currentYear = new Date().getFullYear();
  orgConfigService = inject(OrgConfigService);
}
