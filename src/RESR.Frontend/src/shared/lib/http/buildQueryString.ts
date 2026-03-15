export type QueryParamValue = string | number | boolean | null | undefined;

export function buildQueryString<T extends object>(query: T) {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query) as Array<[string, QueryParamValue]>) {
    if (value === undefined || value === null || value === "") {
      continue;
    }

    params.set(key, String(value));
  }

  const raw = params.toString();
  return raw ? `?${raw}` : "";
}
