import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TurnService {
  // BehaviorSubject — przechowuje ostatnie delty, dzięki czemu po powrocie na Stolicę
  // (ponowna subskrypcja) zmiany z ostatniej tury są nadal widoczne.
  private turnProcessed = new BehaviorSubject<{ [key: string]: number }>({});
  turnProcessed$ = this.turnProcessed.asObservable();

  /** Ostatnio wyliczone delty (utrwalone między przejściami między zakładkami). */
  get lastDeltas(): { [key: string]: number } {
    return this.turnProcessed.value;
  }

  emitDeltas(deltas: { [key: string]: number }): void {
    this.turnProcessed.next(deltas);
  }
}
