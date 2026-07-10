import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ForumService } from '../../core/services/forum.service';
import { CoalitionService } from '../../core/services/coalition.service';
import { KingdomService } from '../../core/services/kingdom.service';
import { ForumPost, Kingdom, KingdomSummary } from '../../core/models/kingdom.model';

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

  // Lista księstw koalicji (pasek u góry forum)
  kingdom: Kingdom | null = null;
  coalitionMembers: KingdomSummary[] = [];

  constructor(private forumService: ForumService, private translate: TranslateService,
              private coalitionService: CoalitionService, private kingdomService: KingdomService) {}

  ngOnInit(): void {
    this.loadPosts();
    this.loadCoalitionMembers();
  }

  /** Członkowie mojej koalicji — wyświetlani u góry forum. */
  private loadCoalitionMembers(): void {
    this.kingdomService.getMyKingdom().subscribe({
      next: k => {
        this.kingdom = k;
        if (!k.coalitionId) return;
        this.coalitionService.getCoalitions().subscribe({
          next: cs => {
            const mine = cs.find(c => c.id === k.coalitionId);
            this.coalitionMembers = mine ? mine.members : [];
          },
          error: () => this.coalitionMembers = []
        });
      },
      error: () => {}
    });
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
