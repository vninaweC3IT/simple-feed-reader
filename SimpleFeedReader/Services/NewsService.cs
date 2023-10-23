using System;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using AutoMapper;
using System.Threading.Tasks;

namespace SimpleFeedReader.Services
{
    public class NewsService
    {
        private readonly IMapper _mapper;

        public NewsService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<List<NewsStoryViewModel>> GetNews(string feedUrl)
        {
            var news = new List<NewsStoryViewModel>();
            var feedUri = new Uri(feedUrl);

            var xmlReaderSettings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Parse };
            using (var webClient = new WebClient())
            using (var xmlStream = webClient.OpenRead(feedUri))
            using (var xmlReader = XmlReader.Create(xmlStream, xmlReaderSettings))
            {
                try
                {
                    var feedReader = new RssFeedReader(xmlReader);

                    while (await feedReader.Read())
                    {
                        switch (feedReader.ElementType)
                        {
                            // RSS Item
                            case SyndicationElementType.Item:
                                ISyndicationItem item = await feedReader.ReadItem();
                                var newsStory = _mapper.Map<NewsStoryViewModel>(item);
                                news.Add(newsStory);
                                break;

                            // Something else
                            default:
                                break;
                        }
                    }
                }
                catch (XmlException xmlEx)
                {
                    // Handle XML parsing errors
                    // Log the error and optionally throw or handle it as needed
                }
                catch (WebException webEx)
                {
                    // Handle web-related errors (e.g., network issues)
                    // Log the error and optionally throw or handle it as needed
                }
            }

            return news.OrderByDescending(story => story.Published).ToList();
        }
    }

    public class NewsStoryProfile : Profile
    {
        public NewsStoryProfile()
        {
            CreateMap<ISyndicationItem, NewsStoryViewModel>()
                .ForMember(dest => dest.Uri, opts => opts.MapFrom(src => src.Id));
        }
    }
}
