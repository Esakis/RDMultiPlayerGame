import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TurnService {
  private turnProcessed = new Subject<{ [key: string]: number }>();
  turnProcessed$ = this.turnProcessed.asObservable();

  emitDeltas(deltas: { [key: string]: number }): void {
    this.turnProcessed.next(deltas);
  }
}
