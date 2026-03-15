import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Directory } from '../../common/directory/directory';

@Component({
  selector: 'app-public-directory',
  standalone: true,
  imports: [CommonModule, Directory],
  template: `
    <div class="public-directory-container">
      <div class="header reveal-top">
        <h1>Alumni Connect</h1>
        <p>Browse and search for fellow alumni in our global network</p>
      </div>
      <app-directory [isCompact]="true" [disableProfile]="true"></app-directory>
    </div>
  `,
  styles: [`
    .public-directory-container {
      padding-bottom: 5rem;
    }
    .header {
      text-align: center;
      margin-bottom: 3rem;
      padding-top: 2rem;
      h1 {
        font-size: 3rem;
        margin: 0;
        font-weight: 900;
        color: var(--primary-color);
      }
      p {
        color: var(--text-muted);
        font-size: 1.1rem;
        margin-top: 0.5rem;
      }
    }
  `]
})
export class PublicDirectory {}
