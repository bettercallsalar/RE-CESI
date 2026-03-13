export function stripHtml(value: string) {
  if (typeof window !== "undefined") {
    const container = window.document.createElement("div");
    container.innerHTML = value;
    return container.textContent?.replace(/\s+/g, " ").trim() ?? "";
  }

  return value.replace(/<[^>]*>/g, " ").replace(/\s+/g, " ").trim();
}

export function hasMeaningfulArticleContent(value: string) {
  return stripHtml(value).length > 0;
}

export function getArticleTextLength(value: string) {
  return stripHtml(value).length;
}
