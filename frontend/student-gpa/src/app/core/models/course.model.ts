export interface Course {
  id: number;
  code: string;
  name: string;
  creditHours: number;
}

export interface SaveCourseRequest {
  code: string;
  name: string;
  creditHours: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
