using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace wporgfkreview.WordpressOrg;

public class Plugin
{
    public string Url { get; set; }
    public List<UserReview> Reviews { get; set; } = new();
}

public class UserReview
{
    public string Url { get; set; }
    public DateTime ReviewCreated { get; set; }
    public string AuthorUrl { get; set; }
    public DateTime AuthorCreated { get; set; }
    public int ReviewsWritten { get; set; }
}

public class Scrape
{
    public static void ParseAuthors(List<Plugin> plugins)
    {
        var authors = plugins.SelectMany(x => x.Reviews.Select(y => y.AuthorUrl)).Distinct();
        foreach (var author in authors)
        {
            Console.WriteLine($"Parsing author: {author}");
            var html = new HtmlWeb().Load(author);
            // get p class bbp-user-member-since
            var memberSince = html.DocumentNode.SelectSingleNode(
                "//p[@class='bbp-user-member-since']"
            );

            if (memberSince == null)
            {
                Console.WriteLine($"Failed to find member since for {author}");
                continue;
            }
            // extract reviews written from <p class="bbp-user-review-count">Reviews Written: 1</p>
            var reviewsWritten = html.DocumentNode.SelectSingleNode(
                "//p[@class='bbp-user-review-count']"
            );
            if (reviewsWritten != null)
            {
                var written = reviewsWritten.InnerText.Replace("Reviews Written:", string.Empty);
                if (int.TryParse(written, out int count))
                {
                    Console.WriteLine($"Reviews written: {count}");
                    // update reviews written
                    foreach (var plugin in plugins)
                    {
                        foreach (var review in plugin.Reviews)
                        {
                            if (review.AuthorUrl == author)
                            {
                                review.ReviewsWritten = count;
                            }
                        }
                    }
                }
            }
            string cleanedDate = Regex
                .Replace(
                    memberSince.InnerText,
                    @"Member Since:\s*|(?<=\d)(th|nd|st|rd)",
                    string.Empty
                )
                .Trim();
            // Try parsing the cleaned date
            if (
                DateTime.TryParseExact(
                    cleanedDate,
                    "MMMM d, yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime memberSinceDate
                )
            )
            {
                Console.WriteLine($"Parsed date: {memberSinceDate:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine($"Failed to parse date: {cleanedDate}");
                continue;
            }

            // update author created date
            foreach (var plugin in plugins)
            {
                foreach (var review in plugin.Reviews)
                {
                    if (review.AuthorUrl == author)
                    {
                        review.AuthorCreated = memberSinceDate;
                    }
                }
            }
        }
    }

    public static List<Plugin> Parse(string url)
    {
        if (!url.EndsWith("/"))
        {
            url += "/";
        }
        var uri = new Uri(url);

        var pluginsUri = new Uri(uri, "#content-plugins");
        var plugins = new List<Plugin>();
        var html = new HtmlWeb().Load(pluginsUri);

        // parse all plugins
        var divNode = html.DocumentNode.SelectNodes("//div[@class='plugin-info-container']/h3/a");

        if (divNode.Count == 0)
        {
            return plugins;
        }

        // get all plugins urls
        var pluginUrls = divNode
            .Select(node => "https:" + node.GetAttributeValue("href", string.Empty))
            .ToArray();

        // get all reviews
        foreach (var pluginUrl in pluginUrls)
        {
            Console.WriteLine($"Parsing plugin: {pluginUrl}");

            // get plugin slug
            var match = Regex.Match(pluginUrl, "(?<=/plugins/)[^/]+");
            if (!match.Success)
            {
                Console.WriteLine($"Invalid URL: {pluginUrl}");
                continue;
            }

            // get reviews page count
            var pageNumber = 1;
            html = new HtmlWeb().Load(
                $"https://wordpress.org/support/plugin/{match.Value}/reviews/page/1/"
            );
            var pagesNodes = html.DocumentNode.SelectNodes("//a[@class='page-numbers']");
            if (pagesNodes != null)
            {
                pageNumber = pagesNodes.Select(node => int.Parse(node.InnerText)).Max();
            }

            // get all reviews
            var plugin = new Plugin { Url = pluginUrl };

            for (int i = 1; i <= pageNumber; i++)
            {
                var reviewPageUrl =
                    $"https://wordpress.org/support/plugin/{match.Value}/reviews/page/{i}/";
                Console.WriteLine($"Parsing review page: {reviewPageUrl}");
                html = new HtmlWeb().Load(reviewPageUrl);

                var topicPermaLinks = html.DocumentNode.SelectNodes(
                    "//a[@class='bbp-topic-permalink']"
                );

                if (topicPermaLinks != null)
                {
                    for (var i2 = 0; i2 < topicPermaLinks.Count; i2++)
                    {
                        var review = new UserReview();
                        html = new HtmlWeb().Load(
                            topicPermaLinks[i2].GetAttributeValue("href", string.Empty)
                        );

                        Console.WriteLine(
                            $"Parsing review: {topicPermaLinks[i2].GetAttributeValue("href", string.Empty)}"
                        );

                        // get span class bbp-author-name
                        var authorUrl = html.DocumentNode.SelectSingleNode(
                            "//div[@class='bbp-topic-author']//a"
                        );

                        // get p class bbp-topic-post-date > a title
                        var postDate = html.DocumentNode.SelectSingleNode(
                            "//p[@class='bbp-topic-post-date']/a"
                        );

                        if (authorUrl != null && postDate != null)
                        {
                            review.Url = topicPermaLinks[i2]
                                .GetAttributeValue("href", string.Empty);
                            review.ReviewCreated = DateTime.ParseExact(
                                postDate.GetAttributeValue("title", string.Empty),
                                "MMMM d, yyyy 'at' h:mm tt",
                                CultureInfo.InvariantCulture
                            );
                            review.AuthorUrl = authorUrl.GetAttributeValue("href", string.Empty);
                            plugin.Reviews.Add(review);
                        }
                    }
                }
            }
            plugins.Add(plugin);
        }
        return plugins;
    }
}
