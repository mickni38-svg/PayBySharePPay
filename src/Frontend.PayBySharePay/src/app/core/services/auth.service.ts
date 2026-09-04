import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type ParticipantType = 'Person' | 'Merchant';

export interface LoginRequest {
  email: string;
  password?: string;
}

export interface LoginResponse {
  token: string;
  participantId: number;
  name: string;
  participantType: ParticipantType;
  expiresAt: string;
}

export interface RegisterPersonRequest {
  name: string;
  email: string;
  phone?: string;
  password: string;
}

export interface RegisterMerchantRequest {
  name: string;
  companyName: string;
  email: string;
  password: string;
  vippsMerchantSerialNumber?: string;
  cvrNumber?: string;
  contactPerson?: string;
  contactEmail?: string;
  contactPhone?: string;
  companyAddress?: string;
}

export interface RegistrationPhoneOptions {
  enabled: boolean;
  phoneNumbers: string[];
}

export interface ExternalLoginRequest {
  idToken: string;
}

const TOKEN_KEY = 'sbys_token';
const USER_KEY = 'sbys_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _user = signal<{ participantId: number; name: string; participantType?: ParticipantType } | null>(
    this._parseStoredUser()
  );

  readonly isLoggedIn = computed(() => this._token() !== null);
  readonly currentUserId = computed(() => this._user()?.participantId ?? null);
  readonly currentUserName = computed(() => this._user()?.name ?? null);
  readonly currentUserType = computed(() => this._user()?.participantType ?? null);

  constructor(private readonly http: HttpClient) {}

  login(email: string, password?: string): Observable<LoginResponse> {
    const body: LoginRequest = password ? { email, password } : { email };
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, body)
      .pipe(tap(res => this._storeSession(res)));
  }

  getRegistrationPhoneOptions(): Observable<RegistrationPhoneOptions> {
    return this.http.get<RegistrationPhoneOptions>(`${environment.apiUrl}/api/auth/available-test-phone-numbers`);
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

  googleLogin(idToken: string): Observable<LoginResponse> {
    const body: ExternalLoginRequest = { idToken };
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/api/auth/google-login`, body)
      .pipe(tap(res => this._storeSession(res)));
  }

  private _storeSession(res: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    const user = {
      participantId: res.participantId,
      name: res.name,
      participantType: res.participantType
    };
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._token.set(res.token);
    this._user.set(user);
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

  updateStoredName(name: string): void {
    const user = this._user();
    if (!user) return;
    const updated = { ...user, name };
    localStorage.setItem(USER_KEY, JSON.stringify(updated));
    this._user.set(updated);
  }

  private _parseStoredUser(): { participantId: number; name: string; participantType?: ParticipantType } | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
