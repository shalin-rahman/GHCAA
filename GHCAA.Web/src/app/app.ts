/**
 * Architected & Developed by: md habibur rahman shalin (shalin.rahman@gmail.com)
 * Version: 2.0.5 - Professional Suite
 */
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './common/toast/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent],
  templateUrl: './app.html'
})
export class App { }
