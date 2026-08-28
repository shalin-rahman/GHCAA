/**
 * Architected & Developed by: md habibur rahman shalin (shalin.rahman@gmail.com)
 * Version: 2.0.5 - Professional Suite
 */
import { Component, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { ToastComponent } from './common/toast/toast';
import { StepUpDialog } from './common/step-up-dialog/step-up-dialog';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, StepUpDialog],
  templateUrl: './app.html'
})
export class App {
  private router = inject(Router);

  constructor() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      window.scrollTo({ top: 0, behavior: 'instant' });
    });
  }
}
