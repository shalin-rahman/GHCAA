import { Component, signal, computed, inject, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ForumService } from '../../core/services/forum.service';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ForumTopic, ForumPost, CreateForumPostDto } from '../../core/models/business.models';
import { LogoSpinnerComponent } from '../../common/logo-spinner/logo-spinner';
import { ImgFallbackDirective } from '../../common/directives/img-fallback.directive';

@Component({
    selector: 'app-topic-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, LogoSpinnerComponent, ImgFallbackDirective],
    templateUrl: './topic-detail.html',
    styleUrls: ['./topic-detail.scss']
})
export class TopicDetail implements OnInit {
    private forumService = inject(ForumService);
    private authService = inject(AuthService);
    private confirmDialog = inject(ConfirmDialogService);
    private route = inject(ActivatedRoute);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);

    topic = signal<ForumTopic | null>(null);
    posts = signal<ForumPost[]>([]);
    
    loadingTopic = signal(false);
    loadingPosts = signal(false);
    errorMsg = signal('');
    
    // Reply form
    newReplyContent = signal('');
    replyingToPost = signal<ForumPost | null>(null);
    submittingReply = signal(false);

    // Pagination
    page = signal(1);
    pageSize = 20;
    hasMorePosts = signal(true);

    currentUser = computed(() => this.authService.currentUser());
    isAdmin = computed(() => {
        const user = this.currentUser();
        return user?.role === 'Admin' || user?.role === 'SuperAdmin';
    });

    ngOnInit() {
        this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
            const topicId = Number(params.get('id'));
            if (topicId) {
                this.loadTopic(topicId);
                this.loadPosts(topicId);
            }
        });
    }

    loadTopic(topicId: number) {
        this.loadingTopic.set(true);
        this.forumService.getTopic(topicId).subscribe({
            next: t => {
                this.topic.set(t);
                this.loadingTopic.set(false);
            },
            error: () => {
                this.errorMsg.set('Failed to load topic.');
                this.loadingTopic.set(false);
            }
        });
    }

    loadPosts(topicId: number, append = false) {
        this.loadingPosts.set(true);
        this.forumService.getPosts(topicId, this.page(), this.pageSize).subscribe({
            next: newPosts => {
                if (append) {
                    this.posts.update(existing => [...existing, ...newPosts]);
                } else {
                    this.posts.set(newPosts);
                }
                this.hasMorePosts.set(newPosts.length === this.pageSize);
                this.loadingPosts.set(false);
            },
            error: () => {
                this.errorMsg.set('Failed to load replies.');
                this.loadingPosts.set(false);
            }
        });
    }

    loadMore() {
        const t = this.topic();
        if (!t || this.loadingPosts() || !this.hasMorePosts()) return;
        this.page.update(p => p + 1);
        this.loadPosts(t.id, true);
    }

    setReplyTo(post: ForumPost | null) {
        this.replyingToPost.set(post);
        // Scroll to reply form
        const replyForm = document.getElementById('reply-form');
        if (replyForm) {
            replyForm.scrollIntoView({ behavior: 'smooth' });
        }
    }

    submitReply() {
        const t = this.topic();
        if (!t || !this.newReplyContent().trim() || this.submittingReply()) return;

        this.submittingReply.set(true);
        const dto: CreateForumPostDto = {
            topicId: t.id,
            content: this.newReplyContent().trim(),
            parentPostId: this.replyingToPost()?.id || undefined
        };

        this.forumService.createPost(dto).subscribe({
            next: post => {
                this.newReplyContent.set('');
                this.replyingToPost.set(null);
                this.submittingReply.set(false);
                
                this.page.set(1);
                this.loadPosts(t.id);
                this.loadTopic(t.id);
            },
            error: () => {
                this.errorMsg.set('Failed to post reply.');
                this.submittingReply.set(false);
            }
        });
    }

    async deleteTopic() {
        const t = this.topic();
        if (!t) return;
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete topic',
            message: 'Are you sure you want to delete this topic and all its replies?',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.forumService.deleteTopic(t.id).subscribe({
            next: () => {
                this.router.navigate(['/portal/forum']);
            },
            error: () => {
                this.errorMsg.set('Failed to delete topic.');
            }
        });
    }

    async deletePost(postId: number) {
        const ok = await firstValueFrom(this.confirmDialog.confirm({
            title: 'Delete reply',
            message: 'Are you sure you want to delete this reply?',
            confirmLabel: 'Delete',
            danger: true
        }));
        if (!ok) return;

        this.forumService.deletePost(postId).subscribe({
            next: () => {
                this.posts.update(existing => existing.filter(p => p.id !== postId));
                const t = this.topic();
                if (t) {
                    this.loadTopic(t.id);
                }
            },
            error: () => {
                this.errorMsg.set('Failed to delete reply.');
            }
        });
    }

    getParentAuthorName(parentPostId: number): string {
        const parent = this.posts().find(p => p.id === parentPostId);
        return parent ? parent.authorName : 'Author';
    }

    formatDate(d: string | Date): string {
        if (!d) return '';
        const date = new Date(d);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}-${month}-${year}`;
    }

    canDeleteTopic(t: ForumTopic): boolean {
        const user = this.currentUser();
        if (!user) return false;
        return t.authorId === user.memberId || this.isAdmin();
    }

    canDeletePost(p: ForumPost): boolean {
        const user = this.currentUser();
        if (!user) return false;
        return p.authorId === user.memberId || this.isAdmin();
    }
}
