import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { Header } from './layout/header/header';
import { Footer } from './layout/footer/footer';

const STANDALONE_LAYOUT_PREFIXES = ['/admin', '/mon-compte'];

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly router = inject(Router);

  protected readonly showPublicChrome = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map((event) => !STANDALONE_LAYOUT_PREFIXES.some((prefix) => event.urlAfterRedirects.startsWith(prefix))),
      startWith(!STANDALONE_LAYOUT_PREFIXES.some((prefix) => this.router.url.startsWith(prefix))),
    ),
    { initialValue: true },
  );
}
