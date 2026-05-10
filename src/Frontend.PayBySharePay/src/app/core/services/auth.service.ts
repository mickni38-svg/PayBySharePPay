import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
}

export interface LoginResponse {
  token: string;
  participantId: number;
  name: string;
  expiresAt: string;
}

export interface RegisterPersonRequest {
  name: string;
  email: string;
  phone?: string;
}

export interface RegisterMerchantRequest {
  name: string;
  companyName: string;
  cvrNumber?: string;
  contactPerson?: string;
  contactEmail?: string;
  contactPhone?: string;
  companyAddress?: string;
}

const TOKEN_KEY = 'sbys_token';
const USER_KEY = 'sbys_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _user = signal<{ participantId: number; name: string } | null>(
    this._parseStoredUser()
  );

  readonly isLoggedIn = computed(() => this._token() !== null);
  readonly currentUserId = computed(() => this._user()?.participantId ?? null);
  readonly currentUserName = computed(() => this._user()?.name ?? null);

  constructor(private readonly http: HttpClient) {}

  login(email: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, { email })
      .pipe(tap(res => this._storeSession(res)));
  }

  register(req: RegisterPersonRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/register`, req)
      .pipe(tap(res => this._storeSession(res)));
  }

  registerMerchant(req: RegisterMerchantRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/register-merchant`, req)
      .pipe(tap(res => this._storeSession(res)));
  }

  private _storeSession(res: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, JSON.stringify({ participantId: res.participantId, name: res.name }));
    this._token.set(res.token);
    this._user.set({ participantId: res.participantId, name: res.name });
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._token.set(null);
    this._user.set(null);
  }

  getToken(): string | null {
    return this._token();
  }

  private _parseStoredUser(): { participantId: number; name: string } | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
