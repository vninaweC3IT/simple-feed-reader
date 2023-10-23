using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SimpleFeedReader.Services;
using SimpleFeedReader.ViewModels;

namespace SimpleFeedReader.Pages
{
    public class IndexModel : PageModel
    {
        private readonly NewsService _newsService;
        private readonly IConfiguration _configuration;  
   
        public IndexModel(NewsService newsService, IConfiguration configuration)
        {
            _newsService = newsService;
            _configuration = configuration;
        }

        public string ErrorText { get; private set; }

        public List<NewsStoryViewModel> NewsItems { get; private set; }

        public async Task OnGet()
        {
            ViewData["Header"] = _configuration.GetValue<string>("UI:Index:Header");
            string feedUrl = Request.Query["feedurl"];

            if (!string.IsNullOrEmpty(feedUrl))
            {
                try
                {
                    NewsItems = await _newsService.GetNews(feedUrl);
                }
                catch (UriFormatException)
                {
                    ErrorText = "There was a problem parsing the URL.";
                    return;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    ErrorText = "Unknown host name.";
                    return;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    ErrorText = "Syndication feed not found.";
                    return;
                }
                catch (AggregateException ae)
                {
                    ae.Handle((x) =>
                    {
                        if (x is XmlException)
                        {
                            ErrorText = "There was a problem parsing the feed. Are you sure that URL is a syndication feed?";
                            return true;
                        }
                        return false;
                    });
                }
            }
        }
    }
}
