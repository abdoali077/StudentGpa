
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { StudentService } from '../../core/services/student';
import { TranscriptService } from '../../core/services/transcript';

import { Student } from '../../core/models/student.model';
import { Transcript } from '../../core/models/transcript.model';

@Component({
  selector: 'app-transcript',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transcript.html',
  styleUrl: './transcript.scss'
})
export class TranscriptComponent implements OnInit {

  private readonly studentService = inject(StudentService);
  private readonly transcriptService = inject(TranscriptService);
  private readonly cdr = inject(ChangeDetectorRef);

  students: Student[] = [];

  selectedStudentId: number | null = null;

  transcript: Transcript | null = null;

  loadingStudents = false;
  loadingTranscript = false;

  errorMessage = '';

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.loadingStudents = true;
    this.errorMessage = '';

    this.studentService.getAll(1, 100).subscribe({
      next: response => {
        this.students = response.items;
        this.loadingStudents = false;

        if (this.students.length > 0) {
          this.selectedStudentId = this.students[0].id;
          this.loadTranscript();
        }

        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Load Students Error:', error);

        this.loadingStudents = false;
        this.errorMessage = 'Failed to load students.';

        this.cdr.markForCheck();
      }
    });
  }

  onStudentChange(): void {
    this.transcript = null;
    this.errorMessage = '';

    if (this.selectedStudentId !== null) {
      this.loadTranscript();
    }
  }

  loadTranscript(): void {
    if (this.selectedStudentId === null) {
      return;
    }

    this.loadingTranscript = true;
    this.errorMessage = '';
    this.transcript = null;

    this.transcriptService
      .getStudentTranscript(this.selectedStudentId)
      .subscribe({
        next: response => {
          this.transcript = response;
          this.loadingTranscript = false;

          this.cdr.markForCheck();
        },
        error: error => {
          console.error('Load Transcript Error:', error);
          console.error('Backend Response:', error.error);

          this.loadingTranscript = false;

          this.errorMessage =
            error.error?.message ||
            error.error?.title ||
            'Failed to load student transcript.';

          this.cdr.markForCheck();
        }
      });
  }

  getGradeClass(grade: string): string {
    if (grade === 'A+' || grade === 'A' || grade === 'A-') {
      return 'bg-success';
    }

    if (grade === 'B+' || grade === 'B' || grade === 'B-') {
      return 'bg-primary';
    }

    if (grade === 'C+' || grade === 'C' || grade === 'C-') {
      return 'bg-info text-dark';
    }

    if (grade === 'D+' || grade === 'D') {
      return 'bg-warning text-dark';
    }

    if (grade === 'F' || grade === 'ABS') {
      return 'bg-danger';
    }

    return 'bg-secondary';
  }
}
