﻿using IP_8IEN.BL.Domain.Data;
using System;
using System.Collections.Generic;
using IP3_8IEN.BL.Domain.Dashboard;

namespace IP_8IEN.BL
{
    public interface IDataManager
    {
        //16 mrt 2018 : Stephane
        //void AddMessages(string sourceUrl);

        //25 mrt 2018 : Stephane
        //Persoon AddPersoon(string voornaam, string achternaam);
        SubjectMessage AddSubjectMessage(Message msg, Persoon persoon);  //20 apr 2018 Victor (update: subjmes toevoegen aan tweet)

        //28 mrt 2018 : Stephane
        Hashtag AddHashtag(string hashtag);
        void AddSubjectMessage(Message msg, Hashtag hashtag);

        //30 mrt 2018 : Stephane
        IEnumerable<Onderwerp> ReadOnderwerpen();

        //31 mrt 2018 : Stephane
        void initNonExistingRepo(bool withUnitOfWork);

        //2 apr 2018 : Stephane
        Organisatie AddOrganisation(string naamOrganisatie);
        void AddOrganisations(string filePath);
        void AddTewerkstelling(string naam, string organisatieNaam);

        //6 apr 2018 : Stephane
        void ApiRequestToJson();

        //16 apr 2018 : Stephane
        void AddMessages(string json);
        Persoon AddPersoon(string naam);


        //22 apr 2018 : Stephane
        void AddPersonen(string path);
        void AddTewerkstelling(Persoon persoon, string organisatie);
        int CountSubjMsgsPersoon(Onderwerp onderwerp); //Onderwerp onderwerp


        //23 apr 2018 : Stephane
        //IEnumerable<Onderwerp> ReadOnderwerpenWithSubjMsgs(); [verwijderd] 4 mei 2018 : Stephane
        IEnumerable<Message> ReadMessagesWithSubjMsgs();

        //24 apr 2018 : Victor
        void GetAlerts();

        //27 apr 2018 : Victor
        void SendMail();
        //Sam
        List<GraphData> GetRanking(int aantal, int interval_uren, bool puntNotatie = true);
        //Dictionary<Persoon, double> GetRanking(int aantal, int interval_uren, bool puntNotatie = true);
        int GetNumber(Persoon persoon, int laatsteAantalUren = 0);
        //Sam
        //Dictionary<DateTime, int> GetTweetsPerDag(Persoon persoon, int aantalDagenTerug = 0);
        List<GraphData> GetTweetsPerDag(Persoon persoon, int aantalDagenTerug = 0);

        List<GraphData2> GetTweetsPerDag2(Persoon persoon1, Persoon persoon2, Persoon persoon3, Persoon persoon4,
            Persoon persoon5, int aantalDagenTerug = 0);

        //3 mei 2018 : Stephane
        Persoon GetPersoon(int persoonId);
        Organisatie GetOrganisatie(int organisatieId);

        //8 mai 2018 : Stephane
        IEnumerable<Persoon> GetPersonen();

        //Sam 15 mei
        string GetImageString(int id);
        string GetBannerString(int id);
    }
}
