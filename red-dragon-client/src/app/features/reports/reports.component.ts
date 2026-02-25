import { Component, OnInit } from '@angular/core';
import { MilitaryService } from '../../core/services/military.service';
import { BattleReport } from '../../core/models/kingdom.model';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  reports: BattleReport[] = [];
  loading = true;

  constructor(private militaryService: MilitaryService) {}

  ngOnInit(): void {
    this.militaryService.getBattleReports().subscribe({
      next: (r) => { this.reports = r; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}
