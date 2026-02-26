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
  activeSubForum: string | null = 'Ważne'; // For coalition forum
  posts: ForumPost[] = [];
  loading = true;
  message = '';

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
    if (tab === 'coalition' && !this.activeSubForum) {
      this.activeSubForum = 'Ważne';
    }
    this.loadPosts();
  }

  switchSubForum(subForum: string): void {
    this.activeSubForum = subForum;
    this.replyingTo = null;
    this.loadPosts();
  }

  loadPosts(): void {
    this.loading = true;
    const obs = this.activeTab === 'general'
      ? this.forumService.getGeneralPosts()
      : this.forumService.getCoalitionPosts(this.activeSubForum || undefined);

    obs.subscribe({
      next: (posts) => { this.posts = posts; this.loading = false; },
      error: (err) => {
        this.message = err.error?.message || err.error || 'Nie udało się załadować forum.';
        this.loading = false;
      }
    });
  }

  submitPost(): void {
    if (!this.newBody.trim()) {
      this.message = 'Wpisz treść wiadomości.';
      return;
    }
    const dto = { 
      body: this.newBody, 
      subForum: this.activeTab === 'coalition' ? this.activeSubForum : null,
      parentPostId: null 
    };
    const obs = this.activeTab === 'general'
      ? this.forumService.createGeneralPost(dto)
      : this.forumService.createCoalitionPost(dto);

    obs.subscribe({
      next: () => {
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
    const dto = { 
      body: this.replyBody, 
      subForum: this.activeTab === 'coalition' ? this.activeSubForum : null,
      parentPostId: this.replyingTo.id 
    };
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

  getRoleClass(role: string): string {
    switch (role) {
      case 'Imperator': return 'imperator';
      case 'MainCommander': return 'main-commander';
      default: return '';
    }
  }

  getRoleDisplay(role: string): string {
    switch (role) {
      case 'Imperator': return '[Imperator]';
      case 'MainCommander': return '[Głównodowodzący]';
      default: return '';
    }
  }

  private clearMsg(): void {
    setTimeout(() => this.message = '', 4000);
  }
}
