import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { StudentService } from '../../core/services/student';
import { Student, SaveStudentRequest } from '../../core/models/student.model';

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.html',
  styleUrl: './students.scss',
})
export class Students implements OnInit {

  private readonly studentService = inject(StudentService);
  private readonly cdr = inject(ChangeDetectorRef);

  students: Student[] = [];
  searchTerm = '';

  isLoading = false;
  isSaving = false;

  errorMessage = '';
  successMessage = '';

  showAddForm = false;
  isEditMode = false;
  editingStudentId: number | null = null;

  newStudent: SaveStudentRequest = {
    code: '',
    name: ''
  };

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.studentService.getAll().subscribe({
      next: (response) => {
        this.students = response.items;
        this.isLoading = false;
        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error loading students:', error);
        this.errorMessage = 'Failed to load students.';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }
  onSearch(): void {
  this.isLoading = true;
  this.errorMessage = '';

  this.studentService.getAll(1, 20, this.searchTerm).subscribe({
    next: (response) => {
      this.students = response.items;
      this.isLoading = false;
      this.cdr.markForCheck();
    },

    error: (error) => {
      console.error('Error searching students:', error);

      this.errorMessage = 'Failed to search students.';
      this.isLoading = false;

      this.cdr.markForCheck();
    }
  });
}

  openAddForm(): void {
    this.showAddForm = true;
    this.successMessage = '';
    this.errorMessage = '';
  }

  cancelAdd(): void {
    this.showAddForm = false;

    this.newStudent = {
      code: '',
      name: ''
    };
  }

  openEditForm(student: Student): void {
  this.isEditMode = true;
  this.editingStudentId = student.id;

  this.newStudent = {
    code: student.code,
    name: student.name
  };

  this.showAddForm = true;
  this.errorMessage = '';
  this.successMessage = '';
}

deleteStudent(student: Student): void {
  const confirmed = confirm(
    `Are you sure you want to delete ${student.name}?`
  );

  if (!confirmed) {
    return;
  }

  this.studentService.delete(student.id).subscribe({
    next: () => {
      this.students = this.students.filter(
        item => item.id !== student.id
      );

      this.successMessage = 'Student deleted successfully!';
      this.errorMessage = '';

      this.cdr.markForCheck();
    },

    error: (error) => {
      console.error('Error deleting student:', error);

      this.errorMessage =
        error.error?.message || 'Failed to delete student.';

      this.cdr.markForCheck();
    }
  });
}
updateStudent(): void {
  if (this.editingStudentId === null) {
    return;
  }

  if (!this.newStudent.code.trim() || !this.newStudent.name.trim()) {
    this.errorMessage = 'Please enter student code and name.';
    return;
  }

  const request: SaveStudentRequest = {
    code: this.newStudent.code.trim(),
    name: this.newStudent.name.trim()
  };

  this.isSaving = true;
  this.errorMessage = '';

  this.studentService.update(this.editingStudentId, request).subscribe({
    next: (updatedStudent) => {
      this.students = this.students.map(student =>
        student.id === updatedStudent.id ? updatedStudent : student
      );

      this.isSaving = false;
      this.showAddForm = false;
      this.isEditMode = false;
      this.editingStudentId = null;

      this.newStudent = { code: '', name: '' };
      this.successMessage = 'Student updated successfully!';

      this.cdr.markForCheck();
    },

    error: (error) => {
      console.error('Error updating student:', error);

      this.isSaving = false;
      this.errorMessage =
        error.error?.message || 'Failed to update student.';

      this.cdr.markForCheck();
    }
  });
}

  saveStudent(): void {
    if (!this.newStudent.code.trim() || !this.newStudent.name.trim()) {
      this.errorMessage = 'Please enter student code and name.';
      return;
    }

    const request: SaveStudentRequest = {
      code: this.newStudent.code.trim(),
      name: this.newStudent.name.trim()
    };

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.studentService.create(request).subscribe({
      next: (student) => {
        this.students = [...this.students, student];

        this.isSaving = false;
        this.showAddForm = false;

        this.newStudent = {
          code: '',
          name: ''
        };

        this.successMessage = 'Student added successfully!';

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Error creating student:', error);

        this.isSaving = false;
        this.errorMessage =
          error.error?.message || 'Failed to add student.';

        this.cdr.markForCheck();
      }
    });
  }
}
