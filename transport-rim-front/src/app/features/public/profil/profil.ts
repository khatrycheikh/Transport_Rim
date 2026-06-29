import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ProfileService } from '../../../core/services/profile.service';
import { extractApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-profil',
  imports: [FormsModule],
  templateUrl: './profil.html',
  styleUrl: './profil.scss',
})
export class Profil {
  private readonly auth = inject(AuthService);
  private readonly profileService = inject(ProfileService);

  protected readonly name = signal(this.auth.currentUser()?.name ?? '');
  protected readonly phoneNumber = signal(this.auth.currentUser()?.phoneNumber ?? '');
  protected readonly saving = signal(false);
  protected readonly success = signal(false);
  protected readonly error = signal('');

  protected submit(): void {
    this.saving.set(true);
    this.success.set(false);
    this.error.set('');

    this.profileService.updateProfile(this.name(), this.phoneNumber()).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set(true);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(extractApiErrorMessage(err, 'Une erreur est survenue lors de la mise à jour du profil.'));
      },
    });
  }
}
