export interface Semester {
  id: number;
  code: string;
  name: string;
  startDate: string;
  endDate: string;
}

export interface SaveSemesterRequest {
  code: string;
  name: string;
  startDate: string;
  endDate: string;
}
