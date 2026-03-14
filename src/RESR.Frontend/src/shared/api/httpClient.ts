import { env } from "@/shared/config/env";

export interface ApiErrorShape {
  message?: string;
}

export class ApiError extends Error {
  status: number;
  details: unknown;

  constructor(status: number, message: string, details?: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.details = details;
  }
}

interface RequestOptions extends Omit<RequestInit, "body"> {
  body?: unknown;
  token?: string | null;
}

class HttpClient {
  constructor(private readonly baseUrl: string) {}

  async get<T>(path: string, options: Omit<RequestOptions, "method" | "body"> = {}) {
    return this.request<T>(path, {
      ...options,
      method: "GET"
    });
  }

  async post<T>(path: string, body?: unknown, options: Omit<RequestOptions, "method" | "body"> = {}) {
    return this.request<T>(path, {
      ...options,
      method: "POST",
      body
    });
  }

  async patch<T>(path: string, body?: unknown, options: Omit<RequestOptions, "method" | "body"> = {}) {
    return this.request<T>(path, {
      ...options,
      method: "PATCH",
      body
    });
  }

  async delete<T>(path: string, options: Omit<RequestOptions, "method" | "body"> = {}) {
    return this.request<T>(path, {
      ...options,
      method: "DELETE"
    });
  }

  private async request<T>(path: string, options: RequestOptions): Promise<T> {
    const headers = new Headers(options.headers);
    headers.set("Accept", "application/json");

    const isFormData = options.body instanceof FormData;

    if (options.body !== undefined && !isFormData) {
      headers.set("Content-Type", "application/json");
    }

    if (options.token) {
      headers.set("Authorization", `Bearer ${options.token}`);
    }

    const response = await fetch(`${this.baseUrl}${path}`, {
      ...options,
      headers,
      body:
        options.body === undefined
          ? undefined
          : isFormData
            ? (options.body as FormData)
            : JSON.stringify(options.body)
    });

    const contentType = response.headers.get("content-type") ?? "";
    const payload = contentType.includes("application/json") ? await response.json() : null;

    if (!response.ok) {
      const error = payload as ApiErrorShape | null;
      throw new ApiError(response.status, error?.message ?? "The request failed.", payload);
    }

    return payload as T;
  }
}

export const httpClient = new HttpClient(env.apiBaseUrl);
