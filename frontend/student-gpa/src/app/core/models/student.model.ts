export interface Student {
  id: number;
  code: string;
  name: string;
}

export interface SaveStudentRequest {
  code: string;
  name: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
