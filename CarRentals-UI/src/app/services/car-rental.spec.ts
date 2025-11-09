import { TestBed } from '@angular/core/testing';

import { CarRental } from './car-rental';

describe('CarRental', () => {
  let service: CarRental;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CarRental);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
