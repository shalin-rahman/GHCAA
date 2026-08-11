import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ForumService } from '../../core/services/forum.service';
import { ForumCategory, ForumTopic, CreateForumTopicDto } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';

@Component({
    selector: 'app-forum',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, LogoSpinnerComponent],
    templateUrl: './forum.html',
    styleUrls: ['./forum.scss']
})
export class Forum implements OnInit {
    private forumService = inject(ForumService);
    private router = inject(Router);

    categories = signal<ForumCategory[]>([]);
    topics = signal<ForumTopic[]>([]);
    selectedCategory = signal<ForumCategory | null>(null);
    loading = signal(false);
    topicsLoading = signal(false);
    showCreateForm = signal(false);
    errorMsg = signal('');

    // Create topic form
    newTopic: CreateForumTopicDto = { categoryId: 0, title: '', content: '' };
    submitting = signal(false);

    totalTopics = computed(() => this.topics().length);

    ngOnInit() {
        this.loadCategories();
    }

    loadCategories() {
        this.loading.set(true);
        this.forumService.getCategories().subscribe({
            next: cats => { this.categories.set(cats); this.loading.set(false); },
            error: () => { this.errorMsg.set('Failed to load forum categories.'); this.loading.set(false); }
        });
    }

    selectCategory(cat: ForumCategory) {
        this.selectedCategory.set(cat);
        this.showCreateForm.set(false);
        this.topics.set([]);
        this.topicsLoading.set(true);
        this.forumService.getTopics(cat.id).subscribe({
            next: t => { this.topics.set(t); this.topicsLoading.set(false); },
            error: () => { this.errorMsg.set('Failed to load topics.'); this.topicsLoading.set(false); }
        });
    }

    openTopic(topic: ForumTopic) {
        this.router.navigate(['/portal/forum', topic.id]);
    }

    openCreateForm() {
        const cat = this.selectedCategory();
        if (!cat) return;
        this.newTopic = { categoryId: cat.id, title: '', content: '' };
        this.showCreateForm.set(true);
    }

    cancelCreate() {
        this.showCreateForm.set(false);
    }

    submitTopic() {
        if (!this.newTopic.title.trim() || !this.newTopic.content.trim()) return;
        this.submitting.set(true);
        this.forumService.createTopic(this.newTopic).subscribe({
            next: topic => {
                this.submitting.set(false);
                this.showCreateForm.set(false);
                this.router.navigate(['/portal/forum', topic.id]);
            },
            error: () => {
                this.errorMsg.set('Failed to create topic. Please try again.');
                this.submitting.set(false);
            }
        });
    }

    backToCategories() {
        this.selectedCategory.set(null);
        this.topics.set([]);
    }

    formatDate(d: string | Date): string {
        if (!d) return '';
        const date = new Date(d);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}-${month}-${year}`;
    }
}
