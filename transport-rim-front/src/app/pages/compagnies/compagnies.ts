import { Component } from '@angular/core';

interface Company {
  name: string;
  logo: string;
  fleet: number;
  destinations: number;
}

@Component({
  selector: 'app-compagnies',
  imports: [],
  templateUrl: './compagnies.html',
  styleUrl: './compagnies.scss',
})
export class Compagnies {
  protected readonly companies: Company[] = [
    { name: 'SONEF Transport', logo: '/images/logos/sonef.png', fleet: 24, destinations: 12 },
    { name: 'STP', logo: '/images/logos/stp.png', fleet: 18, destinations: 10 },
    { name: 'STM', logo: '/images/logos/stm.png', fleet: 15, destinations: 8 },
    { name: 'TVC', logo: '/images/logos/tvc.png', fleet: 20, destinations: 11 },
    { name: 'SNT', logo: '/images/logos/snt.png', fleet: 30, destinations: 15 },
  ];
}
