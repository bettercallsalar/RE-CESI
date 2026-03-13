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
  department: Department;
  idRole: number;
}
