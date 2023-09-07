import { Component } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Movie } from './models/movie';
import { MovieService } from './services/movie.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(private modalService: NgbModal, private movieService: MovieService){
  }
  // declare array of movies array
  movies: [] = [];
  
  public open(modal: any): void {
    this.modalService.open(modal);
  }

  ngOnInit(): void {
    this.movieService.getMovies().subscribe(
      (response: any) => {
        this.movies = response;
      },
    );
  }

  title = 'NetflixApp';
}
