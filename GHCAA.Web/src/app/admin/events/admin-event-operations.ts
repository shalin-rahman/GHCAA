import { Component, inject, signal, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';
import { Icon } from '../../common/icon/icon';
import { PageHeaderComponent } from '../../common/page-header/page-header.component';
import { OrgConfigService } from '../../core/services/org-config.service';
import { AppCurrencyPipe } from '../../core/pipes/app-currency.pipe';

@Component({
  selector: 'app-admin-event-operations',
  standalone: true,
  imports: [CommonModule, FormsModule, Icon, PageHeaderComponent, AppCurrencyPipe],
  templateUrl: './admin-event-operations.html',
  styleUrl: './admin-event-operations.scss'
})
export class AdminEventOperations implements OnInit {
  private eventsService = inject(EventsService);
  private notify = inject(NotificationService);
  private confirmDialog = inject(ConfirmDialogService);
  orgConfig = inject(OrgConfigService);

  @Input() event!: AlumniEvent;

  tasks = signal<any[]>([]);
  budget = signal<any>(null);
  
  // Tab handling
  activeOpTab = signal<'tasks' | 'budget'>('tasks');

  addingTask = signal(false);
  togglingTaskId = signal<number | null>(null);
  deletingTaskId = signal<number | null>(null);
  savingBudget = signal(false);
  addingExpense = signal(false);
  deletingExpenseId = signal<number | null>(null);

  formatDateToDMY(d: any) {
    return toDisplayDate(d);
  }

  // Task Form
  newTask = { title: '', description: '', assignedMemberId: null as number | null, dueDate: '' };
  
  // Budget Form
  newBudget = { estimatedTotal: 0 };
  newExpense = { category: '', amount: 0, note: '', spentAt: this.formatDateToDMY(new Date()) };

  ngOnInit() {
    this.loadTasks();
    this.loadBudget();
  }

  loadTasks() {
    // 29F.2: surface HTTP failures instead of failing silently
    this.eventsService.getEventTasks(this.event.id).subscribe({
      next: (data: any[]) => this.tasks.set(data),
      error: () => this.notify.error('Failed to load tasks.')
    });
  }

  loadBudget() {
    this.eventsService.getEventBudget(this.event.id).subscribe({
      next: (data: any) => {
        this.budget.set(data);
        if (data) {
          this.newBudget.estimatedTotal = data.estimatedTotal;
        }
      },
      error: () => this.budget.set(null)
    });
  }

  addTask() {
    if (this.addingTask() || !this.newTask.title) return;
    this.addingTask.set(true);
    this.eventsService.createTask({ ...this.newTask, dueDate: toWireDate(this.newTask.dueDate), eventId: this.event.id }).subscribe({
      next: () => {
        this.addingTask.set(false);
        this.notify.success('Task assigned!');
        this.newTask = { title: '', description: '', assignedMemberId: null, dueDate: '' };
        this.loadTasks();
      },
      error: () => {
        this.addingTask.set(false);
        this.notify.error('Failed to assign task.');
      }
    });
  }

  toggleTask(taskId: number) {
    if (this.togglingTaskId() !== null) return;
    this.togglingTaskId.set(taskId);
    this.eventsService.toggleTask(taskId).subscribe({
      next: () => { this.togglingTaskId.set(null); this.loadTasks(); },
      error: () => {
        this.togglingTaskId.set(null);
        this.notify.error('Failed to update task.');
      }
    });
  }

  async deleteTask(taskId: number) {
    if (this.deletingTaskId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Delete task',
      message: 'Delete this task?',
      confirmLabel: 'Delete',
      danger: true
    }));
    if (!ok) return;

    this.deletingTaskId.set(taskId);
    this.eventsService.deleteTask(taskId).subscribe({
      next: () => {
        this.deletingTaskId.set(null);
        this.notify.success('Task removed');
        this.loadTasks();
      },
      error: () => {
        this.deletingTaskId.set(null);
        this.notify.error('Failed to remove task.');
      }
    });
  }

  updateBudget() {
    if (this.savingBudget()) return;
    this.savingBudget.set(true);
    this.eventsService.updateBudget({ eventId: this.event.id, estimatedTotal: this.newBudget.estimatedTotal }).subscribe({
      next: () => {
        this.savingBudget.set(false);
        this.notify.success('Budget updated!');
        this.loadBudget();
      },
      error: () => {
        this.savingBudget.set(false);
        this.notify.error('Failed to update budget.');
      }
    });
  }

  addExpense() {
    if (this.addingExpense() || !this.newExpense.category || this.newExpense.amount <= 0) return;
    this.addingExpense.set(true);
    this.eventsService.addExpense({ ...this.newExpense, spentAt: toWireDate(this.newExpense.spentAt), eventId: this.event.id }).subscribe({
      next: () => {
        this.addingExpense.set(false);
        this.notify.success('Expense recorded!');
        this.newExpense = { category: '', amount: 0, note: '', spentAt: this.formatDateToDMY(new Date()) };
        this.loadBudget();
      },
      error: () => {
        this.addingExpense.set(false);
        this.notify.error('Failed to record expense.');
      }
    });
  }

  async deleteExpense(expenseId: number) {
    if (this.deletingExpenseId() !== null) return;
    const ok = await firstValueFrom(this.confirmDialog.confirm({
      title: 'Delete expense',
      message: 'Delete this expense?',
      confirmLabel: 'Delete',
      danger: true
    }));
    if (!ok) return;

    this.deletingExpenseId.set(expenseId);
    this.eventsService.deleteExpense(expenseId).subscribe({
      next: () => {
        this.deletingExpenseId.set(null);
        this.notify.success('Expense removed');
        this.loadBudget();
      },
      error: () => {
        this.deletingExpenseId.set(null);
        this.notify.error('Failed to remove expense.');
      }
    });
  }
}
