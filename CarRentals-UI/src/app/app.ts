import { Component, ElementRef, ViewChild } from '@angular/core';
import { JsonPipe, CommonModule } from '@angular/common';
import { CarRental } from './services/car-rental';
import { NgForm, FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [CommonModule, JsonPipe, FormsModule]
})

export class AppComponent {
  title = 'Car Rental';
  error: string | null = null;
  success: string | null = null;
  reservations: any[] = [];
  @ViewChild("loadbtn") loadbtn!:ElementRef<HTMLButtonElement>;

  constructor(private carSvc: CarRental) {}

  customerId = '';
  carType: number | null = null;
  start = '';
  end = '';
  apiKey = 'secret-key-123';

  ngOnInit(){
   this.loadbtn.nativeElement.click();
  }

  submitReservation(form: NgForm) {
    if (form.invalid || !this.carType) {
      this.error = 'Please fill all required fields.';
      return;
    }

    const startDt = new Date(this.start);
    const endDt = new Date(this.end);

    if (isNaN(startDt.getTime()) || isNaN(endDt.getTime())) {
      this.error = 'Please select valid start and end date/time.';
      return;
    }

    

    if(startDt.getDate < new Date().getDate){
       this.error = 'Start date cannot be lesser than today.';
      return;
    }   
    else if (endDt <= startDt) {
      this.error = 'End date & time must be after start date & time.';
      return;
    }

    const req = {
      customerId: this.customerId,
      carType: this.carType,
      start: startDt.toISOString(),
      end: endDt.toISOString(),
      apiKey: this.apiKey
    };

    this.carSvc.createReservation(req).subscribe({
      next: (res) => {
        this.error = null;
       if(res.id){
       
        this.success = "Created successfully! "
        this.start ="";
        this.customerId="";
        this.carType=null;
        this.end=""
         this.loadbtn.nativeElement.click();
       }             
      },
      error: err => {
        this.error = err?.error ?? 'Something went wrong';
         this.loadbtn.nativeElement.click();
      }
    });
  }

  loadAll() {
    this.carSvc.getAll().subscribe({
      next: res => {
        this.reservations = res;
        this.error = null;
      },
      error: err => {
        this.error = err?.error ?? 'Failed to load reservations';
      }
    });
  }
}
