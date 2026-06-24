import { Component, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

interface Trip {
  from: string;
  to: string;
  duration: string;
  seats: number;
  price: number;
  company: string;
  imageClass: string;
}

@Component({
  selector: 'app-trajets',
  imports: [DecimalPipe],
  templateUrl: './trajets.html',
  styleUrl: './trajets.scss',
})
export class Trajets {
  private readonly allTrips: Trip[] = [
    { from: 'Nouakchott', to: 'Nouadhibou', duration: '16h 30m', seats: 45, price: 3500, company: 'SNT', imageClass: 'from-sky-500 to-blue-700' },
    { from: 'Nouakchott', to: 'Rosso', duration: '5h 30m', seats: 23, price: 1500, company: 'STP', imageClass: 'from-amber-500 to-orange-700' },
    { from: 'Nouakchott', to: 'Kiffa', duration: '8h 15m', seats: 34, price: 2000, company: 'SONEF', imageClass: 'from-stone-500 to-stone-700' },
    { from: 'Nouakchott', to: 'Ayoun', duration: '10h 45m', seats: 28, price: 2500, company: 'TVC', imageClass: 'from-emerald-500 to-teal-700' },
    { from: 'Nouakchott', to: 'Atar', duration: '7h 00m', seats: 30, price: 2200, company: 'Mauritanie Voyages', imageClass: 'from-rose-500 to-pink-700' },
    { from: 'Nouakchott', to: 'Néma', duration: '14h 00m', seats: 20, price: 3200, company: 'SNT', imageClass: 'from-indigo-500 to-violet-700' },
  ];

  protected readonly departureFilter = signal('');
  protected readonly arrivalFilter = signal('');

  protected readonly filteredTrips = computed(() => {
    const from = this.departureFilter().toLowerCase();
    const to = this.arrivalFilter().toLowerCase();
    return this.allTrips.filter(
      (trip) =>
        (!from || trip.from.toLowerCase().includes(from)) &&
        (!to || trip.to.toLowerCase().includes(to))
    );
  });

  constructor(route: ActivatedRoute) {
    const params = route.snapshot.queryParamMap;
    this.departureFilter.set(params.get('depart') ?? '');
    this.arrivalFilter.set(params.get('arrivee') ?? '');
  }

  protected onDepartureInput(event: Event): void {
    this.departureFilter.set((event.target as HTMLInputElement).value);
  }

  protected onArrivalInput(event: Event): void {
    this.arrivalFilter.set((event.target as HTMLInputElement).value);
  }
}
