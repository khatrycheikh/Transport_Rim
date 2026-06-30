import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { jsPDF } from 'jspdf';
import { toDataURL } from 'qrcode';
import { extractApiErrorMessage } from '../../../core/utils/api-error';
import { Ticket as TicketModel } from '../../../core/models/ticket.model';
import { TicketService } from '../../../core/services/ticket.service';

@Component({
  selector: 'app-ticket',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './ticket.html',
  styleUrl: './ticket.scss',
})
export class Ticket {
  private readonly route = inject(ActivatedRoute);
  private readonly ticketService = inject(TicketService);

  protected readonly ticket = signal<TicketModel | null>(null);
  protected readonly qrCodeImage = signal('');
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly downloading = signal(false);

  constructor() {
    const reservationId = Number(this.route.snapshot.paramMap.get('reservationId'));
    this.ticketService.getByReservationId(reservationId).subscribe({
      next: async (ticket) => {
        this.ticket.set(ticket);
        this.qrCodeImage.set(await toDataURL(ticket.qrCodeData, { width: 220, margin: 1 }));
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiErrorMessage(err, 'Billet introuvable.'));
        this.loading.set(false);
      },
    });
  }

  protected async downloadPdf(): Promise<void> {
    const ticket = this.ticket();
    const qrImage = this.qrCodeImage();
    if (!ticket || !qrImage) return;

    this.downloading.set(true);
    const doc = new jsPDF({ unit: 'mm', format: [100, 150] });

    doc.setFontSize(16);
    doc.text('Billet Transport Rim', 50, 15, { align: 'center' });

    doc.setFontSize(11);
    const lines = [
      `${ticket.departureCity} -> ${ticket.arrivalCity}`,
      `Date : ${new Date(ticket.tripDate).toLocaleDateString('fr-FR')}`,
      `Passager : ${ticket.passengerName}`,
      `Places : ${ticket.reservedSeats}`,
      `Total : ${ticket.totalPrice} MRU`,
    ];
    lines.forEach((line, index) => doc.text(line, 10, 28 + index * 7));

    doc.addImage(qrImage, 'PNG', 30, 70, 40, 40);
    doc.setFontSize(8);
    doc.text(ticket.qrCodeData, 50, 115, { align: 'center' });

    doc.save(`billet-${ticket.reservationId}.pdf`);
    this.downloading.set(false);
  }
}
