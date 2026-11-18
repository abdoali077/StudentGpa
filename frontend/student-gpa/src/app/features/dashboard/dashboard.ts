
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';

import { StudentService } from '../../core/services/student';
import { CourseService } from '../../core/services/course';
import { SemesterService } from '../../core/services/semester';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private readonly studentService = inject(StudentService);
  private readonly courseService = inject(CourseService);
  private readonly semesterService = inject(SemesterService);
  private readonly cdr = inject(ChangeDetectorRef);

  totalStudents = 0;
  totalCourses = 0;
  totalSemesters = 0;

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.errorMessage = '';

    // Load total students
    this.studentService.getAll(1, 1).subscribe({
      next: response => {
        this.totalStudents = response.totalCount;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Students Dashboard Error:', error);
        this.errorMessage = 'Failed to load students statistics.';
        this.cdr.markForCheck();
      }
    });

    // Load total courses
    this.courseService.getAll(1, 1).subscribe({
      next: response => {
        this.totalCourses = response.totalCount;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Courses Dashboard Error:', error);
        this.errorMessage = 'Failed to load courses statistics.';
        this.cdr.markForCheck();
      }
    });

    // Load total semesters
    this.semesterService.getAll().subscribe({
      next: response => {
        this.totalSemesters = response.length;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Semesters Dashboard Error:', error);
        this.errorMessage = 'Failed to load semesters statistics.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }
}
