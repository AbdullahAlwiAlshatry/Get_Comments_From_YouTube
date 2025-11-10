using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Get_Comments_From_YouTube
{


    public class Rootobject_
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public Pageinfo_ pageInfo { get; set; }
        public Item_[] items { get; set; }
    }

        public class Pageinfo_
        {
            public int resultsPerPage { get; set; }
        }

    public class Item_
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public Snippet_ snippet { get; set; }

        public override string ToString() { return snippet.textOriginal; }
    }

        public class Snippet_
        {
            public string channelId { get; set; }
            public string textDisplay { get; set; }
            public string textOriginal { get; set; }
            public string parentId { get; set; }
            public string authorDisplayName { get; set; }
            public string authorProfileImageUrl { get; set; }
            public string authorChannelUrl { get; set; }
            public Authorchannelid_ authorChannelId { get; set; }
            public bool canRate { get; set; }
            public string viewerRating { get; set; }
            public int likeCount { get; set; }
            public DateTime publishedAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        public class Authorchannelid_
        {
            public string value { get; set; }
        }


    }
