import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movie } from '../models/movie';
import { environment } from 'src/environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  url = "Movie"
  constructor(private http:HttpClient) { }

  public getMovies() : Observable<Movie[]> {
    return this.http.get<Movie[]>(`${environment.apiUrl}/${this.url}`);
  }
}
