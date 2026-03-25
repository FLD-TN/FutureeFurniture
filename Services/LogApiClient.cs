using System.Net.Http;
using System.Net.Http.Json; 
using System.Threading.Tasks;

public class LogApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5226/api/SystemLog";

    public LogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LogUserLoginAsync(string userId, string action, string details)
    {
        var payload = new
        {
            UserId = userId,
            Action = action,
            Details = details
        };

        var url = $"{_apiBaseUrl}/User";

        await _httpClient.PostAsJsonAsync(url, payload);
    }

    public async Task LogAdminLoginAsync(string adminId, string action, string details)
    {
        var payload = new
        {
            AdminId = adminId,
            Action = action,
            Details = details
        };

        var url = $"{_apiBaseUrl}/Admin";

        await _httpClient.PostAsJsonAsync(url, payload);
    }
}