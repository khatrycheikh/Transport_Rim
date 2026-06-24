import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  protected readonly email = signal('');
  protected readonly password = signal('');

  protected submit(): void {
    console.log('Connexion', { email: this.email(), password: this.password() });
  }
}
