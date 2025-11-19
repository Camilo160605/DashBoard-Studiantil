import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, AuthResponse, AuthState } from '../models/auth.model';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(TokenStorageService);

  private readonly _state = signal<AuthState>(this.restoreState());

  readonly isAuthenticated = computed(() => {
    const { token, expiresAt } = this._state();
    if (!token) {
      return false;
    }

    return !expiresAt || expiresAt > Date.now();
  });

  readonly token = computed(() => (this.isAuthenticated() ? this._state().token : null));

  login(payload: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, payload)
      .pipe(
        tap((response) => {
          const expiresAt = Date.now() + response.expiresIn * 1000;
          this.storage.persist(response.accessToken, expiresAt);
          this._state.set({ token: response.accessToken, expiresAt });
        })
      );
  }

  logout(): void {
    this.storage.clear();
    this._state.set({ token: null, expiresAt: null });
    void this.router.navigate(['/login']);
  }

  private restoreState(): AuthState {
    const stored = this.storage.get();
    if (!stored) {
      return { token: null, expiresAt: null };
    }

    if (stored.expiresAt <= Date.now()) {
      this.storage.clear();
      return { token: null, expiresAt: null };
    }

    return { token: stored.token, expiresAt: stored.expiresAt };
  }
}
