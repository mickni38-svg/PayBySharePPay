import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddFriendRequest } from '../models/friend.model';
import { DirectoryEntry } from '../models/directory.model';
import { DirectoryService } from './directory.service';

@Injectable({
  providedIn: 'root'
})
export class FriendService {
  private apiUrl = `${environment.apiUrl}/api/friends`;

  constructor(private http: HttpClient, private directoryService: DirectoryService) {}

  getFriends(participantId: number): Observable<DirectoryEntry[]> {
    return this.directoryService.getFriends(participantId);
  }

  // POST /api/friends  { initiatorId, receiverId }
  addFriend(request: AddFriendRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }
}

