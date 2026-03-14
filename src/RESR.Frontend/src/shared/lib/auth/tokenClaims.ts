export const SUPER_ADMIN_ROLE_ID = 3;

interface TokenPayload {
  sub?: unknown;
  id_role?: unknown;
  permission?: unknown;
}

export interface AuthTokenClaims {
  userId: number | null;
  roleId: number | null;
  permissions: string[];
}

function parseNumericClaim(value: unknown) {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim() !== "") {
    const parsed = Number(value);

    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  return null;
}

function parsePermissionsClaim(value: unknown) {
  if (Array.isArray(value)) {
    return value.filter((entry): entry is string => typeof entry === "string" && entry.trim() !== "");
  }

  if (typeof value === "string" && value.trim() !== "") {
    return [value];
  }

  return [];
}

function decodePayloadSegment(segment: string) {
  const normalized = segment.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");

  return window.atob(padded);
}

export function getAuthTokenClaims(token: string | null | undefined): AuthTokenClaims {
  if (!token) {
    return {
      userId: null,
      roleId: null,
      permissions: []
    };
  }

  try {
    const payloadSegment = token.split(".")[1];

    if (!payloadSegment) {
      throw new Error("Missing JWT payload segment.");
    }

    const payload = JSON.parse(decodePayloadSegment(payloadSegment)) as TokenPayload;

    return {
      userId: parseNumericClaim(payload.sub),
      roleId: parseNumericClaim(payload.id_role),
      permissions: parsePermissionsClaim(payload.permission)
    };
  } catch {
    return {
      userId: null,
      roleId: null,
      permissions: []
    };
  }
}

export function isSuperAdminToken(token: string | null | undefined) {
  return getAuthTokenClaims(token).roleId === SUPER_ADMIN_ROLE_ID;
}
