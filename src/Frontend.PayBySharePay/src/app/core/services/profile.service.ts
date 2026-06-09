import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ParticipantApiDto } from '../models/participant.model';

export interface UpdateProfileRequest {
  name: string;
  email?: string;
  phone?: string;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly apiUrl = `${environment.apiUrl}/api/participants`;

  constructor(private readonly http: HttpClient) {}

  getProfile(id: number): Observable<ParticipantApiDto> {
    return this.http.get<ParticipantApiDto>(`${this.apiUrl}/${id}`);
  }

  updateProfile(id: number, request: UpdateProfileRequest): Observable<ParticipantApiDto> {
    return this.http.put<ParticipantApiDto>(`${this.apiUrl}/${id}/profile`, request);
  }
}
