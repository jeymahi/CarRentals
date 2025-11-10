// src/app/services/car-rental.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReservationRequestDto {
  customerId: string;
  carType: number;
  start: string;
  apiKey: string;
}

export interface Reservation {
  id: string;
  customerId: string;
  carType: number;
  start: string;
  end: string;
}

@Injectable({
  providedIn: 'root'
})
export class CarRental {
 
  private readonly baseUrl = 'https://localhost:7036/api/Reservations';

  constructor(private http: HttpClient) {}

  createReservation(req: ReservationRequestDto): Observable<Reservation> {
    return this.http.post<Reservation>(this.baseUrl, req);
  }

  getAll(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.baseUrl}/all`);
  }

  deleteReservation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
