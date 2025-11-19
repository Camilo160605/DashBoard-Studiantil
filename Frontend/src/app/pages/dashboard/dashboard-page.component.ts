import { CommonModule } from '@angular/common';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BoardService } from '../../core/services/board.service';
import { BoardDetail, BoardSummary, CardDto, ColumnDto } from '../../core/models/board.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DragDropModule,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly boardService = inject(BoardService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly boards = signal<BoardSummary[]>([]);
  readonly currentBoard = signal<BoardDetail | null>(null);
  readonly columns = signal<ColumnViewModel[]>([]);
  readonly isLoadingBoards = signal(false);
  readonly isLoadingBoard = signal(false);
  readonly message = signal<string | null>(null);
  readonly selectedBoardId = signal<string | null>(null);

  readonly dropListIds = computed(() => this.columns().map((c) => `column-${c.id}`));

  readonly createBoardForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]]
  });

  readonly createColumnForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]]
  });

  private readonly cardForms = new Map<string, ReturnType<typeof this.buildCardForm>>();

  constructor() {
    this.loadBoards();
  }

  logout(): void {
    this.authService.logout();
  }

  loadBoards(): void {
    this.isLoadingBoards.set(true);
    this.boardService
      .getBoards()
      .pipe(
        finalize(() => this.isLoadingBoards.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (boards) => {
          this.boards.set(boards);
          const currentId = this.selectedBoardId();
          if (!currentId && boards.length) {
            this.selectBoard(boards[0].id);
          } else if (currentId && boards.some((b) => b.id === currentId)) {
            this.selectBoard(currentId);
          }
        },
        error: () => this.message.set('No se pudieron cargar los tableros. Verifica el backend.')
      });
  }

  selectBoard(boardId: string): void {
    this.selectedBoardId.set(boardId);
    this.isLoadingBoard.set(true);
    this.boardService
      .getBoard(boardId)
      .pipe(
        finalize(() => this.isLoadingBoard.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (board) => {
          this.currentBoard.set(board);
          this.columns.set(this.mapColumns(board));
          this.message.set(board.cards.length ? null : 'Aún no hay tarjetas en este tablero.');
        },
        error: () => this.message.set('No se pudo obtener el tablero seleccionado.')
      });
  }

  createBoard(): void {
    if (this.createBoardForm.invalid) {
      this.createBoardForm.markAllAsTouched();
      return;
    }

    const payload = this.createBoardForm.getRawValue();
    this.boardService
      .createBoard(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (board) => {
          this.boards.update((value) => [...value, board]);
          this.createBoardForm.reset();
          this.selectBoard(board.id);
          this.message.set('Tablero creado correctamente.');
        },
        error: () => this.message.set('No se pudo crear el tablero.')
      });
  }

  createColumn(): void {
    if (this.createColumnForm.invalid || !this.selectedBoardId()) {
      this.createColumnForm.markAllAsTouched();
      return;
    }

    const payload = this.createColumnForm.getRawValue();
    const boardId = this.selectedBoardId()!;
    this.boardService
      .createColumn(boardId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (column) => {
          this.columns.update((cols) =>
            [...cols, { ...column, cards: [] }].sort(sortByPosition)
          );
          this.createColumnForm.reset();
          this.message.set('Columna agregada.');
        },
        error: () => this.message.set('No se pudo crear la columna.')
      });
  }

  createCard(column: ColumnViewModel): void {
    const form = this.cardForm(column.id);
    if (form.invalid || !this.selectedBoardId()) {
      form.markAllAsTouched();
      return;
    }

    const boardId = this.selectedBoardId()!;
    const payload = {
      columnId: column.id,
      title: form.controls.title.value,
      description: form.controls.description.value ?? undefined
    };

    this.boardService
      .createCard(boardId, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (card) => {
          form.reset();
          this.columns.update((cols) =>
            cols.map((col) =>
              col.id === column.id
                ? { ...col, cards: [...col.cards, card].sort(sortByPosition) }
                : col
            )
          );
          this.message.set('Tarjeta creada.');
        },
        error: () => this.message.set('No se pudo crear la tarjeta.')
      });
  }

  download(format: 'pdf' | 'excel'): void {
    const boardId = this.selectedBoardId();
    if (!boardId) {
      return;
    }

    this.boardService
      .exportBoard(boardId, format)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const extension = format === 'pdf' ? 'pdf' : 'xlsx';
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = `board-${boardId}.${extension}`;
          anchor.click();
          URL.revokeObjectURL(url);
        },
        error: () => this.message.set('No se pudo exportar el tablero.')
      });
  }

  dropCard(event: CdkDragDrop<CardDto[]>, target: ColumnViewModel): void {
    const boardId = this.selectedBoardId();
    if (!boardId) {
      return;
    }

    if (event.previousContainer === event.container) {
      moveItemInArray(target.cards, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    const targetCards = event.container.data;
    const movedCard = targetCards[event.currentIndex];
    movedCard.columnId = target.id;
    const prevPos = event.currentIndex > 0 ? targetCards[event.currentIndex - 1].position : null;
    const nextPos =
      event.currentIndex < targetCards.length - 1 ? targetCards[event.currentIndex + 1].position : null;

    this.boardService
      .moveCard(boardId, {
        cardId: movedCard.id,
        toColumnId: target.id,
        prevPos,
        nextPos
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (card) => {
          movedCard.position = card.position;
          movedCard.columnId = card.columnId;
          this.message.set('Tarjeta actualizada.');
        },
        error: () => this.selectBoard(boardId)
      });
  }

  cardForm(columnId: string) {
    if (!this.cardForms.has(columnId)) {
      this.cardForms.set(columnId, this.buildCardForm());
    }

    return this.cardForms.get(columnId)!;
  }

  trackColumn(_: number, column: ColumnViewModel): string {
    return column.id;
  }

  trackCard(_: number, card: CardDto): string {
    return card.id;
  }

  private mapColumns(board: BoardDetail): ColumnViewModel[] {
    const grouped = board.cards.reduce<Record<string, CardDto[]>>((acc, card) => {
      acc[card.columnId] = acc[card.columnId] ?? [];
      acc[card.columnId].push(card);
      return acc;
    }, {});

    return [...board.columns]
      .sort(sortByPosition)
      .map((column) => ({
        ...column,
        cards: (grouped[column.id] ?? []).sort(sortByPosition)
      }));
  }

  private buildCardForm() {
    return this.fb.nonNullable.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['']
    });
  }
}

type ColumnViewModel = ColumnDto & { cards: CardDto[] };

function sortByPosition<T extends { position: number }>(a: T, b: T) {
  return a.position - b.position;
}
