import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
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

  constructor(private forumService: ForumService, private translate: TranslateService) {}

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
        this.message = err.error?.message || err.error || this.translate.instant('forum.errLoad');
        this.loading = false;
      }
    });
  }

  submitPost(): void {
    if (!this.newBody.trim()) {
      this.message = this.translate.instant('forum.enterBody');
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
        this.message = this.translate.instant('forum.postAdded');
        this.loadPosts();
        this.clearMsg();
      },
      error: (err) => {
        this.message = err.error?.message || err.error || this.translate.instant('common.error');
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
        this.message = this.translate.instant('forum.replyAdded');
        this.loadPosts();
        this.clearMsg();
      },
      error: (err) => {
        this.message = err.error?.message || err.error || this.translate.instant('common.error');
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
      case 'Imperator': return `[${this.translate.instant('pol.role.imperator')}]`;
      case 'MainCommander': return `[${this.translate.instant('pol.role.commander')}]`;
      default: return '';
    }
  }

  private clearMsg(): void {
    setTimeout(() => this.message = '', 4000);
  }
}
