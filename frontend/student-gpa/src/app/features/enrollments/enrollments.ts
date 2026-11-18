
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { StudentService } from '../../core/services/student';
import { CourseService } from '../../core/services/course';
import { SemesterService } from '../../core/services/semester';
import { EnrollmentService } from '../../core/services/enrollment';

import { Student } from '../../core/models/student.model';
import { Course } from '../../core/models/course.model';
import { Semester } from '../../core/models/semester.model';

import {
  Enrollment,
  AcademicStatus,
  CreateEnrollmentRequest,
  UpdateEnrollmentGradeRequest
} from '../../core/models/enrollment.model';

@Component({
  selector: 'app-enrollments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './enrollments.html',
  styleUrl: './enrollments.scss'
})
export class Enrollments implements OnInit {

  private readonly studentService = inject(StudentService);
  private readonly courseService = inject(CourseService);
  private readonly semesterService = inject(SemesterService);
  private readonly enrollmentService = inject(EnrollmentService);
  private readonly cdr = inject(ChangeDetectorRef);

  students: Student[] = [];
  courses: Course[] = [];
  semesters: Semester[] = [];
  enrollments: Enrollment[] = [];

  selectedStudentId: number | null = null;

  loading = false;
  saving = false;

  errorMessage = '';
  successMessage = '';

  showAddForm = false;
  showGradeForm = false;

  editingEnrollmentId: number | null = null;

  newEnrollment: CreateEnrollmentRequest = {
    studentId: 0,
    semesterId: 0,
    courseId: 0,
    score: null,
    specialStatus: null
  };

  gradeForm: UpdateEnrollmentGradeRequest = {
    score: null,
    specialStatus: null
  };

  readonly AcademicStatus = AcademicStatus;

  ngOnInit(): void {
    this.loadStudents();
    this.loadCourses();
    this.loadSemesters();
  }

  loadStudents(): void {
    this.studentService.getAll(1, 20).subscribe({
      next: response => {
        this.students = response.items;

        if (this.students.length > 0 && this.selectedStudentId === null) {
          this.selectedStudentId = this.students[0].id;
          this.loadEnrollments();
        }

        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Failed to load students.';
        this.cdr.markForCheck();
      }
    });
  }

  loadCourses(): void {
    this.courseService.getAll(1, 100).subscribe({
      next: response => {
        this.courses = response.items;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Failed to load courses.';
        this.cdr.markForCheck();
      }
    });
  }

  loadSemesters(): void {
    this.semesterService.getAll().subscribe({
      next: response => {
        this.semesters = response;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Failed to load semesters.';
        this.cdr.markForCheck();
      }
    });
  }

  onStudentChange(): void {
    this.enrollments = [];
    this.showAddForm = false;
    this.showGradeForm = false;

    if (this.selectedStudentId !== null) {
      this.loadEnrollments();
    }
  }

  loadEnrollments(): void {
    if (this.selectedStudentId === null) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.enrollmentService.getByStudent(this.selectedStudentId).subscribe({
      next: response => {
        this.enrollments = response;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Failed to load enrollments.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  openAddForm(): void {
    if (this.selectedStudentId === null) {
      this.errorMessage = 'Please select a student first.';
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.showGradeForm = false;
    this.showAddForm = true;

    this.newEnrollment = {
      studentId: this.selectedStudentId,
      semesterId: 0,
      courseId: 0,
      score: null,
      specialStatus: null
    };
  }

  cancelAdd(): void {
    this.showAddForm = false;
  }
  get selectedSemesterRegisteredHours(): number {
  if (!this.newEnrollment.semesterId) {
    return 0;
  }

  return this.enrollments
    .filter(
      enrollment =>
        enrollment.semesterId === this.newEnrollment.semesterId
    )
    .reduce(
      (total, enrollment) => total + enrollment.creditHours,
      0
    );
}

  saveEnrollment(): void {
  this.errorMessage = '';
  this.successMessage = '';

  if (
    !this.newEnrollment.semesterId ||
    !this.newEnrollment.courseId
  ) {
    this.errorMessage = 'Please select a semester and a course.';
    return;
  }

  // Get selected course
  const selectedCourse = this.courses.find(
    course => course.id === this.newEnrollment.courseId
  );

  if (!selectedCourse) {
    this.errorMessage = 'Selected course was not found.';
    return;
  }

  // Calculate registered hours in the selected semester only
  const registeredHours = this.enrollments
    .filter(
      enrollment =>
        enrollment.semesterId === this.newEnrollment.semesterId
    )
    .reduce(
      (total, enrollment) => total + enrollment.creditHours,
      0
    );



  // Calculate total hours after adding the new course
  const totalHoursAfterEnrollment =
    registeredHours + selectedCourse.creditHours;

  // Check the 18-hour limit
  if (totalHoursAfterEnrollment > 18) {
    this.errorMessage =
      `Cannot enroll in this course. The maximum allowed is 18 credit hours per semester. ` +
      `Currently registered: ${registeredHours} hours. ` +
      `Selected course: ${selectedCourse.creditHours} hours.`;

    return;
  }

  const request: CreateEnrollmentRequest = {
    ...this.newEnrollment,
    studentId: this.selectedStudentId!
  };

  this.saving = true;

  this.enrollmentService.create(request).subscribe({
    next: () => {
      this.saving = false;
      this.showAddForm = false;
      this.successMessage = 'Enrollment created successfully.';
      this.loadEnrollments();
      this.cdr.markForCheck();
    },
    error: error => {
      this.saving = false;

      console.error('Create Enrollment Error:', error);
      console.error('Backend Response:', error.error);

      this.errorMessage =
        error.error?.message ||
        error.error?.title ||
        'Failed to create enrollment.';

      this.cdr.markForCheck();
    }
  });
}

  openGradeForm(enrollment: Enrollment): void {
    this.errorMessage = '';
    this.successMessage = '';

    this.showAddForm = false;
    this.showGradeForm = true;

    this.editingEnrollmentId = enrollment.id;

    this.gradeForm = {
      score: enrollment.score,
      specialStatus: enrollment.specialStatus
    };
  }

  cancelGradeEdit(): void {
    this.showGradeForm = false;
    this.editingEnrollmentId = null;
  }

  updateGrade(): void {
    if (this.editingEnrollmentId === null) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    const request: UpdateEnrollmentGradeRequest = {
      ...this.gradeForm
    };

    this.saving = true;

    this.enrollmentService.updateGrade(
      this.editingEnrollmentId,
      request
    ).subscribe({
      next: () => {
        this.saving = false;
        this.showGradeForm = false;
        this.editingEnrollmentId = null;
        this.successMessage = 'Grade updated successfully.';
        this.loadEnrollments();
        this.cdr.markForCheck();
      },
      error: error => {
        this.saving = false;

        console.error('Update Grade Error:', error);
        console.error('Backend Response:', error.error);
        console.error('Request Body:', request);

        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          error.message ||
          'Failed to update grade.';

        this.cdr.markForCheck();
      }
    });
  }

  deleteEnrollment(enrollment: Enrollment): void {
    const confirmed = confirm(
      `Delete ${enrollment.courseCode} from ${enrollment.semesterName}?`
    );

    if (!confirmed) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    this.enrollmentService.delete(enrollment.id).subscribe({
      next: () => {
        this.successMessage = 'Enrollment deleted successfully.';
        this.loadEnrollments();
      },
      error: error => {
        console.error('Delete Enrollment Error:', error);

        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          'Failed to delete enrollment.';

        this.cdr.markForCheck();
      }
    });
  }

  get totalRegisteredHours(): number {
    return this.enrollments.reduce(
      (total, enrollment) => total + enrollment.creditHours,
      0
    );
  }

  get selectedStudent(): Student | undefined {
    return this.students.find(
      student => student.id === this.selectedStudentId
    );
  }

  get availableCourses(): Course[] {
    return this.courses.filter(course =>
      !this.enrollments.some(
        enrollment =>
          enrollment.courseId === course.id &&
          enrollment.semesterId === this.newEnrollment.semesterId
      )
    );
  }

  getGradeLabel(status: AcademicStatus | null): string {
    switch (status) {
      case AcademicStatus.Absent:
        return 'Absent';
      case AcademicStatus.Withdrawn:
        return 'Withdrawn';
      case AcademicStatus.Incomplete:
        return 'Incomplete';
      default:
        return 'Normal Grade';
    }
  }
}
