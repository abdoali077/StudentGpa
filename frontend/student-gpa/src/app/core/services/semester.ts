
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_CONFIG } from '../config/api.config';

import {
  Semester,
  SaveSemesterRequest
} from '../models/semester.model';

@Injectable({
  providedIn: 'root'
})
export class SemesterService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${API_CONFIG.baseUrl}/semesters`;

  // Get All Semesters
  getAll(): Observable<Semester[]> {
    return this.http.get<Semester[]>(this.apiUrl);
  }

  // Get Semester By Id
  getById(id: number): Observable<Semester> {
    return this.http.get<Semester>(`${this.apiUrl}/${id}`);
  }

  // Create Semester
  create(request: SaveSemesterRequest): Observable<Semester> {
    return this.http.post<Semester>(this.apiUrl, request);
  }

  // Update Semester
  update(id: number, request: SaveSemesterRequest): Observable<Semester> {
    return this.http.put<Semester>(
      `${this.apiUrl}/${id}`,
      request
    );
  }

  // Delete Semester
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
