﻿using System;
using System.Collections.Generic;

namespace IP_8IEN.BL.Domain.Data
{
    public class Message
    {
        //PK

        public int MessageId { get; set; }
        public string Source { get; set; }
        //id van oorspronkelijke tweet, misschien niet nodig
        public int Id { get; set; }
        //user id van oorspronkelijke tweet
        public int UserId { get; set; }
        //geo format nog onbekend
        public string Geo { get; set; }
        public List<string> Mentions { get; set; }
        public bool Retweet { get; set; }
        public DateTime Date { get; set; }
        public List<string> Words { get; set; }
        //twee getallen tussen -1 en 1
        public Sentiment Sentiment { get; set; }
        public List<string> Hashtags { get; set; }
        public List<string> Urls { get; set; }
        public string Politician { get; set; }

        //wat zou dit representeren uit de testdata? ~Jorden
        public ICollection<SubjectMessage> SubjectMessages { get; private set; }


        public Message(int msgId, string source, int id, int userId, string geo, List<string> mentions, bool retweet, DateTime date, List<string> words, Sentiment sentiment, List<string> hashtags, List<string> urls, string politician)
        {
            MessageId = msgId;
            Source = source;
            Id = id;
            UserId = userId;
            Geo = geo;
            Mentions = mentions;
            Retweet = retweet;
            Date = date;
            Words = words;
            Sentiment = sentiment;
            Hashtags = hashtags;
            Urls = urls;
            Politician = politician;
        }

        public Message()
        {
        }
    }
}