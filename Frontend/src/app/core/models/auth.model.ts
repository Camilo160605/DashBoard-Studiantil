export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
}

export interface AuthState {
  token: string | null;
  expiresAt: number | null;
}
