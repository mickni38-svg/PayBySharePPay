import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DirectoryEntry } from '../models/directory.model';

@Injectable({
  providedIn: 'root'
})
export class DirectoryService {
  private apiUrl = `${environment.apiUrl}/api/directory`;

  constructor(private http: HttpClient) {}

  // GET /api/directory/search?query=...
  search(query: string, excludeFriendsOf?: number): Observable<DirectoryEntry[]> {
    let params = new HttpParams().set('query', query);
    if (excludeFriendsOf != null) {
      params = params.set('excludeFriendsOf', excludeFriendsOf);
    }
    return this.http.get<DirectoryEntry[]>(`${this.apiUrl}/search`, { params });
  }

  // GET /api/directory/{participantId}/friends
  getFriends(participantId: number): Observable<DirectoryEntry[]> {
    return this.http.get<DirectoryEntry[]>(`${this.apiUrl}/${participantId}/friends`);
  }
}
