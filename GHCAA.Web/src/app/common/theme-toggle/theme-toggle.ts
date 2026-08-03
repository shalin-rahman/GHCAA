import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';
import { Icon } from '../icon/icon';

/**
 * Shared light/dark switch, extracted from <app-user-menu> so the public site can offer
 * it too.
 *
 * The theme choice is persisted globally in localStorage by ThemeService, but the switch
 * used to live only in the portal/admin header — so a visitor who had ever selected light
 * mode saw a white public site with no way to switch back. Placing the same component in
 * the public nav closes that one-way door.
 */
@Component({
    selector: 'app-theme-toggle',
    standalone: true,
    imports: [Icon],
    templateUrl: './theme-toggle.html',
    styleUrl: './theme-toggle.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ThemeToggle {
    theme = inject(ThemeService);

    label = computed(() =>
        this.theme.theme() === 'light' ? 'Switch to dark mode' : 'Switch to light mode'
    );
}
