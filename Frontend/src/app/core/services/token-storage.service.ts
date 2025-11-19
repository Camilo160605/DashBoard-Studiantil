import { Injectable } from '@angular/core';

interface StoredAuth {
  token: string;
  expiresAt: number;
}

const TOKEN_KEY = 'kanban-token';
const EXP_KEY = 'kanban-token-exp';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  persist(token: string, expiresAt: number): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(EXP_KEY, expiresAt.toString());
  }

  get(): StoredAuth | null {
    const token = localStorage.getItem(TOKEN_KEY);
    const expiresRaw = localStorage.getItem(EXP_KEY);

    if (!token || !expiresRaw) {
      return null;
    }

    const expiresAt = Number(expiresRaw);
    if (Number.isNaN(expiresAt)) {
      this.clear();
      return null;
    }

    return { token, expiresAt };
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXP_KEY);
  }
}
