import type { Department, User } from "@/shared/types/user";

export interface UpdateOwnProfilePayload {
  username?: string;
  email?: string;
  firstName?: string;
  birthDate?: string | null;
  bio?: string | null;
  idDepartment?: number;
}

export interface ProfileFormValues {
  username: string;
  email: string;
  firstName: string;
  birthDate: string;
  bio: string;
  idDepartment: number | "";
}

export interface ProfilePageData {
  user: User;
  departments: Department[];
}
