using System;
using CrawlData;
using CrawlData.Helper;
using CrawlData.Model;

namespace Crawler.UnitTest
{
    [TestFixture]
    public class TestFindCombination
    {
        // Basic Test Case - Multiple TV Shows with Exact Unavailable Episodes
        [Test]
        public void FindCombinationOfTvShowToPushToGCS_MultipleTVShowsWithExacNumberOfUnAvailableEpisode_ReturnListContainingThatTvShow()
        {
            // Arrange
            Random rand = new();
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = Enumerable
                .Range(1, rand.Next(1, 10))
                .ToDictionary(_ => new MovieItem()
                {
                    AvailableEpisode = 0,
                    StreamingUrls = Enumerable.Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).ToDictionary(x => x.ToString(), x => x.ToString())
                }, _ => Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY);

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return a list containing TvShow that has exactly Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY unavailable episodes.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.StreamingUrls.Count - x.AvailableEpisode == Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY));
        }

        //Basic Test Case - Multiple TV shows with one having Exact Unavailable Episodes
        [Test]
        public void FindCombinationOfTvShowToPushToGCS_MultipleTVShowsWithOneHaveExacNumberOfUnAvailableEpisode_ReturnListContainingThatTvShow()
        {
            // Arrange
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = Enumerable
                .Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY)
                .ToDictionary(_ => new MovieItem()
                {
                    AvailableEpisode = 0,
                    StreamingUrls = Enumerable.Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).ToDictionary(x => x.ToString(), x => x.ToString())
                }, index => index);

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return a list containing TvShow that has exactly Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY unavailable episodes.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.StreamingUrls.Count - x.AvailableEpisode == Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY));
        }


        // Basic Test Case - Single TV Show with Exact Unavailable Episodes: return a list containing the single TV show.
        [Test]
        public void TestFindCombination_SingleTVShowWithExacNumberOfUnAvailableEpisode_ReturnListContainingThatTvShow()
        {
            // Arrange
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = new()
            {
                {
                    new MovieItem(){
                        AvailableEpisode = 0,
                        StreamingUrls = Enumerable.Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).ToDictionary(x => x.ToString(), x => x.ToString())
                    }, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                }
            };

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return a list containing the single TV show.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.StreamingUrls.Count - x.AvailableEpisode == Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY));
        }

        //Basic Test Case - Single TV Show Available with More Unavailable Episodes than Needed: Return a list containing that TV Show
        [Test]
        public void TestFindCombination_SingleTvShowAvailableWithMoreUnavailableEpisodesThanNeeded_ReturnListContainingThatTvShow()
        {
            // Arrange
            // Input: unavailableEpisodeOfTvShow contains a single TV show with more unavailable episodes than needed (Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = new(){
                {
                    new MovieItem(){
                        Id = "1",
                        AvailableEpisode = 0,
                        StreamingUrls = Enumerable.Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY + 2).ToDictionary(x => x.ToString(), x => x.ToString())
                    },Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY + 2
                },
            };

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return a list containing that TV show.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.Id == "1"));
        }

        // Edge Case - Single TV Show Available with Fewer Unavailable Episodes than Needed: Return a list containing that TV show
        [Test]
        public void TestFindCombination_SingleTvShowAvailableWithFewerUnavailableEpisodesThanNeeded_ReturnListContainingThatTvShow()
        {
            // Arrange
            Dictionary<MovieItem, int> availableEpisodes = new(){
                {
                    new MovieItem(){
                        Id = "1",
                        AvailableEpisode = 0,
                        StreamingUrls = Enumerable.Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY - 2).ToDictionary(x => x.ToString(), x => x.ToString())
                    },Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY - 2
                },
            };

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(availableEpisodes);

            // Assert
            // Expected Output: The method should return a list containing that TV show.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.Id == "1"));
        }

        // Basic Test Case - No TV Show with Exact Number of Unavailable Episodes
        [Test]
        public void TestFindCombination_CombinationofTVshowsMeetTheCriteria_ReturnCombinationOfTvShowWhoseSumOfUnAvailableEpisodeIsInAcceptableErrorRange()
        {
            // Arrange
            // Input: unavailableEpisodeOfTvShow contains TV shows, but none of them has exactly Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY unavailable episodes.
            // init Dictionary<MovieItem, int> with 4 element, each have unique value from 1 to 4
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = Enumerable
                .Range(1, 4)
                .ToDictionary(x => new MovieItem()
                {
                    AvailableEpisode = 0,
                    StreamingUrls = Enumerable.Range(1, x).ToDictionary(x => x.ToString(), x => x.ToString())
                }, x => x);

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            //Expected Output: The method should return a combination of TV shows whose sum of unavailable episodes = 6
            Assert.That(result, Has.Count.EqualTo(3));
            // sum of unavailable episode of all element in result is equal to 6
            Assert.That(result!.Sum(x => x.StreamingUrls.Count - x.AvailableEpisode), Is.InRange(Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY - Consts.ACCEPTABLE_ERROR, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY + Consts.ACCEPTABLE_ERROR));
        }

        // Test Case: Combination of TV shows is larger than the criteria.
        [Test]
        public void TestFindCombination_CombinationOfYvShowsIsLargerThanTheCriteria_ReturnAListOfTvShowThatSumUpInRange()
        {
            // Arrange
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = Enumerable
                .Range(8, 12)
                .ToDictionary(x => new MovieItem()
                {
                    AvailableEpisode = 0,
                    StreamingUrls = Enumerable.Range(1, x).ToDictionary(x => x.ToString(), x => x.ToString())
                }, x => x);

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return a list of TV shows whose sum of unavailable episodes is in the acceptable error range.
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result!.Sum(x => x.StreamingUrls.Count - x.AvailableEpisode), Is.EqualTo(8));
        }

        // Basic Test Case - No TV Show Available: Return Null
        [Test]
        public void TestFindCombination_NoTvShowAvailable_ReturnNull()
        {
            // Arrange
            // Input: unavailableEpisodeOfTvShow contains no TV show.
            Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = new();

            // Act
            var result = MovieHelper.FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);

            // Assert
            // Expected Output: The method should return empty list with no element
            Assert.That(result, Is.Null);
        }
    }
}
