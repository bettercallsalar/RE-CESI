export interface Permission {
  idPermission: number;
  name: string;
  description: string | null;
}

export interface Role {
  idRole: number;
  name: string;
  description: string | null;
  permissions: Permission[];
}

export interface RoleSummary {
  idRole: number;
  name: string;
  description: string | null;
}
