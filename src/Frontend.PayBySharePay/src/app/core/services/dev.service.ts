import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DevService {
  private apiUrl = `${environment.apiUrl}/api/dev`;

  constructor(private http: HttpClient) {}

  resetData(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reset`);
  }
}
