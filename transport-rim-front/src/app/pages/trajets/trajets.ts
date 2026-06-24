import { Component, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

interface Trip {
  from: string;
  to: string;
  wilaya: string;
  duration: string;
  seats: number;
  price: number;
  company: string;
  image?: string;
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
    { from: 'Nouakchott', to: 'Nouadhibou', wilaya: 'Dakhlet Nouadhibou', duration: '16h 30m', seats: 45, price: 700, company: 'SNT', image: '/images/trips/nouadhibou.jpg', imageClass: 'from-sky-500 to-blue-700' },
    { from: 'Nouakchott', to: 'Rosso', wilaya: 'Trarza', duration: '5h 30m', seats: 23, price: 500, company: 'STM', image: '/images/trips/rosso.jpg', imageClass: 'from-amber-500 to-orange-700' },
    { from: 'Nouakchott', to: 'Kiffa', wilaya: 'Assaba', duration: '8h 15m', seats: 34, price: 1000, company: 'SONEF', image: '/images/trips/kiffa.jpg', imageClass: 'from-stone-500 to-stone-700' },
    { from: 'Nouakchott', to: 'Ayoun', wilaya: 'Hodh El Gharbi', duration: '10h 45m', seats: 28, price: 1200, company: 'TVC', image: '/images/trips/ayoun.jpg', imageClass: 'from-emerald-500 to-teal-700' },
    { from: 'Nouakchott', to: 'Atar', wilaya: 'Adrar', duration: '7h 00m', seats: 30, price: 700, company: 'Mauritanie Voyages', image: '/images/trips/atar.jpg', imageClass: 'from-rose-500 to-pink-700' },
    { from: 'Nouakchott', to: 'Néma', wilaya: 'Hodh Ech Chargui', duration: '14h 00m', seats: 20, price: 1300, company: 'SNT', image: '/images/trips/nema.jpg', imageClass: 'from-indigo-500 to-violet-700' },
    { from: 'Nouakchott', to: 'Kaédi', wilaya: 'Gorgol', duration: '6h 00m', seats: 32, price: 600, company: 'STM', image: '/images/trips/kaedi.jpg', imageClass: 'from-cyan-500 to-sky-700' },
    { from: 'Nouakchott', to: 'Aleg', wilaya: 'Brakna', duration: '3h 00m', seats: 36, price: 300, company: 'STP', image: '/images/trips/aleg.jpg', imageClass: 'from-lime-500 to-green-700' },
    { from: 'Nouakchott', to: 'Tidjikja', wilaya: 'Tagant', duration: '9h 00m', seats: 26, price: 900, company: 'SONEF', image: '/images/trips/tidjikja.jpg', imageClass: 'from-amber-600 to-stone-700' },
    { from: 'Nouakchott', to: 'Sélibaby', wilaya: 'Guidimakha', duration: '9h 30m', seats: 24, price: 800, company: 'TVC', image: '/images/trips/selibaby.jpg', imageClass: 'from-fuchsia-500 to-purple-700' },
    { from: 'Nouakchott', to: 'Zouérat', wilaya: 'Tiris Zemmour', duration: '12h 00m', seats: 22, price: 9000, company: 'Mauritanie Voyages', image: '/images/trips/zouerat.jpg', imageClass: 'from-slate-500 to-zinc-700' },
    { from: 'Nouakchott', to: 'Akjoujt', wilaya: 'Inchiri', duration: '3h 30m', seats: 34, price: 600, company: 'SNT', image: '/images/trips/akjoujt.jpg', imageClass: 'from-orange-400 to-amber-700' },
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
