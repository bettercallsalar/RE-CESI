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

export function sanitizeArticleHtml(value: string) {
  if (typeof window === "undefined") {
    return value;
  }

  const template = window.document.createElement("template");
  template.innerHTML = value;

  template.content.querySelectorAll("script, style, iframe, object, embed").forEach((node) => {
    node.remove();
  });

  template.content.querySelectorAll("*").forEach((element) => {
    for (const attribute of Array.from(element.attributes)) {
      const name = attribute.name.toLowerCase();
      const content = attribute.value.trim().toLowerCase();

      if (name.startsWith("on")) {
        element.removeAttribute(attribute.name);
      }

      if ((name === "href" || name === "src") && content.startsWith("javascript:")) {
        element.removeAttribute(attribute.name);
      }
    }
  });

  return template.innerHTML;
}

export function getPrimaryArticleImage<T extends { idFile: number }>(article: { defaultImageId: number | null; files: T[] }) {
  if (article.defaultImageId) {
    return article.files.find((file) => file.idFile === article.defaultImageId) ?? article.files[0] ?? null;
  }

  return article.files[0] ?? null;
}
