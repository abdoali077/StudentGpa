
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_CONFIG } from '../config/api.config';

import {
  Enrollment,
  CreateEnrollmentRequest,
  UpdateEnrollmentGradeRequest
} from '../models/enrollment.model';

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${API_CONFIG.baseUrl}/enrollments`;

  // Get all enrollments for a specific student
  getByStudent(studentId: number): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(
      `${this.apiUrl}/students/${studentId}`
    );
  }

  // Register a student in a course
  create(request: CreateEnrollmentRequest): Observable<Enrollment> {
    return this.http.post<Enrollment>(
      this.apiUrl,
      request
    );
  }

  // Update enrollment grade or special status
  updateGrade(
    enrollmentId: number,
    request: UpdateEnrollmentGradeRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${enrollmentId}/grade`,
      request
    );
  }

  // Delete an enrollment
  delete(enrollmentId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${enrollmentId}`
    );
  }
}
