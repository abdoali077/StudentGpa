
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { SemesterService } from '../../core/services/semester';

import {
  Semester,
  SaveSemesterRequest
} from '../../core/models/semester.model';

@Component({
  selector: 'app-semesters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './semesters.html',
  styleUrl: './semesters.scss',
})
export class Semesters implements OnInit {

  private readonly semesterService = inject(SemesterService);
  private readonly cdr = inject(ChangeDetectorRef);

  semesters: Semester[] = [];

  isLoading = false;
  isSaving = false;

  errorMessage = '';
  successMessage = '';

  showAddForm = false;

  isEditMode = false;
  editingSemesterId: number | null = null;

  searchTerm = '';

  newSemester: SaveSemesterRequest = {
    code: '',
    name: '',
    startDate: '',
    endDate: ''
  };

  ngOnInit(): void {
    this.loadSemesters();
  }

  // Get filtered semesters
  get filteredSemesters(): Semester[] {
    const search = this.searchTerm.trim().toLowerCase();

    if (!search) {
      return this.semesters;
    }

    return this.semesters.filter(semester =>
      semester.code.toLowerCase().includes(search) ||
      semester.name.toLowerCase().includes(search)
    );
  }

  // Load Semesters
  loadSemesters(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.semesterService.getAll().subscribe({
      next: (response) => {
        this.semesters = response;
        this.isLoading = false;

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error loading semesters:', error);

        this.errorMessage = 'Failed to load semesters.';
        this.isLoading = false;

        this.cdr.markForCheck();
      }
    });
  }

  // Search Semesters
  onSearch(): void {
    // Filtering is handled by filteredSemesters getter.
  }

  // Open Add Form
  openAddForm(): void {
    this.isEditMode = false;
    this.editingSemesterId = null;

    this.newSemester = {
      code: '',
      name: '',
      startDate: '',
      endDate: ''
    };

    this.showAddForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Open Edit Form
  openEditForm(semester: Semester): void {
    this.isEditMode = true;
    this.editingSemesterId = semester.id;

    this.newSemester = {
      code: semester.code,
      name: semester.name,
      startDate: semester.startDate,
      endDate: semester.endDate
    };

    this.showAddForm = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Cancel Form
  cancelAdd(): void {
    this.showAddForm = false;
    this.isEditMode = false;
    this.editingSemesterId = null;

    this.newSemester = {
      code: '',
      name: '',
      startDate: '',
      endDate: ''
    };

    this.errorMessage = '';
  }

  // Validate Form Data
  private validateSemester(): boolean {

    if (
      !this.newSemester.code.trim() ||
      !this.newSemester.name.trim() ||
      !this.newSemester.startDate ||
      !this.newSemester.endDate
    ) {
      this.errorMessage = 'Please fill in all fields.';
      return false;
    }

    if (this.newSemester.startDate >= this.newSemester.endDate) {
      this.errorMessage = 'End date must be after start date.';
      return false;
    }

    return true;
  }

  // Create Semester
  saveSemester(): void {

    if (!this.validateSemester()) {
      return;
    }

    const request: SaveSemesterRequest = {
      code: this.newSemester.code.trim(),
      name: this.newSemester.name.trim(),
      startDate: this.newSemester.startDate,
      endDate: this.newSemester.endDate
    };

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.semesterService.create(request).subscribe({
      next: () => {
        this.isSaving = false;
        this.showAddForm = false;

        this.newSemester = {
          code: '',
          name: '',
          startDate: '',
          endDate: ''
        };

        this.successMessage = 'Semester added successfully!';

        this.loadSemesters();
        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error creating semester:', error);

        this.isSaving = false;

        this.errorMessage =
          error.error?.message || 'Failed to add semester.';

        this.cdr.markForCheck();
      }
    });
  }

  // Update Semester
  updateSemester(): void {

    if (this.editingSemesterId === null) {
      return;
    }

    if (!this.validateSemester()) {
      return;
    }

    const request: SaveSemesterRequest = {
      code: this.newSemester.code.trim(),
      name: this.newSemester.name.trim(),
      startDate: this.newSemester.startDate,
      endDate: this.newSemester.endDate
    };

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.semesterService.update(
      this.editingSemesterId,
      request
    ).subscribe({
      next: () => {
        this.isSaving = false;
        this.showAddForm = false;
        this.isEditMode = false;
        this.editingSemesterId = null;

        this.newSemester = {
          code: '',
          name: '',
          startDate: '',
          endDate: ''
        };

        this.successMessage = 'Semester updated successfully!';

        this.loadSemesters();
        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error updating semester:', error);

        this.isSaving = false;

        this.errorMessage =
          error.error?.message || 'Failed to update semester.';

        this.cdr.markForCheck();
      }
    });
  }

  // Delete Semester
  deleteSemester(semester: Semester): void {

    const confirmed = confirm(
      `Are you sure you want to delete ${semester.name}?`
    );

    if (!confirmed) {
      return;
    }

    this.semesterService.delete(semester.id).subscribe({
      next: () => {
        this.successMessage = 'Semester deleted successfully!';
        this.errorMessage = '';

        this.loadSemesters();
        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error deleting semester:', error);

        this.errorMessage =
          error.error?.message || 'Failed to delete semester.';

        this.cdr.markForCheck();
      }
    });
  }
}
