export interface Department {
  idDepartment: number;
  name: string;
  code: string;
}

export interface User {
  idUser: number;
  username: string;
  email: string;
  firstName: string;
  birthDate: string | null;
  bio: string | null;
  isVerified: boolean;
  isBanned: boolean;
  department: Department;
  idRole: number;
}

export interface PaginatedUsersResponse {
  items: User[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
