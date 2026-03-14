import { env } from "@/shared/config/env";

export interface ApiErrorShape {
  message?: string;
  detail?: string;
  title?: string;
  errors?: Record<string, string[]>;
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
    const payload = await this.readResponsePayload(response, contentType);

    if (!response.ok) {
      throw new ApiError(response.status, this.getApiErrorMessage(payload), payload);
    }

    return payload as T;
  }

  private async readResponsePayload(response: Response, contentType: string) {
    if (response.status === 204 || response.status === 205) {
      return null;
    }

    const rawBody = await response.text();

    if (!rawBody) {
      return null;
    }

    if (contentType.includes("json")) {
      try {
        return JSON.parse(rawBody) as unknown;
      } catch {
        return rawBody;
      }
    }

    return rawBody;
  }

  private getApiErrorMessage(payload: unknown) {
    if (typeof payload === "string" && payload.trim()) {
      return payload.trim();
    }

    if (payload && typeof payload === "object") {
      const error = payload as ApiErrorShape;

      if (typeof error.message === "string" && error.message.trim()) {
        return error.message.trim();
      }

      const firstValidationError = Object.values(error.errors ?? {})
        .flat()
        .find((value) => typeof value === "string" && value.trim());

      if (firstValidationError) {
        return firstValidationError.trim();
      }

      if (typeof error.detail === "string" && error.detail.trim()) {
        return error.detail.trim();
      }

      if (typeof error.title === "string" && error.title.trim()) {
        return error.title.trim();
      }
    }

    return "The request failed.";
  }
}

export const httpClient = new HttpClient(env.apiBaseUrl);
