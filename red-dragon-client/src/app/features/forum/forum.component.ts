import { Component, OnInit } from '@angular/core';
import { ForumService } from '../../core/services/forum.service';
import { ForumPost } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-forum',
  templateUrl: './forum.component.html',
  styleUrls: ['./forum.component.scss']
})
export class ForumComponent implements OnInit {
  activeTab: 'general' | 'coalition' = 'general';
  posts: ForumPost[] = [];
  loading = true;
  message = '';

  newSubject = '';
  newBody = '';
  replyingTo: ForumPost | null = null;
  replyBody = '';

  constructor(private forumService: ForumService) {}

  ngOnInit(): void {
    this.loadPosts();
  }

  switchTab(tab: 'general' | 'coalition'): void {
    this.activeTab = tab;
    this.replyingTo = null;
    this.loadPosts();
  }

  loadPosts(): void {
    this.loading = true;
    const obs = this.activeTab === 'general'
      ? this.forumService.getGeneralPosts()
      : this.forumService.getCoalitionPosts();

    obs.subscribe({
      next: (posts) => { this.posts = posts; this.loading = false; },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Nie udało się załadować forum.';
        this.loading = false;
      }
    });
  }

  submitPost(): void {
    if (!this.newSubject.trim() || !this.newBody.trim()) {
      this.message = 'Wypełnij temat i treść.';
      return;
    }
    const dto = { subject: this.newSubject, body: this.newBody, parentPostId: null };
    const obs = this.activeTab === 'general'
      ? this.forumService.createGeneralPost(dto)
      : this.forumService.createCoalitionPost(dto);

    obs.subscribe({
      next: () => {
        this.newSubject = '';
        this.newBody = '';
        this.message = 'Post dodany.';
        this.loadPosts();
        this.clearMsg();
      },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Błąd.';
        this.clearMsg();
      }
    });
  }

  startReply(post: ForumPost): void {
    this.replyingTo = post;
    this.replyBody = '';
  }

  cancelReply(): void {
    this.replyingTo = null;
    this.replyBody = '';
  }

  submitReply(): void {
    if (!this.replyingTo || !this.replyBody.trim()) return;
    const dto = { subject: 'Re: ' + this.replyingTo.subject, body: this.replyBody, parentPostId: this.replyingTo.id };
    const obs = this.activeTab === 'general'
      ? this.forumService.createGeneralPost(dto)
      : this.forumService.createCoalitionPost(dto);

    obs.subscribe({
      next: () => {
        this.replyingTo = null;
        this.replyBody = '';
        this.message = 'Odpowiedź dodana.';
        this.loadPosts();
        this.clearMsg();
      },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Błąd.';
        this.clearMsg();
      }
    });
  }

  private clearMsg(): void {
    setTimeout(() => this.message = '', 4000);
  }
}
