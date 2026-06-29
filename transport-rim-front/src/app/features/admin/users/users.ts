import { Component, inject, signal } from '@angular/core';
import { User } from '../../../core/models/user.model';
import { UserService } from '../../../core/services/user.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-admin-users',
  imports: [],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users {
  private readonly userService = inject(UserService);

  protected readonly users = signal<User[]>([]);
  protected readonly loading = signal(true);
  protected readonly search = signal('');
  protected readonly deletingId = signal<number | null>(null);
  protected readonly error = signal('');

  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.userService.getAll(this.search() || undefined).subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  protected onSearchInput(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
    this.load();
  }

  protected deleteUser(user: User): void {
    if (!confirm(`Supprimer l'utilisateur ${user.name} ?`)) {
      return;
    }

    this.error.set('');
    this.deletingId.set(user.id);
    this.userService.delete(user.id).subscribe({
      next: () => {
        this.users.update((list) => list.filter((u) => u.id !== user.id));
        this.deletingId.set(null);
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(extractApiErrorMessage(err, 'La suppression a échoué.'));
      },
    });
  }
}
