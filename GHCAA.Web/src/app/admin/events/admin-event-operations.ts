import { Component, inject, signal, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';
import { toWireDate, toDisplayDate } from '../../core/utils/date.util';
import { Icon } from '../../common/icon/icon';

@Component({
  selector: 'app-admin-event-operations',
  standalone: true,
  imports: [CommonModule, FormsModule, Icon],
  templateUrl: './admin-event-operations.html',
  styleUrl: './admin-event-operations.scss'
})
export class AdminEventOperations implements OnInit {
  private eventsService = inject(EventsService);
  private notify = inject(NotificationService);

  @Input() event!: AlumniEvent;

  tasks = signal<any[]>([]);
  budget = signal<any>(null);
  
  // Tab handling
  activeOpTab = signal<'tasks' | 'budget'>('tasks');

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
    if (!this.newTask.title) return;
    this.eventsService.createTask({ ...this.newTask, dueDate: toWireDate(this.newTask.dueDate), eventId: this.event.id }).subscribe({
      next: () => {
        this.notify.success('Task assigned!');
        this.newTask = { title: '', description: '', assignedMemberId: null, dueDate: '' };
        this.loadTasks();
      },
      error: () => this.notify.error('Failed to assign task.')
    });
  }

  toggleTask(taskId: number) {
    this.eventsService.toggleTask(taskId).subscribe({
      next: () => this.loadTasks(),
      error: () => this.notify.error('Failed to update task.')
    });
  }

  deleteTask(taskId: number) {
    if (confirm('Delete this task?')) {
      this.eventsService.deleteTask(taskId).subscribe({
        next: () => {
          this.notify.success('Task removed');
          this.loadTasks();
        },
        error: () => this.notify.error('Failed to remove task.')
      });
    }
  }

  updateBudget() {
    this.eventsService.updateBudget({ eventId: this.event.id, estimatedTotal: this.newBudget.estimatedTotal }).subscribe({
      next: () => {
        this.notify.success('Budget updated!');
        this.loadBudget();
      },
      error: () => this.notify.error('Failed to update budget.')
    });
  }

  addExpense() {
    if (!this.newExpense.category || this.newExpense.amount <= 0) return;
    this.eventsService.addExpense({ ...this.newExpense, spentAt: toWireDate(this.newExpense.spentAt), eventId: this.event.id }).subscribe({
      next: () => {
        this.notify.success('Expense recorded!');
        this.newExpense = { category: '', amount: 0, note: '', spentAt: this.formatDateToDMY(new Date()) };
        this.loadBudget();
      },
      error: () => this.notify.error('Failed to record expense.')
    });
  }

  deleteExpense(expenseId: number) {
    if (confirm('Delete this expense?')) {
      this.eventsService.deleteExpense(expenseId).subscribe({
        next: () => {
          this.notify.success('Expense removed');
          this.loadBudget();
        },
        error: () => this.notify.error('Failed to remove expense.')
      });
    }
  }
}
