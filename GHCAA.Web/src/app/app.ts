import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './core/services/auth.service';
import { LoginDto } from './core/models/auth.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  authService = inject(AuthService);

  credentials: LoginDto = {
    username: '',
    password: ''
  };

  login() {
    this.authService.login(this.credentials).subscribe({
      next: (user) => alert(`Logged in as key: ${user.username}`),
      error: (err) => alert('Login failed! Check console.')
    });
  }

  logout() {
    this.authService.logout();
  }
}
