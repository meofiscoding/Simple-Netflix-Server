using System.Net;

namespace Movie.FunctionalTests;

public class MovieScenratios : MovieScenarioBase
{
    [Test]
    public async Task Get_get_all_movies_and_response_ok_status_code()
    {
        using var server = CreateServer();
        var response = await server.CreateClient()
            .GetAsync(Get.Movies());

        response.EnsureSuccessStatusCode();
    }
}
