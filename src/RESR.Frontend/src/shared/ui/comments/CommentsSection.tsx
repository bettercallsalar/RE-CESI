import { Badge, Button, Card, CardBody, Heading, HStack, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { commentsService } from "@/shared/api/comments.service";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import { buildCommentTree, type CommentThreadNode } from "@/shared/lib/comments/buildCommentTree";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import type { ResourceComment } from "@/shared/types/comment";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { CommentComposer } from "./CommentComposer";

interface CommentsSectionProps {
  idResource: number;
  resourceOwnerId?: number;
}

interface CommentItemProps {
  node: CommentThreadNode;
  depth: number;
  currentUserId: number | null;
  resourceOwnerId?: number;
  isAuthenticated: boolean;
  canModerateComments: boolean;
  isBusy: boolean;
  pendingAction: string | null;
  onReply: (parentCommentId: number, content: string) => Promise<boolean>;
  onDelete: (comment: ResourceComment, actionKey: string) => Promise<void>;
}

function formatCommentDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

function getCommentAuthorLabel(comment: ResourceComment) {
  const username = comment.author?.username?.trim();
  const firstName = comment.author?.firstName?.trim();
  return username || firstName || `utilisateur #${comment.idUser}`;
}

function getCommentCountLabel(count: number) {
  return count > 1 ? `${count} commentaires` : `${count} commentaire`;
}

function CommentItem({
  node,
  depth,
  currentUserId,
  resourceOwnerId,
  isAuthenticated,
  canModerateComments,
  isBusy,
  pendingAction,
  onReply,
  onDelete
}: CommentItemProps) {
  const [isReplying, setIsReplying] = useState(false);
  const { comment, children } = node;
  const isDeleted = Boolean(comment.deletedAt);
  const isOwnComment = currentUserId === comment.idUser;
  const canDelete = !isDeleted && (isOwnComment || canModerateComments);
  const canReply = isAuthenticated && !isDeleted;
  const replyActionKey = `reply-${comment.idComment}`;
  const deleteActionKey = `delete-${comment.idComment}`;

  async function handleReply(content: string) {
    const isSuccessful = await onReply(comment.idComment, content);

    if (isSuccessful) {
      setIsReplying(false);
    }

    return isSuccessful;
  }

  return (
    <Stack
      borderLeft={depth > 0 ? "2px solid" : undefined}
      borderColor={depth > 0 ? "canvas.200" : undefined}
      pl={depth > 0 ? { base: 4, md: 6 } : 0}
      spacing={4}
    >
      <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
        <CardBody p={{ base: 4, md: 5 }}>
          <Stack spacing={4}>
            <HStack align="start" justify="space-between" spacing={4} wrap="wrap">
              <Stack spacing={1}>
                <HStack spacing={2} wrap="wrap">
                  <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                    {getCommentAuthorLabel(comment)}
                  </Text>

                  {resourceOwnerId === comment.idUser ? (
                    <Badge bg="brand.500" color="white" fontSize="11px" px={2} py={0.5} rounded="full">
                      Auteur
                    </Badge>
                  ) : null}

                  {isDeleted ? (
                    <Badge bg="canvas.200" color="ink.800" fontSize="11px" px={2} py={0.5} rounded="full">
                      Supprime
                    </Badge>
                  ) : null}
                </HStack>

                <Text color="ink.500" fontSize="13px">
                  {formatCommentDate(comment.createdAt)}
                  {comment.modifiedAt ? " • modifie" : ""}
                </Text>
              </Stack>

              {!isDeleted ? (
                <HStack spacing={3} wrap="wrap">
                  {canReply ? (
                    <Button
                      isDisabled={isBusy}
                      onClick={() => setIsReplying((current) => !current)}
                      size="sm"
                      variant="ghost"
                    >
                      {isReplying ? "Fermer" : "Repondre"}
                    </Button>
                  ) : null}

                  {canDelete ? (
                    <Button
                      bg={isOwnComment ? "red.50" : "red.100"}
                      color="red.700"
                      isDisabled={isBusy}
                      isLoading={pendingAction === deleteActionKey}
                      onClick={() => void onDelete(comment, deleteActionKey)}
                      size="sm"
                      _hover={{ bg: isOwnComment ? "red.100" : "red.200" }}
                    >
                      Supprimer
                    </Button>
                  ) : null}
                </HStack>
              ) : null}
            </HStack>

            <Text color={isDeleted ? "ink.500" : "ink.800"} fontStyle={isDeleted ? "italic" : "normal"} lineHeight="1.7" whiteSpace="pre-wrap">
              {isDeleted ? "Ce commentaire a ete supprime." : comment.content}
            </Text>

            {isReplying ? (
              <CommentComposer
                autoFocus
                isDisabled={isBusy}
                isSubmitting={pendingAction === replyActionKey}
                label="Votre reponse"
                minHeight="108px"
                onCancel={() => setIsReplying(false)}
                onSubmit={handleReply}
                placeholder="Repondez a ce commentaire..."
                submitLabel="Publier la reponse"
              />
            ) : null}
          </Stack>
        </CardBody>
      </Card>

      {children.length > 0 ? (
        <Stack spacing={4}>
          {children.map((childNode) => (
            <CommentItem
              canModerateComments={canModerateComments}
              currentUserId={currentUserId}
              depth={depth + 1}
              isAuthenticated={isAuthenticated}
              isBusy={isBusy}
              key={childNode.comment.idComment}
              node={childNode}
              onDelete={onDelete}
              onReply={onReply}
              pendingAction={pendingAction}
              resourceOwnerId={resourceOwnerId}
            />
          ))}
        </Stack>
      ) : null}
    </Stack>
  );
}

export function CommentsSection({ idResource, resourceOwnerId }: CommentsSectionProps) {
  const { hasPermission, status, token, user } = useAuth();
  const [comments, setComments] = useState<ResourceComment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pendingAction, setPendingAction] = useState<string | null>(null);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const canModerateComments = hasPermission(PermissionNames.deleteComment);
  const isAuthLoading = status === "loading";
  const isAuthenticated = status === "authenticated";
  const isBusy = pendingAction !== null;
  const commentTree = buildCommentTree(comments);

  useEffect(() => {
    let cancelled = false;

    async function loadComments() {
      setIsLoading(true);

      try {
        const response = await commentsService.getCommentsByResource(idResource);

        if (cancelled) {
          return;
        }

        setComments(response);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setComments([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadComments();

    return () => {
      cancelled = true;
    };
  }, [idResource]);

  async function refreshComments() {
    const response = await commentsService.getCommentsByResource(idResource);
    setComments(response);
  }

  async function handleCreateComment(content: string, idParentComment?: number) {
    if (!token) {
      navigateTo("/login");
      return false;
    }

    const actionKey = idParentComment ? `reply-${idParentComment}` : "create-root";
    setPendingAction(actionKey);

    try {
      await commentsService.createComment(token, idResource, {
        content,
        idParentComment
      });
      await refreshComments();
      showFormMessage(
        setMessage,
        createSuccessMessage(idParentComment ? "Votre reponse a ete publiee." : "Votre commentaire a ete publie.")
      );
      return true;
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Publication impossible"));
      return false;
    } finally {
      setPendingAction(null);
    }
  }

  async function handleDeleteComment(comment: ResourceComment, actionKey: string) {
    if (!token) {
      navigateTo("/login");
      return;
    }

    setPendingAction(actionKey);

    try {
      if (user?.idUser === comment.idUser) {
        await commentsService.deleteComment(token, comment.idComment);
      } else {
        await commentsService.deleteCommentForModeration(token, comment.idComment);
      }

      await refreshComments();
      showFormMessage(setMessage, createSuccessMessage("Le commentaire a ete supprime."));
    } catch (deleteError) {
      showFormMessage(setMessage, createErrorMessage(deleteError, "Suppression impossible"));
    } finally {
      setPendingAction(null);
    }
  }

  return (
    <Stack spacing={5}>
      <Stack spacing={2}>
        <Heading color="ink.800" fontSize={{ base: "28px", md: "32px" }}>
          Commentaires
        </Heading>
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {comments.length > 0 ? getCommentCountLabel(comments.length) : "Aucun commentaire pour le moment."}
        </Text>
      </Stack>

      {message ? <MessageBanner message={message.message} onClose={() => showFormMessage(setMessage, null)} title={message.title} tone={message.tone} /> : null}

      {isAuthLoading ? (
        <Skeleton borderRadius="18px" height="172px" />
      ) : isAuthenticated ? (
        <CommentComposer
          isDisabled={isBusy}
          isSubmitting={pendingAction === "create-root"}
          label="Ajouter un commentaire"
          onSubmit={(content) => handleCreateComment(content)}
          placeholder="Partagez votre avis sur cette publication..."
          submitLabel="Publier le commentaire"
        />
      ) : (
        <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
          <CardBody p={{ base: 4, md: 5 }}>
            <HStack align="center" justify="space-between" spacing={4} wrap="wrap">
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                Connectez-vous pour commenter et repondre aux discussions.
              </Text>

              <Button onClick={() => navigateTo("/login")} variant="outline">
                Se connecter
              </Button>
            </HStack>
          </CardBody>
        </Card>
      )}

      {isLoading ? (
        <Stack spacing={4}>
          <Skeleton borderRadius="16px" height="180px" />
          <Skeleton borderRadius="16px" height="180px" />
        </Stack>
      ) : null}

      {!isLoading && commentTree.length > 0 ? (
        <Stack spacing={5}>
          {commentTree.map((node) => (
            <CommentItem
              canModerateComments={canModerateComments}
              currentUserId={user?.idUser ?? null}
              depth={0}
              isAuthenticated={isAuthenticated}
              isBusy={isBusy}
              key={node.comment.idComment}
              node={node}
              onDelete={handleDeleteComment}
              onReply={(parentCommentId, content) => handleCreateComment(content, parentCommentId)}
              pendingAction={pendingAction}
              resourceOwnerId={resourceOwnerId}
            />
          ))}
        </Stack>
      ) : null}
    </Stack>
  );
}
