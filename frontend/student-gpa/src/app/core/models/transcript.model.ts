
export interface TranscriptCourse {
  enrollmentId: number;
  courseId: number;
  courseCode: string;
  courseName: string;
  creditHours: number;
  score: number | null;
  grade: string;
  gradePoints: number | null;
  qualityPoints: number | null;
  isGpaEligible: boolean;
}

export interface SemesterTranscript {
  semesterId: number;
  semesterCode: string;
  semesterName: string;
  startDate: string;
  courses: TranscriptCourse[];
  registeredCreditHours: number;
  eligibleCreditHours: number;
  totalQualityPoints: number;
  gpa: number | null;
  gpaDisplay: string;
}

export interface Transcript {
  studentId: number;
  studentCode: string;
  studentName: string;
  semesters: SemesterTranscript[];
  totalRegisteredCreditHours: number;
  totalEligibleCreditHours: number;
  totalQualityPoints: number;
  cumulativeGpa: number | null;
  cumulativeGpaDisplay: string;
}
