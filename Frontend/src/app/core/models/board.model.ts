export interface BoardSummary {
  id: string;
  name: string;
}

export interface ColumnDto {
  id: string;
  boardId: string;
  name: string;
  position: number;
}

export interface CardDto {
  id: string;
  boardId: string;
  columnId: string;
  title: string;
  description?: string | null;
  position: number;
  assigneeId?: string | null;
  assigneeEmail?: string | null;
  dueDate?: string | null;
}

export interface BoardDetail {
  id: string;
  name: string;
  columns: ColumnDto[];
  cards: CardDto[];
}

export interface CreateBoardRequest {
  name: string;
}

export interface CreateColumnRequest {
  name: string;
}

export interface CreateCardRequest {
  columnId: string;
  title: string;
  description?: string | null;
}

export interface MoveCardRequest {
  cardId: string;
  toColumnId: string;
  prevPos: number | null;
  nextPos: number | null;
}
