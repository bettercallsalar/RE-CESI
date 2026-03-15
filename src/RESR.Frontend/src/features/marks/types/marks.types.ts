export interface Mark {
  idMark: number;
  isFavorite: boolean;
  isReadLater: boolean;
  idRessource: number;
  idUser: number;
}

export interface PaginatedMarksResponse {
  items: Mark[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
