import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact',
  imports: [FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss',
})
export class Contact {
  protected readonly name = signal('');
  protected readonly email = signal('');
  protected readonly message = signal('');
  protected readonly submitted = signal(false);

  protected submit(): void {
    this.submitted.set(true);
    this.name.set('');
    this.email.set('');
    this.message.set('');
  }
}
