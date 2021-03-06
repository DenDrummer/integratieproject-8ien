﻿using IP3_8IEN.BL.Domain.Data;
using System;
using System.Collections.Generic;
using IP3_8IEN.BL.Domain.Dashboard;
using IP_8IEN.BL.Domain.Dashboard;

namespace IP3_8IEN.BL
{
    public interface IDataManager
    {
        #region Alerts
        void GetAlerts();
        #endregion

        #region Onderwerpen
        IEnumerable<Onderwerp> ReadOnderwerpen();

        #region Organisaties
        Organisatie GetOrganisatie(int organisatieId);
        IEnumerable<Organisatie> GetOrganisaties();
        void ChangeOrganisation(Organisatie organisatie);
        Organisatie AddOrganisation(string naamOrganisatie);
        void AddOrganisations(string filePath);
        #endregion

        #region Personen
        void ChangePersoon(Persoon persoon);
        Persoon GetPersoon(string naam);
        Persoon GetPersoon(int persoonId);
        IEnumerable<Persoon> GetPersonen();
        IEnumerable<Persoon> GetPersonenOnly();
        //6 apr 2018 : Stephane
        void ApiRequestToJson(bool isReCheck = false);
        string ExportToCSV(IEnumerable<Persoon> personen);

        //16 apr 2018 : Stephane
        void AddMessages(string json);
        Persoon AddPersoon(string naam);
        void AddPersonen(string path);
        Persoon GetPersoonWithTewerkstelling(string naam);
        Persoon GetPersoonWithTewerkstelling(int id);
        List<int> ExtractListPersoonId(IEnumerable<GraphData> graphDataList);
        double GetPolarityByPerson(Persoon persoon);
        double GetPolarityByPerson(Persoon persoon, DateTime start);
        double GetPolarityByPerson(Persoon persoon, DateTime start, DateTime stop);
        double GetObjectivityByPerson(Persoon persoon);
        double GetObjectivityByPerson(Persoon persoon, DateTime start);
        double GetObjectivityByPerson(Persoon persoon, DateTime start, DateTime stop);
        int CountSubjMsgsPersoon(Onderwerp onderwerp); //Onderwerp onderwerp
        #endregion

        #region Themas
        //WIP in de ThemaMethodes branch op de github
        #endregion
        #endregion

        #region (Subject)Messages
        IEnumerable<Message> ReadMessagesWithSubjMsgs();
        //void AddSubjectMessage(Message msg, Hashtag hashtag);
        SubjectMessage AddSubjectMessage(Message msg, Persoon persoon);
        //void AddMessages(string sourceUrl);
        #endregion

        #region Tewerkstellingen
        void AddTewerkstelling(Persoon persoon, string organisatie);
        void AddTewerkstelling(string naam, string organisatieNaam);
        #endregion

        IEnumerable<Hashtag> GetHashtags();
        void UpdateHashtags(IEnumerable<Hashtag> hashtags);
        void CreateTheme(string naam, string beschrijving, IEnumerable<Hashtag> hashForTheme);
        IEnumerable<Thema> GetThemas();
        void UpdateThema(Thema thema);
        List<GraphData> GetTweetsPerDag(Organisatie organisatie, int aantalDagenTerug);
        List<GraphData> GetTweetsPerDag(Organisatie organisatie, string town, int aantalDagenTerug);
        List<GraphData> GetTweetsPerDag(Thema organisatie, int aantalDagenTerug);
        List<GraphData> GetNumberGraph(Persoon persoon, int laatsteAantalUren);
        List<GraphData> GetNumberGraph(Organisatie organisatie, int laatsteAantalUren);
        List<GraphData> GetNumberGraph(Thema thema, int laatsteAantalUren);
        Thema GetThema(int id);
        List<GraphData> GetTweetsPerDagList(Organisatie organisatie, int aantalDagenTerug);
        List<GraphData> GetTweetsPerDagList(Thema thema, int aantalDagenTerug);
        SubjectMessage AddSubjectMessage(Message msg, Hashtag hashtag);
        IEnumerable<Hashtag> GetHashtagsWithSubjMsgs();
        List<string> GetTowns(IEnumerable<Persoon> personen);
        int GetAantalVermeldingen(Thema theme);

        IEnumerable<ViewDataValue> GetViewDataValues();
        ViewDataValue GetViewDataValue(string name);
        void UpdateViewDataValue(ViewDataValue vdv);
        void AddViewDataValue(ViewDataValue vdv);

        #region Unsorted
        int GetMentionCountByName(string naam);
        int GetMentionCountByName(string naam, DateTime start);
        int GetMentionCountByName(string naam, DateTime start, DateTime stop);
        List<GraphData> GetTopWordsCount();
        List<GraphData> GetTopWordsCount(int aantal);
        List<GraphData> GetTopWordsCount(int aantal, DateTime start);
        List<GraphData> GetTopWordsCount(int aantal, DateTime start, DateTime stop);
        List<GraphData> GetTopWordsCount(DateTime start, DateTime stop);
        int GetWordCountByName(string name);
        int GetWordCountByName(string name, DateTime start);
        int GetWordCountByName(string name, DateTime start, DateTime stop);
        List<GraphData> GetComparisonPersonNumberOfTweets(Persoon p1, Persoon p2);
        List<GraphData> GetComparisonPersonNumberOfTweets(Persoon p1, Persoon p2, Persoon p3);
        List<GraphData> GetComparisonPersonNumberOfTweets(Persoon p1, Persoon p2, Persoon p3, Persoon p4);
        List<GraphData> GetComparisonPersonNumberOfTweets(Persoon p1, Persoon p2, Persoon p3, Persoon p4, Persoon p5);
        List<GraphData> GetTopStoryCount();
        List<GraphData> GetTopStoryCount(int aantal);
        List<GraphData> GetTopStoryCount(int aantal, DateTime start);
        List<GraphData> GetTopStoryCount(int aantal, DateTime start, DateTime stop);
        List<GraphData> GetTopStoryCount(DateTime start, DateTime stop);
        List<GraphData> GetTopStoryByPolitician(Persoon persoon);
        List<GraphData> GetComparisonPersonNumberOfTweetsOverTime(Persoon p1, Persoon p2, Persoon p3, Persoon p4, Persoon p5);
        List<GraphData> GetTopMentions(int aantal);
        void SendMail();
        List<GraphData> GetRanking(int aantal, int interval_uren, bool puntNotatie = false);
        int GetNumber(Persoon persoon, int laatsteAantalUren = 0);
        List<GraphData> GetTweetsPerDag(Persoon persoon, int aantalDagenTerug = 0, string town = null);
        List<GraphData> GetTweetsPerDagList(Persoon persoon, int aantalDagenTerug = 0);
        List<GraphData> GetTweetsPerDagComparisonOverTime(Persoon persoon1, Persoon persoon2, Persoon persoon3, Persoon persoon4,
            Persoon persoon5, int aantalDagenTerug = 10);
        string GetImageString(string screenname);
        string GetBannerString(string screenname);
        Hashtag AddHashtag(string hashtag);
        void InitNonExistingRepo(bool withUnitOfWork);
        List<DataChart> GetTweetsPerDagDataChart(Persoon persoon, int aantalDagenTerug = 0);
        List<GraphData> FrequenteWoorden(ICollection<SubjectMessage> subjMsgs, int ammount);
        Persoon GetPersoonWithSjctMsg(int persoonId);
        List<String> TopWordsCountByPerson(Persoon persoon);
        List<String> TopStoryCountByPerson(Persoon persoon);
        List<String> TopWordsCountByOrganisatie(Organisatie organisatie);
        List<String> TopStoryCountByOrganisatie(Organisatie organisatie);
        List<String> TopHashtagCountByPerson(Persoon persoon);
        List<String> TopHashtagCountByOrganisation(Organisatie organisatie);
        string GetTrendByOnderwerp(Onderwerp onderwerp);
        List<double> GetTotalMessagesSparkline();
        double GetstijgingTweets();
        #endregion
    }
}