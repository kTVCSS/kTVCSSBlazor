using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using RadzenBlazorDemos.Models.GitHub;

namespace RadzenBlazorDemos.Services
{
    class Link
    {
        public string Next { get; set; }
        public string Last { get; set; }

        public int NextPage { get; set; }
        public int LastPage { get; set; }

        private static int ExtractPage(string value)
        {
            var match = Regex.Match(value, "&page=(?<page>\\d+)");

            if (match != null)
            {
                return Convert.ToInt32(match.Groups["page"].Value);
            }

            return 0;
        }

        public static Link FromHeader(IEnumerable<string> header)
        {
            var result = new Link();

            var links = String.Join("", header).Split(',');

            foreach (var link in links)
            {
                var rel = Regex.Match(link, "(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase);
                var value = Regex.Match(link, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

                if (rel.Success && value.Success)
                {
                    if (rel.Value == "next")
                    {
                        result.Next = value.Value;
                        result.NextPage = ExtractPage(result.Next);
                    }
                    if (rel.Value == "last")
                    {
                        result.Last = value.Value;
                        result.LastPage = ExtractPage(result.Last);
                    }
                }
            }

            return result;
        }
    }

    class IssueCache
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("issues")]
        public IEnumerable<Issue> Issues { get; set; }
    }

    public class FetchProgressEventArgs
    {
        public int Total { get; set; }
        public int Current { get; set; }
    }

    public class GitHubService
    {
        private readonly JsonSerializerOptions options;

        public Action<FetchProgressEventArgs> OnProgress;
        private IEnumerable<Issue> issues;

        public GitHubService()
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public async Task<IEnumerable<Issue>> GetIssues(DateTime date)
        {
            var target = date.AddMonths(-1);

            if (issues == null)
            {
                issues = await FetchIssues(target);
                issues = issues.Where(issue => issue.CreatedAt >= target).OrderBy(issue => issue.CreatedAt);
            }

            return issues;
        }

        private async Task<IEnumerable<Issue>> FetchIssues(DateTime since)
        {
            var issues = new List<Issue>();
            const string repoOwner = "kTVCSS";
            const string repoName = "kTVCSSBlazor";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "kTVCSSBlazor");
            http.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

            var apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/issues?state=all&per_page=100&since={since:yyyy-MM-ddTHH:mm:ssZ}";
            var currentPageUrl = apiUrl;
            int currentPage = 1;
            int totalPages = 1;

            try
            {
                while (!string.IsNullOrEmpty(currentPageUrl))
                {
                    var response = await http.GetAsync(currentPageUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ApplicationException($"GitHub API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }

                    // Получаем информацию о пагинации из заголовков
                    var headers = response.Headers;
                    if (headers.Contains("Link"))
                    {
                        var links = ParseLinkHeader(headers.GetValues("Link").FirstOrDefault());
                        currentPageUrl = links.Next;
                        totalPages = links.LastPage;
                    }
                    else
                    {
                        currentPageUrl = null;
                    }

                    // Десериализация ответа
                    var content = await response.Content.ReadAsStreamAsync();
                    var pageIssues = await JsonSerializer.DeserializeAsync<List<Issue>>(content, options);
                    issues.AddRange(pageIssues);

                    // Обновление прогресса
                    OnProgress?.Invoke(new FetchProgressEventArgs
                    {
                        Current = currentPage++,
                        Total = totalPages
                    });
                }

                return issues;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to fetch issues from GitHub API", ex);
            }
        }

        // Вспомогательный метод для парсинга заголовка Link
        private static (string Next, int LastPage) ParseLinkHeader(string linkHeader)
        {
            string next = null;
            int lastPage = 1;

            if (!string.IsNullOrEmpty(linkHeader))
            {
                var links = linkHeader.Split(',');
                foreach (var link in links)
                {
                    var parts = link.Split(';');
                    if (parts.Length < 2) continue;

                    var url = parts[0].Trim().TrimStart('<').TrimEnd('>');
                    var rel = parts[1].Trim();

                    if (rel.Contains("next"))
                    {
                        next = url;
                    }
                    else if (rel.Contains("last"))
                    {
                        var match = Regex.Match(url, @"&page=(\d+)");
                        if (match.Success)
                        {
                            lastPage = int.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }

            return (next, lastPage);
        }
    }
}