import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { env } from '../../../core/config/env';

export interface AuthResponse {
  token: string;
  tenantId: string;
  userId: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${env.apiBaseUrl}/api/auth/login`, { email, password }).pipe(
      tap((res) => this.storeSession(res))
    );
  }

  register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${env.apiBaseUrl}/api/auth/register`, { email, password }).pipe(
      tap((res) => this.storeSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem('apiforms_jwt');
    localStorage.removeItem('apiforms_tenant');
    localStorage.removeItem('apiforms_user');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('apiforms_jwt');
  }

  getCurrentUserEmail(): string {
    return localStorage.getItem('apiforms_user') ?? '';
  }

  private storeSession(res: AuthResponse): void {
    localStorage.setItem('apiforms_jwt', res.token);
    localStorage.setItem('apiforms_tenant', res.tenantId);
    localStorage.setItem('apiforms_user', res.email);
  }
}
