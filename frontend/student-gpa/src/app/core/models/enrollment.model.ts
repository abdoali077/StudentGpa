
export enum AcademicStatus {
  Absent = 1,
  Withdrawn = 2,
  Incomplete = 3
}

export interface Enrollment {
  id: number;

  studentId: number;
  studentCode: string;
  studentName: string;

  semesterId: number;
  semesterCode: string;
  semesterName: string;

  courseId: number;
  courseCode: string;
  courseName: string;

  creditHours: number;

  score: number | null;
  specialStatus: AcademicStatus | null;

  grade: string;
  gradePoints: number | null;
  qualityPoints: number | null;

  isGpaEligible: boolean;
}

export interface CreateEnrollmentRequest {
  studentId: number;
  semesterId: number;
  courseId: number;
  score: number | null;
  specialStatus: AcademicStatus | null;
}

export interface UpdateEnrollmentGradeRequest {
  score: number | null;
  specialStatus: AcademicStatus | null;
}
