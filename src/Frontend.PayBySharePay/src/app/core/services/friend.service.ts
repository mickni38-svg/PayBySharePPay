import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddFriendRequest } from '../models/friend.model';

@Injectable({
  providedIn: 'root'
})
export class FriendService {
  private apiUrl = `${environment.apiUrl}/api/friends`;

  constructor(private http: HttpClient) {}

  // POST /api/friends  { initiatorId, receiverId }
  addFriend(request: AddFriendRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }
}

