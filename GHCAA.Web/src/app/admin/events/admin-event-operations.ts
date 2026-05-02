import { Component, inject, signal, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventsService } from '../../core/services/events.service';
import { AlumniEvent } from '../../core/models/business.models';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-admin-event-operations',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
    if (!d) return '';
    const date = new Date(d);
    if (isNaN(date.getTime())) return d;
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
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
    this.eventsService.getEventTasks(this.event.id).subscribe((data: any[]) => this.tasks.set(data));
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
    this.eventsService.createTask({ ...this.newTask, eventId: this.event.id }).subscribe(() => {
      this.notify.success('Task assigned!');
      this.newTask = { title: '', description: '', assignedMemberId: null, dueDate: '' };
      this.loadTasks();
    });
  }

  toggleTask(taskId: number) {
    this.eventsService.toggleTask(taskId).subscribe(() => this.loadTasks());
  }

  deleteTask(taskId: number) {
    if (confirm('Delete this task?')) {
      this.eventsService.deleteTask(taskId).subscribe(() => {
        this.notify.success('Task removed');
        this.loadTasks();
      });
    }
  }

  updateBudget() {
    this.eventsService.updateBudget({ eventId: this.event.id, estimatedTotal: this.newBudget.estimatedTotal }).subscribe(() => {
      this.notify.success('Budget updated!');
      this.loadBudget();
    });
  }

  addExpense() {
    if (!this.newExpense.category || this.newExpense.amount <= 0) return;
    this.eventsService.addExpense({ ...this.newExpense, eventId: this.event.id }).subscribe(() => {
      this.notify.success('Expense recorded!');
      this.newExpense = { category: '', amount: 0, note: '', spentAt: this.formatDateToDMY(new Date()) };
      this.loadBudget();
    });
  }

  deleteExpense(expenseId: number) {
    if (confirm('Delete this expense?')) {
      this.eventsService.deleteExpense(expenseId).subscribe(() => {
        this.notify.success('Expense removed');
        this.loadBudget();
      });
    }
  }
}
