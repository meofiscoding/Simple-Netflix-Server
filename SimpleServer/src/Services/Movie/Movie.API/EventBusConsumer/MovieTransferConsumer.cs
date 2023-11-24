using AutoMapper;
using EventBus.Message.Events;
using MassTransit;
using Movie.API.Models;
using Movie.API.Repository;
using SimpleServer.src.Movie;

namespace Movie.API.EventBusConsumer
{
    public class MovieTransferConsumer : IConsumer<TransferMovieListEvent>
    {
        private readonly IMapper _mapper;
        private readonly IMovieRepository _movieService;
        private readonly ILogger<MovieTransferConsumer> _logger;

        public MovieTransferConsumer(IMapper mapper, IMovieRepository movieService, ILogger<MovieTransferConsumer> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<TransferMovieListEvent> context)
        {
            // Add context.Message to database
            var movie = _mapper.Map<MovieInformation>(context.Message);
            // Upsert movie to database
            await _movieService.UpsertMovieAsync(movie);
            _logger.LogInformation($"Movie consume from MQ is added to database: {movie.MovieName}");
        }
    }
}
