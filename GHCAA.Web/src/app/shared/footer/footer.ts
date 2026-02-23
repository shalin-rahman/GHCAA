import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  template: `
    <footer class="footer">
      <div class="container footer-grid">
        <div class="footer-brand">
            <div class="brand-icon footer-brand-icon">GHC</div>
            <p class="motto">"Sharing Heritage, Aligning Lives, Integrating Networks"</p>
            <p class="bengali-motto hide-mobile">ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন</p>
        </div>

        <div class="footer-links">
            <h4>Quick Links</h4>
            <a routerLink="/">Home Page</a>
            <a routerLink="/about">About GHCAA</a>
            <a routerLink="/contact">Contact Us</a>
            <a routerLink="/register">Member Registry</a>
        </div>

        <div class="footer-links">
            <h4>Institution</h4>
            <a href="https://www.haragangacollege.edu.bd/en" target="_blank">College Website</a>
            <a href="/#vision">Purpose & Objectives</a>
            <a href="#">Code of Ethics</a>
            <a href="#">Privacy Policy</a>
        </div>

        <div class="footer-contact">
            <h4>Secretariat</h4>
            <p><strong>Registered Office:</strong> Govt. Haraganga College Campus, Munshiganj, Bangladesh.</p>
            <p><strong>Enquiries:</strong> haragangian&#64;gmail.com</p>
            <div class="social-icons">
                <a href="#" class="social-icon">FB</a>
                <a href="#" class="social-icon">WA</a>
                <a href="#" class="social-icon">YT</a>
            </div>
        </div>
      </div>
      <div class="footer-bottom">
        <div class="container">
            <p>&copy; 2025 Govt. Haraganga College Alumni Association (HARAGANGIAN). <br class="show-mobile"> Registered under the Registrar of Societies.</p>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer { 
      background: #111111; 
      color: white; 
      padding-top: 5rem; 
      margin-top: 0;
      border-top: 1px solid var(--glass-border);
    }
    .footer-grid { display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 4rem; padding-bottom: 4rem; }
    
    .footer-brand .footer-brand-icon { 
        margin-bottom: 1.5rem; 
        font-size: 1.5rem;
        width: 60px;
        height: 60px;
    }
    .motto { font-style: italic; color: var(--primary-color); font-weight: 600; margin-bottom: 0.5rem; font-size: 1rem; }
    .bengali-motto { font-size: 0.85rem; opacity: 0.5; color: #fff; }

    .footer-links h4, .footer-contact h4 { color: #fff; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 1.5px; margin-bottom: 1.5rem; border-bottom: 2px solid var(--accent-color); display: inline-block; padding-bottom: 0.25rem; }
    .footer-links { display: flex; flex-direction: column; gap: 0.75rem; }
    .footer-links a { color: rgba(255,255,255,0.6); text-decoration: none; font-size: 0.9rem; transition: 0.3s; }
    .footer-links a:hover { color: var(--accent-color); transform: translateX(5px); }

    .footer-contact p { font-size: 0.9rem; color: rgba(255,255,255,0.6); margin-bottom: 1rem; line-height: 1.6; }
    .social-icons { display: flex; gap: 1rem; margin-top: 1.5rem; }
    .social-icon { width: 32px; height: 32px; background: rgba(255,255,255,0.1); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 0.7rem; font-weight: 800; color: white; text-decoration: none; transition: 0.3s; }
    .social-icon:hover { background: var(--accent-color); color: #111; }

    .footer-bottom { background: rgba(0,0,0,0.3); padding: 1.5rem 0; text-align: center; font-size: 0.75rem; color: rgba(255,255,255,0.4); border-top: 1px solid rgba(255,255,255,0.05); }

    @media (max-width: 992px) {
        .footer-grid { grid-template-columns: 1fr 1fr; gap: 3rem; }
    }
    @media (max-width: 600px) {
        .footer-grid { grid-template-columns: 1fr; text-align: center; }
        .footer-links h4, .footer-contact h4 { display: block; }
        .footer-links a:hover { transform: none; }
        .social-icons { justify-content: center; }
    }
  `]
})
export class AppFooter { }
