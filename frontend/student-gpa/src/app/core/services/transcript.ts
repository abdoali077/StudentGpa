
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_CONFIG } from '../config/api.config';
import { Transcript } from '../models/transcript.model';

@Injectable({
  providedIn: 'root'
})
export class TranscriptService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${API_CONFIG.baseUrl}/gpa`;

  getStudentTranscript(studentId: number): Observable<Transcript> {
    return this.http.get<Transcript>(
      `${this.apiUrl}/students/${studentId}/transcript`
    );
  }
}
