import { Pipe, PipeTransform, inject } from '@angular/core';
import { OrgConfigService } from '../services/org-config.service';
import { toDisplayDate } from '../utils/date.util';

@Pipe({
  name: 'appDate',
  standalone: true,
  pure: false
})
export class AppDatePipe implements PipeTransform {
  private readonly orgConfig = inject(OrgConfigService);

  transform(value: unknown): string {
    return toDisplayDate(value, this.orgConfig.dateFormat());
  }
}
