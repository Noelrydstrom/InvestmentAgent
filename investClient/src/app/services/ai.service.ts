import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AiResponse {
  reply: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {

  private http = inject(HttpClient);

  private readonly apiUrl = 'http://localhost:5206/api/ai';

  sendMessage(message: string): Observable<AiResponse> {
    return this.http.post<AiResponse>(
      `${this.apiUrl}/chat`,
      { message }
    );
  }
}