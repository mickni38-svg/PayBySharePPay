import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Participant,
  ParticipantApiDto,
  mapParticipant,
  CreatePersonRequest,
  CreateMerchantRequest
} from '../models/participant.model';

@Injectable({
  providedIn: 'root'
})
export class ParticipantService {
  private apiUrl = `${environment.apiUrl}/api/participants`;

  constructor(private http: HttpClient) {}

  // GET /api/participants/search?query=...&initiatorId=...
  searchParticipants(query: string, initiatorId?: number): Observable<Participant[]> {
    let params = new HttpParams().set('query', query);
    if (initiatorId !== undefined) {
      params = params.set('initiatorId', initiatorId);
    }
    return this.http
      .get<ParticipantApiDto[]>(`${this.apiUrl}/search`, { params })
      .pipe(map(dtos => dtos.map(mapParticipant)));
  }

  // POST /api/participants/person
  createPerson(request: CreatePersonRequest): Observable<Participant> {
    return this.http
      .post<ParticipantApiDto>(`${this.apiUrl}/person`, request)
      .pipe(map(mapParticipant));
  }

  // POST /api/participants/merchant
  createMerchant(request: CreateMerchantRequest): Observable<Participant> {
    return this.http
      .post<ParticipantApiDto>(`${this.apiUrl}/merchant`, request)
      .pipe(map(mapParticipant));
  }
}

