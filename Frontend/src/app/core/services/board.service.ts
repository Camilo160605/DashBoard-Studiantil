import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BoardDetail,
  BoardSummary,
  CardDto,
  ColumnDto,
  CreateBoardRequest,
  CreateCardRequest,
  CreateColumnRequest,
  MoveCardRequest
} from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/boards`;

  getBoards(): Observable<BoardSummary[]> {
    return this.http.get<BoardSummary[]>(this.baseUrl);
  }

  getBoard(boardId: string): Observable<BoardDetail> {
    return this.http.get<BoardDetail>(`${this.baseUrl}/${boardId}`);
  }

  createBoard(payload: CreateBoardRequest): Observable<BoardSummary> {
    return this.http.post<BoardSummary>(this.baseUrl, payload);
  }

  createColumn(boardId: string, payload: CreateColumnRequest): Observable<ColumnDto> {
    return this.http.post<ColumnDto>(`${this.baseUrl}/${boardId}/columns`, payload);
  }

  createCard(boardId: string, payload: CreateCardRequest): Observable<CardDto> {
    return this.http.post<CardDto>(`${this.baseUrl}/${boardId}/cards`, payload);
  }

  moveCard(boardId: string, payload: MoveCardRequest): Observable<CardDto> {
    return this.http.patch<CardDto>(`${this.baseUrl}/${boardId}/cards/move`, payload);
  }

  exportBoard(boardId: string, format: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${boardId}/export/${format}`, {
      responseType: 'blob'
    });
  }
}
