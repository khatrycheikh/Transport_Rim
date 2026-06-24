import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  protected readonly fullName = signal('');
  protected readonly email = signal('');
  protected readonly password = signal('');

  protected submit(): void {
    console.log('Inscription', { fullName: this.fullName(), email: this.email(), password: this.password() });
  }
}
