import type { ResourceComment } from "@/shared/types/comment";

export interface CommentThreadNode {
  comment: ResourceComment;
  children: CommentThreadNode[];
}

export function buildCommentTree(comments: ResourceComment[]) {
  const nodes = new Map<number, CommentThreadNode>();
  const roots: CommentThreadNode[] = [];

  for (const comment of comments) {
    nodes.set(comment.idComment, {
      comment,
      children: []
    });
  }

  for (const comment of comments) {
    const node = nodes.get(comment.idComment);

    if (!node) {
      continue;
    }

    if (comment.idParentComment && nodes.has(comment.idParentComment)) {
      nodes.get(comment.idParentComment)?.children.push(node);
      continue;
    }

    roots.push(node);
  }

  return roots;
}
