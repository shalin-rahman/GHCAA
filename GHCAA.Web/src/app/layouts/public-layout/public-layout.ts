import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AppFooter } from '../../shared/footer/footer';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, AppFooter],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss'
})
export class PublicLayout { }
