import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { extractApiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly fullName = signal('');
  protected readonly phoneNumber = signal('');
  protected readonly password = signal('');
  protected readonly confirmPassword = signal('');
  protected readonly showPassword = signal(false);
  protected readonly showConfirmPassword = signal(false);
  protected readonly loading = signal(false);
  protected readonly error = signal('');

  protected submit(): void {
    if (this.password() !== this.confirmPassword()) {
      this.error.set('Les mots de passe ne correspondent pas.');
      return;
    }

    this.error.set('');
    this.loading.set(true);

    this.auth
      .register({
        name: this.fullName(),
        phoneNumber: this.phoneNumber(),
        password: this.password(),
        role: 'Traveler',
      })
      .subscribe({
        next: () => this.router.navigateByUrl('/connexion'),
        error: (err) => {
          this.loading.set(false);
          this.error.set(extractApiErrorMessage(err, "Une erreur est survenue lors de l'inscription."));
        },
      });
  }
}
