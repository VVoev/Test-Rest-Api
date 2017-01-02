namespace ApiTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    class TV
    {
        [JsonProperty("original_name")]
        public string Name { get; set; }

        public SortedSet<Season> Seasons { get; set; }
    }

    class Season : IComparable<Season>
    {
        [JsonProperty("season_number")]
        public int Id { get; set; }

        [JsonProperty("episode_count")]
        public int EpisodeCount { get; set; }

        protected bool Equals(Season other)
        {
            return this.Id == other.Id && this.EpisodeCount == other.EpisodeCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Season) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id*397) ^ this.EpisodeCount;
            }
        }

        public static bool operator ==(Season left, Season right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Season left, Season right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(Season other)
        {
            if (this.Id == other.Id)
            {
                return other.EpisodeCount.CompareTo(this.EpisodeCount);
            }
            return other.Id.CompareTo(this.Id);
        }
    }


    class Episode
    {
        public List<Character> Cast { get; set; }
        public List<Character> Crew { get; set; }
        [JsonProperty("guest_stars")]
        public List<Character> Guests { get; set; }
    }

    internal class Character
    {
        public int Id { get; set; }

        [JsonProperty("name")]
        public string RealName { get; set; }

        [JsonProperty("character")]
        public string CastName { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        const string ApiKey = "4fe9af9b19fc0feceb89a23a761bcb3c";
        const string ApiUrl = "https://api.themoviedb.org/3/";

        [TestMethod]
        public void TestSeasons_SeasonIdAndEpisodesEquality_ShouldPassCorrectly()
        {
            string response = GetResponse("tv", 1396);
            TV tv = JsonConvert.DeserializeObject<TV>(response);
            SortedSet<Season> actual = tv.Seasons;
            SortedSet<Season> expected = new SortedSet<Season>
            {
                new Season {Id = 5, EpisodeCount = 16},
                new Season {Id = 4, EpisodeCount = 13},
                new Season {Id = 1, EpisodeCount = 8},
                new Season {Id = 3, EpisodeCount = 13},
                new Season {Id = 2, EpisodeCount = 13}
            };

           CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSeasons_CountEpisodesPerSeason()
        {
            string response = GetResponse("tv", 1396);
            TV tv = JsonConvert.DeserializeObject<TV>(response);
            SortedSet<Season> actual = tv.Seasons;

            bool passed = true;
            string msg = "";
            foreach (Season season in actual)
            {
                if (season.Id == 5)
                {
                    if (season.EpisodeCount != 16)
                    {
                        passed = false;
                        msg += "\r\n Season 5: Expected: [" + 16 + "], Actual: [" + season.EpisodeCount + "]";
                    }
                }
                if (season.Id == 4)
                {
                    if (season.EpisodeCount != 13)
                    {
                        passed = false;
                        msg += "\r\n Season 4: Expected: [" + 13 + "], Actual: [" + season.EpisodeCount + "]";
                    }
                }
                if (season.Id == 3)
                {
                    if (season.EpisodeCount != 13)
                    {
                        passed = false;
                        msg += "\r\n Season 3: Expected: [" + 13 + "], Actual: [" + season.EpisodeCount + "]";
                    }
                }
                if (season.Id == 2)
                {
                    if (season.EpisodeCount != 13)
                    {
                        passed = false;
                        msg += "\r\n Season 2: Expected: [" + 13 + "], Actual: [" + season.EpisodeCount + "]";
                    }
                }
                if (season.Id == 1)
                {
                    if (season.EpisodeCount != 7)
                    {
                        passed = false;
                        msg += "\r\n Season 1: Expected: [" + 7 + "], Actual: [" + season.EpisodeCount + "]";
                    }
                }
            }

            if (!passed)
            {
                Assert.Fail(msg);
            }
        }

        [TestMethod]
        public void TestActors_IsNotInSeason()
        {
            WebClient web = new WebClient();
            string response =
                web.DownloadString(
                    "https://api.themoviedb.org/3/tv/1399/season/6/episode/1/credits?api_key=4fe9af9b19fc0feceb89a23a761bcb3c");

            Episode episode = JsonConvert.DeserializeObject<Episode>(response);

            bool result = episode.Cast.Any(
                character => character.RealName == "Nell Tiger Free"
                             && character.CastName == "Myrcella Baratheon")
                          ||
                          episode.Guests.Any(
                              character => character.RealName == "Nell Tiger Free"
                                           && character.CastName == "Myrcella Baratheon");

            Assert.IsFalse(result, "The character is present in the current season");
        }

        [TestMethod]
        public void TestActors_IsMainActor()
        {
            WebClient web = new WebClient();
            string response =
                web.DownloadString(
                    "https://api.themoviedb.org/3/tv/1399/season/5/episode/1/credits?api_key=4fe9af9b19fc0feceb89a23a761bcb3c");

            Episode episode = JsonConvert.DeserializeObject<Episode>(response);

            bool isMain
                = episode.Cast.Any(character => character.RealName == "Nell Tiger Free"
                                                && character.CastName == "Myrcella Baratheon");

            bool isGuest
                = episode.Guests.Any(character => character.RealName == "Nell Tiger Free"
                                                && character.CastName == "Myrcella Baratheon");

            Assert.IsTrue(isMain && !isGuest);
        }

        [TestMethod]
        public void TestActors_IsMainActorAndGuest()
        {
            WebClient web = new WebClient();
            string response =
                web.DownloadString(
                    "https://api.themoviedb.org/3/tv/1399/season/5/episode/2/credits?api_key=4fe9af9b19fc0feceb89a23a761bcb3c");

            Episode episode = JsonConvert.DeserializeObject<Episode>(response);

            bool isMain
                = episode.Cast.Any(character => character.RealName == "Nell Tiger Free"
                                                && character.CastName == "Myrcella Baratheon");

            bool isGuest
                = episode.Guests.Any(character => character.RealName == "Nell Tiger Free"
                                                && character.CastName == "Myrcella Baratheon");

            Assert.IsTrue(isMain && isGuest);
        }
        private string GetResponse(string resource, int? id)
        {
            WebClient web = new WebClient();
            string response = web.DownloadString(
                ApiUrl
                +
                resource
                + "/"
                + id
                + "?api_key=" + ApiKey
                );

            return response;
        }
    }
}
