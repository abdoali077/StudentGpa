
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_CONFIG } from '../config/api.config';

import {
  Course,
  SaveCourseRequest,
  PagedResult
} from '../models/course.model';

@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${API_CONFIG.baseUrl}/courses`;

  getAll(
    page: number = 1,
    pageSize: number = 20,
    search: string = ''
  ): Observable<PagedResult<Course>> {

    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResult<Course>>(
      this.apiUrl,
      { params }
    );
  }

  getById(id: number): Observable<Course> {
    return this.http.get<Course>(`${this.apiUrl}/${id}`);
  }

  create(request: SaveCourseRequest): Observable<Course> {
    return this.http.post<Course>(this.apiUrl, request);
  }

  update(id: number, request: SaveCourseRequest): Observable<Course> {
    return this.http.put<Course>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
