
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { CourseService } from '../../core/services/course';

import {
  Course,
  SaveCourseRequest
} from '../../core/models/course.model';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses implements OnInit {

  private readonly courseService = inject(CourseService);
  private readonly cdr = inject(ChangeDetectorRef);

  courses: Course[] = [];

  isLoading = false;
  isSaving = false;

  errorMessage = '';
  successMessage = '';

  showAddForm = false;

  isEditMode = false;
  editingCourseId: number | null = null;

  searchTerm = '';

  newCourse: SaveCourseRequest = {
    code: '',
    name: '',
    creditHours: 3
  };

  ngOnInit(): void {
    this.loadCourses();
  }

  // Load Courses
  loadCourses(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.courseService.getAll(1, 20, this.searchTerm).subscribe({
      next: (response) => {
        this.courses = response.items;
        this.isLoading = false;

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error loading courses:', error);

        this.errorMessage = 'Failed to load courses.';
        this.isLoading = false;

        this.cdr.markForCheck();
      }
    });
  }

  // Search Courses
  onSearch(): void {
    this.loadCourses();
  }

  // Open Add Form
  openAddForm(): void {
    this.isEditMode = false;
    this.editingCourseId = null;

    this.newCourse = {
      code: '',
      name: '',
      creditHours: 3
    };

    this.showAddForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Open Edit Form
  openEditForm(course: Course): void {
    this.isEditMode = true;
    this.editingCourseId = course.id;

    this.newCourse = {
      code: course.code,
      name: course.name,
      creditHours: course.creditHours
    };

    this.showAddForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Cancel Form
  cancelAdd(): void {
    this.showAddForm = false;
    this.isEditMode = false;
    this.editingCourseId = null;

    this.newCourse = {
      code: '',
      name: '',
      creditHours: 3
    };

    this.errorMessage = '';
  }

  // Create Course
  saveCourse(): void {
    if (
      !this.newCourse.code.trim() ||
      !this.newCourse.name.trim()
    ) {
      this.errorMessage = 'Please enter course code and name.';
      return;
    }

    const request: SaveCourseRequest = {
      code: this.newCourse.code.trim(),
      name: this.newCourse.name.trim(),
      creditHours: Number(this.newCourse.creditHours)
    };

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.courseService.create(request).subscribe({
      next: (course) => {
        this.isSaving = false;
        this.showAddForm = false;

        this.newCourse = {
          code: '',
          name: '',
          creditHours: 3
        };

        this.successMessage = 'Course added successfully!';

        // Reload to preserve API sorting and pagination.
        this.loadCourses();

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error creating course:', error);

        this.isSaving = false;

        this.errorMessage =
          error.error?.message || 'Failed to add course.';

        this.cdr.markForCheck();
      }
    });
  }

  // Update Course
  updateCourse(): void {
    if (this.editingCourseId === null) {
      return;
    }

    if (
      !this.newCourse.code.trim() ||
      !this.newCourse.name.trim()
    ) {
      this.errorMessage = 'Please enter course code and name.';
      return;
    }

    const request: SaveCourseRequest = {
      code: this.newCourse.code.trim(),
      name: this.newCourse.name.trim(),
      creditHours: Number(this.newCourse.creditHours)
    };

    this.isSaving = true;
    this.errorMessage = '';

    this.courseService.update(
      this.editingCourseId,
      request
    ).subscribe({
      next: () => {
        this.isSaving = false;
        this.showAddForm = false;
        this.isEditMode = false;
        this.editingCourseId = null;

        this.newCourse = {
          code: '',
          name: '',
          creditHours: 3
        };

        this.successMessage = 'Course updated successfully!';

        this.loadCourses();

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error updating course:', error);

        this.isSaving = false;

        this.errorMessage =
          error.error?.message || 'Failed to update course.';

        this.cdr.markForCheck();
      }
    });
  }

  // Delete Course
  deleteCourse(course: Course): void {
    const confirmed = confirm(
      `Are you sure you want to delete ${course.name}?`
    );

    if (!confirmed) {
      return;
    }

    this.courseService.delete(course.id).subscribe({
      next: () => {
        this.successMessage = 'Course deleted successfully!';
        this.errorMessage = '';

        this.loadCourses();

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error deleting course:', error);

        this.errorMessage =
          error.error?.message || 'Failed to delete course.';

        this.cdr.markForCheck();
      }
    });
  }
}
