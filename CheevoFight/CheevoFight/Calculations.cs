using CheevoFight.Tools;
using CheevoFight.ViewPlusViewModel;
using CheevoFight.ViewPlusViewModel.Windows;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CheevoFight
{
    internal class Calculations
    {
        private static (string, string) PlayerSteamNameAndAvatar { get; set; }
        private static Dictionary<string, string> PlayerGamesOwnedNameByAppId { get; set; }
        private static Dictionary<string, (string, string)> SteamNamesAndAvatarsByFriendSteamId { get; set; }
        private static Dictionary<string, HashSet<string>> MutuallyOwnedAppIdsByFriendSteamId { get; set; }
        private static Dictionary<string, int> PlayerAchievementsByAppId { get; set; }
        private static Dictionary<string, Dictionary<string, int>> AchievementsByAppIdByFriendSteamId { get; set; }
        private static Dictionary<string, (string, HashSet<string>)> CapsuleAndTagsByAppId { get; set; }
        private static Dictionary<string, Dictionary<string, HashSet<string>>> MutuallyOwnedGameNamesByTagByFriendSteamId { get; set; }
        private static Dictionary<string, Dictionary<string, HashSet<string>>> MutuallyOwnedCapsuleImagesByTagByFriendSteamId { get; set; }
        private static Dictionary<string, OrderedDictionary<string, int>> PlayerPercentageByMututallyOwnedTagByFriendSteamId { get; set; }
        private static int APICalls { get; set; }
        private static HashSet<string> AppIdsWithoutAchievements { get; set; }


        static Calculations()
        {
            Calculations.PlayerGamesOwnedNameByAppId = new Dictionary<string, string>();
            Calculations.SteamNamesAndAvatarsByFriendSteamId = new Dictionary<string, (string, string)>();
            Calculations.MutuallyOwnedAppIdsByFriendSteamId = new Dictionary<string, HashSet<string>>();
            Calculations.PlayerAchievementsByAppId = new Dictionary<string, int>();
            Calculations.AchievementsByAppIdByFriendSteamId = new Dictionary<string, Dictionary<string, int>>();
            Calculations.CapsuleAndTagsByAppId = new Dictionary<string, (string, HashSet<string>)>();
            Calculations.MutuallyOwnedGameNamesByTagByFriendSteamId = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            Calculations.MutuallyOwnedCapsuleImagesByTagByFriendSteamId = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            Calculations.PlayerPercentageByMututallyOwnedTagByFriendSteamId = new Dictionary<string, OrderedDictionary<string, int>>();
            Calculations.APICalls = 0;
            Calculations.AppIdsWithoutAchievements = new HashSet<string>();
        }


        public static async Task CompileData(ViewModel viewModel)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var progressBarWindow = new ViewPlusViewModel.Windows.ProgressBar(viewModel);
            progressBarWindow.Show();
            progressBarWindow.Activate();

            var steamId = await GetSteamIdFromProfileURLAsync();
            await SetPlayerSteamNameAndAvatarAsync(steamId);
            await SetPlayerOwnedAppIdsAsync(steamId);
            await SetSteamNamesAndAvatarsByFriendSteamIdAsync(steamId);
            if (Calculations.SteamNamesAndAvatarsByFriendSteamId.Count == 0)
            {
                MessageBox.Show(
                    "Unable to retrieve any friends for this Steam profile. If this is your profile, change your friends list " +
                    "privacy setting to public. The program will now close."
                );
                progressBarWindow.Close();
                return;
            }
            await SetMutuallyOwnedAppIdsByFriendSteamIdAsync();
            RemoveFriendsWithNoDataDueToPrivacySettings();
            await SetPlayerAchievementsByAppIdAsync(steamId);
            await SetAchievementsByAppIdByFriendSteamIdAsync();
            await SetCapsuleAndTagsByAppIdAsync();
            SetPlayerPercentageByMututallyOwnedTagByFriendSteamId();
            PrepareToDisplayResults(viewModel);

            progressBarWindow.Close();

            var resultsWindow = new Results(viewModel);
            resultsWindow.ShowDialog();

            stopwatch.Stop();
            Debug.WriteLine(stopwatch.Elapsed.ToString());
            Debug.WriteLine(Calculations.APICalls.ToString());
            MessageBox.Show("All Done.");

            progressBarWindow.Close();
        }


        private static async Task<string> GetSteamIdFromProfileURLAsync()
        {
            _ = new ProgressBarManager("Default")
            {
                LabelContentTask = "Retrieving Steam Id",
                ProgressBarValue = 0,
                ProgressBarMaximum = 1
            };

            using var httpClient = new HttpClient();
            var pageHtml = await httpClient.GetStringAsync(ViewModel.SteamProfileURL);
            var startPosition = pageHtml.IndexOf('7', pageHtml.IndexOf("\"steamid\":\""));
            var endPosition = pageHtml.IndexOf('\"', startPosition);
            return pageHtml.Mid(startPosition, endPosition - startPosition);
        }


        private static async Task SetPlayerSteamNameAndAvatarAsync(string steamId)
        {
            ProgressBarManager.IncrementValue("Default");
            ProgressBarManager.IncrementMaximum("Default", 1);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving your Steam name and avatar");

            Calculations.PlayerSteamNameAndAvatar = await GetSteamNameAndAvatar(steamId);
        }


        private static async Task<(string, string)> GetSteamNameAndAvatar(string steamId)
        {
            using var httpClient = new HttpClient();
            var jSONPersona = await httpClient.GetStringAsync("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + ViewModel.SteamWebAPIKey + "&steamids=" + steamId);
            Calculations.APICalls++;

            var startPosition = jSONPersona.IndexOf(':', jSONPersona.IndexOf("\"personaname\":\"")) + 2;
            var endPosition = jSONPersona.IndexOf('\"', startPosition);
            var name = jSONPersona.Mid(startPosition, endPosition - startPosition);

            startPosition = jSONPersona.IndexOf(':', jSONPersona.IndexOf("\"avatar\":\"")) + 2;
            endPosition = jSONPersona.IndexOf('\"', startPosition);
            var pathWebAvatar = jSONPersona.Mid(startPosition, endPosition - startPosition);

            var streamAvatar = await GetStreamOfCapsuleImageFromWebAsync(pathWebAvatar);
            var pathDisk = Path.GetTempFileName();
            using var fileStream = File.Create(pathDisk);
            streamAvatar.CopyTo(fileStream);

            return (name, pathDisk);
        }


        private static async Task SetPlayerOwnedAppIdsAsync(string steamId)
        {
            ProgressBarManager.IncrementValue("Default");
            ProgressBarManager.IncrementMaximum("Default", 1);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving your owned games");

            Calculations.PlayerGamesOwnedNameByAppId = await GetGamesOwnedNameByAppId(steamId);
        }


        private static async Task<Dictionary<string, string>> GetGamesOwnedNameByAppId(string steamId)
        {
            using var httpClient = new HttpClient();
            var jSONOwnedGames = await httpClient.GetStringAsync("http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + ViewModel.SteamWebAPIKey + "&steamid=" + steamId + "&include_appinfo=1&include_played_free_games=1");
            Calculations.APICalls++;

            var gamesOwnedNameByAppId = new Dictionary<string, string>();
            var allAppIdIndicators = jSONOwnedGames.GetAllIndicesOfSubstring("\"appid\":");
            foreach (var appIdIndicator in allAppIdIndicators)
            {
                var startPosition = jSONOwnedGames.IndexOf(':', appIdIndicator) + 1;
                var endPosition = jSONOwnedGames.IndexOf(',', startPosition);
                var appId = jSONOwnedGames.Mid(startPosition, endPosition - startPosition);

                startPosition = jSONOwnedGames.IndexOf(':', jSONOwnedGames.IndexOf("\"name\":\"", endPosition)) + 2;
                endPosition = jSONOwnedGames.IndexOf('\"', startPosition);
                var gameName = jSONOwnedGames.Mid(startPosition, endPosition - startPosition);

                gamesOwnedNameByAppId.Add(appId, gameName);
            }

            return gamesOwnedNameByAppId;
        }


        private static async Task SetSteamNamesAndAvatarsByFriendSteamIdAsync(string steamId)
        {
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving your friends' Steam names and avatars");

            string? jSONSteamFriends = null;
            using var httpClient = new HttpClient();
            try
            {
                jSONSteamFriends = await httpClient.GetStringAsync("https://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key=" + ViewModel.SteamWebAPIKey + "&steamid=" + steamId + "&relationship=friend");
            }
            catch (HttpRequestException exception)
            {
                if (exception.Message.Equals("Response status code does not indicate success: 401 (Unauthorized)."))
                {
                    return;
                }
            }

            Calculations.APICalls++;

            var allSteamIdIndicators = jSONSteamFriends.GetAllIndicesOfSubstring("\"steamid\":\"");
            ProgressBarManager.IncrementMaximum("Default", allSteamIdIndicators.Count);
            foreach (var startPositionIndicator in allSteamIdIndicators)
            {
                var startPosition = jSONSteamFriends.IndexOf('7', startPositionIndicator);
                var endPosition = jSONSteamFriends.IndexOf('\"', startPosition);
                var friendSteamId = jSONSteamFriends.Mid(startPosition, endPosition - startPosition);
                Calculations.SteamNamesAndAvatarsByFriendSteamId.Add(
                    friendSteamId,
                    await GetSteamNameAndAvatar(friendSteamId)
                );

                ProgressBarManager.IncrementValue("Default");
            }
        }


        private static async Task SetMutuallyOwnedAppIdsByFriendSteamIdAsync()
        {
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving your mutually owned games with each friend");
            ProgressBarManager.IncrementMaximum("Default", Calculations.SteamNamesAndAvatarsByFriendSteamId.Count);

            foreach (var friendSteamId in Calculations.SteamNamesAndAvatarsByFriendSteamId.Keys)
            {
                var friendGamesOwnedNameByAppId = await GetGamesOwnedNameByAppId(friendSteamId);
                var mutuallyOwnedAppIds = Calculations.PlayerGamesOwnedNameByAppId.Keys.Intersect(
                    friendGamesOwnedNameByAppId.Keys
                ).ToHashSet();
                Calculations.MutuallyOwnedAppIdsByFriendSteamId.Add(friendSteamId, mutuallyOwnedAppIds);

                ProgressBarManager.IncrementValue("Default");
            }
        }


        private static void RemoveFriendsWithNoDataDueToPrivacySettings()
        {
            var friendSteamIdsWithNoData = new List<string>();
            foreach (var keyValuePair in Calculations.MutuallyOwnedAppIdsByFriendSteamId)
            {
                if (keyValuePair.Value.Count == 0)
                {
                    friendSteamIdsWithNoData.Add(keyValuePair.Key);
                }
            }

            for (var i = 0; i < friendSteamIdsWithNoData.Count; i++)
            {
                var friendSteamId = friendSteamIdsWithNoData[i];
                Calculations.MutuallyOwnedAppIdsByFriendSteamId.Remove(friendSteamId);
                Calculations.SteamNamesAndAvatarsByFriendSteamId.Remove(friendSteamId);
            }
        }


        private static async Task SetPlayerAchievementsByAppIdAsync(string steamId)
        {
            ProgressBarManager.IncrementValue("Default");
            ProgressBarManager.IncrementMaximum("Default", 1);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving your achievement count per game");

            Calculations.PlayerAchievementsByAppId = await GetAchievementsByAppIdForGivenSteamIdAndAppIds(
                PlayerGamesOwnedNameByAppId.Keys.ToHashSet(), steamId
            );
        }


        private static async Task<Dictionary<string, int>> GetAchievementsByAppIdForGivenSteamIdAndAppIds(
            HashSet<string> appIds, string steamId
        )
        {
            var appIdsAsList = appIds.ToList();
            var achievementsByAppId = new Dictionary<string, int>();

            // 20250831 Found approximate limit by trial and error - worked at 430/failed at 432 but keeping it at 400 to
            // accommodate appids of increasing length. Limit may possibly be byte size of url and not # of appIds.
            var numberOfRunsNeeded = (appIds.Count % 400 > 0) ? (appIds.Count / 400) + 1 : appIds.Count / 400;
            for (var i = 0; i < numberOfRunsNeeded; i++)
            {
                var appFilter = string.Empty;
                for (var j = 0; j < 400 && (j + (400 * i)) < appIds.Count; j++)
                {
                    appFilter += "&appids[" + j + "]=" + appIdsAsList[j + (400 * i)];
                }

                using var httpClient = new HttpClient();
                var jSONAchievements = await httpClient.GetStringAsync("https://api.steampowered.com/IPlayerService/GetTopAchievementsForGames/v1/?key=" + ViewModel.SteamWebAPIKey + "&steamid=" + steamId + "&max_achievements=10000" + appFilter);
                Calculations.APICalls++;

                var allAppIdIndicators = jSONAchievements.GetAllIndicesOfSubstring("\"appid\":");
                foreach (var appIdIndicator in allAppIdIndicators)
                {
                    var startPosition = jSONAchievements.IndexOf(':', appIdIndicator) + 1;
                    var endPosition = GetEndPositionOfAppIdInTopAchievementsEntry(jSONAchievements, startPosition);
                    var appId = jSONAchievements.Mid(startPosition, endPosition - startPosition);

                    var totalAchievements = 0;
                    var positionOfNextAchievementsIndicator = jSONAchievements.IndexOf("\"total_achievements\":", endPosition);
                    var positionOfNextAppId = jSONAchievements.IndexOf("\"appid\":", endPosition);
                    if (
                        positionOfNextAchievementsIndicator != -1 && 
                        (
                            positionOfNextAchievementsIndicator < positionOfNextAppId ||
                            positionOfNextAppId == -1
                        )
                    )
                    {
                        string unlockedAchievementsBlock;
                        if (positionOfNextAppId == -1)
                        {
                            unlockedAchievementsBlock = jSONAchievements.Substring(
                                positionOfNextAchievementsIndicator, jSONAchievements.Length - positionOfNextAchievementsIndicator
                            );
                        }
                        else
                        {
                            unlockedAchievementsBlock = jSONAchievements.Substring(
                                positionOfNextAchievementsIndicator, positionOfNextAppId - positionOfNextAchievementsIndicator
                            );
                        }
                        totalAchievements = unlockedAchievementsBlock.GetAllIndicesOfSubstring("\"name\":").Count;
                        achievementsByAppId.Add(appId, totalAchievements);
                    }
                    else
                    {
                        Calculations.AppIdsWithoutAchievements.Add(appId);
                    }
                }
            }

            return achievementsByAppId;
        }


        private static int GetEndPositionOfAppIdInTopAchievementsEntry(string? jSONAchievements, int startPosition)
        {
            var endPositionComma = jSONAchievements.IndexOf(',', startPosition);
            var endPositionRightBrace = jSONAchievements.IndexOf('}', startPosition);
            return Math.Min(endPositionComma, endPositionRightBrace);
        }


        private static async Task SetAchievementsByAppIdByFriendSteamIdAsync()
        {
            ProgressBarManager.IncrementMaximum("Default", Calculations.MutuallyOwnedAppIdsByFriendSteamId.Count);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving friends' achievement counts per mutually owned game");

            var achievementsByAppIdByFriendSteamId = new Dictionary<string, Dictionary<string, int>>();

            foreach (var keyValuePair in Calculations.MutuallyOwnedAppIdsByFriendSteamId)
            {
                var friendSteamId = keyValuePair.Key;
                var mutuallyOwnedAppIds = keyValuePair.Value;
                var mutuallyOwnedAppIdsWithAchievements = mutuallyOwnedAppIds.Except(Calculations.AppIdsWithoutAchievements).ToHashSet();

                achievementsByAppIdByFriendSteamId.Add(
                    friendSteamId,
                    await GetAchievementsByAppIdForGivenSteamIdAndAppIds(mutuallyOwnedAppIdsWithAchievements, friendSteamId)
                );

                ProgressBarManager.IncrementValue("Default");
            }

            Calculations.AchievementsByAppIdByFriendSteamId = achievementsByAppIdByFriendSteamId;
        }


        private static async Task SetCapsuleAndTagsByAppIdAsync()
        {
            await SetCapsuleImagePathAndGenresFromStoreAPIAsync();

            ProgressBarManager.IncrementMaximum("Default", Calculations.PlayerGamesOwnedNameByAppId.Count);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving game tags from store pages");

            foreach (var appId in Calculations.PlayerGamesOwnedNameByAppId.Keys)
            {
                if (Calculations.AppIdsWithoutAchievements.Contains(appId))
                {
                    ProgressBarManager.IncrementValue("Default");
                    continue;
                }

                if (Calculations.CapsuleAndTagsByAppId.ContainsKey(appId))
                {
                    await Calculations.UpdateTagsWithHTMLofSteamStorePageAsync(appId);
                }


                foreach (var keyValuePair in Calculations.MutuallyOwnedAppIdsByFriendSteamId)
                {
                    var friendSteamId = keyValuePair.Key;
                    var mutuallyOwnedAppIds = keyValuePair.Value;

                    if (mutuallyOwnedAppIds.Contains(appId) && Calculations.CapsuleAndTagsByAppId.TryGetValue(appId, out var capsuleAndTags))
                    {
                        foreach (var tag in capsuleAndTags.Item2)
                        {
                            if (Calculations.MutuallyOwnedGameNamesByTagByFriendSteamId.TryGetValue(friendSteamId, out var mutuallyOwnedGameNamesByTag))
                            {
                                if (mutuallyOwnedGameNamesByTag.TryGetValue(tag, out var mutuallyOwnedGameNames))
                                {
                                    mutuallyOwnedGameNames.Add(Calculations.PlayerGamesOwnedNameByAppId[appId]);
                                    Calculations.MutuallyOwnedCapsuleImagesByTagByFriendSteamId[friendSteamId][tag].Add(
                                        Calculations.CapsuleAndTagsByAppId[appId].Item1
                                    );
                                }
                                else
                                {
                                    mutuallyOwnedGameNamesByTag.Add(tag, new HashSet<string> { Calculations.PlayerGamesOwnedNameByAppId[appId] });
                                    Calculations.MutuallyOwnedCapsuleImagesByTagByFriendSteamId[friendSteamId].Add(
                                        tag, new HashSet<string> { Calculations.CapsuleAndTagsByAppId[appId].Item1 }
                                    );
                                }
                            }
                            else
                            {
                                Calculations.MutuallyOwnedGameNamesByTagByFriendSteamId.Add(
                                    friendSteamId,
                                    new Dictionary<string, HashSet<string>>
                                    {
                                        { tag, new HashSet<string> { Calculations.PlayerGamesOwnedNameByAppId[appId] } }
                                    }
                                );
                                Calculations.MutuallyOwnedCapsuleImagesByTagByFriendSteamId.Add(
                                    friendSteamId,
                                    new Dictionary<string, HashSet<string>>
                                    {
                                        { tag, new HashSet<string> { Calculations.CapsuleAndTagsByAppId[appId].Item1 } }
                                    }
                                );
                            }
                        }
                    }
                }

                ProgressBarManager.IncrementValue("Default");
            }
        }


        private static async Task SetCapsuleImagePathAndGenresFromStoreAPIAsync()
        {
            ProgressBarManager.IncrementMaximum("Default", Calculations.PlayerGamesOwnedNameByAppId.Count);
            ProgressBarManager.SetLabelContentTask("Default", "Retrieving game genres and capsule images from store API");

            foreach (var appId in Calculations.PlayerGamesOwnedNameByAppId.Keys)
            {
                if (Calculations.AppIdsWithoutAchievements.Contains(appId))
                {
                    ProgressBarManager.IncrementValue("Default");
                    continue;
                }

                var jSONAppDetails = await GetAppDetailsJSONFromSteamStoreAPIAsync(appId);

                var appDetailsQueryReturnedNoResults = "{\"" + appId + "\":{\"success\":false}}";
                if (jSONAppDetails.Left(appDetailsQueryReturnedNoResults.Length).Equals(appDetailsQueryReturnedNoResults))
                {
                    // Some games like Deus Ex: Human Revolution that have been replaced with later editions still return results
                    // but their results are empty/blank
                    ProgressBarManager.IncrementValue("Default");
                    continue;
                }

                var capsule = await GetCapsuleImagePathFromStoreAPIJSONAsync(jSONAppDetails);

                var genres = new HashSet<string>();
                if (jSONAppDetails.Contains("\"genres\":["))
                {
                    // Some games like Middle Earth: Shadow of Mordor return results but are missing genres tags for no
                    // apparent reason
                    genres = GetGenresFromStoreAPIJSON(jSONAppDetails);
                }

                Calculations.CapsuleAndTagsByAppId.Add(appId, (capsule, genres));
                ProgressBarManager.IncrementValue("Default");
            }
        }


        private static async Task<string?> GetAppDetailsJSONFromSteamStoreAPIAsync(string appId)
        {
            bool tryAgain;
            string? jSONAppDetails = null;
            do
            {
                tryAgain = false;
                using var httpClient = new HttpClient();
                try
                {
                    jSONAppDetails = await httpClient.GetStringAsync("https://store.steampowered.com/api/appdetails?appids=" + appId);
                }
                catch (HttpRequestException exception)
                {
                    switch (exception.Message)
                    {
                        case "Response status code does not indicate success: 302 (Moved Temporarily).":
                            // Game no longer has a store page, e.g. Blur
                            return null;
                        case "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (store.steampowered.com:443)":
                            // Too many requests being sent too quickly to connect
                            await Task.Delay(1000);
                            tryAgain = true;
                            break;
                        case "Response status code does not indicate success: 429 (Too Many Requests).":
                            // Rate limit exceeded
                            ProgressBarManager.SetLabelContentTask("Default", "Waiting for more Steam store API requests to become available");
                            await Task.Delay(60000);
                            tryAgain = true;
                            ProgressBarManager.SetLabelContentTask("Default", "Retrieving game genres and capsule images from store API");
                            break;
                        default:
                            throw;
                    }
                }
            } while (tryAgain);

            return jSONAppDetails;
        }


        private static async Task<string> GetCapsuleImagePathFromStoreAPIJSONAsync(string jSONAppDetails)
        {
            var startPosition = jSONAppDetails.IndexOf(':', jSONAppDetails.IndexOf("\"capsule_imagev5\":")) + 2;
            var endPosition = jSONAppDetails.IndexOf(".jpg", startPosition) + 4;
            var pathWeb = jSONAppDetails.Mid(startPosition, endPosition - startPosition).Replace("\\", string.Empty);

            var streamCapsuleImage = await GetStreamOfCapsuleImageFromWebAsync(pathWeb);
            var pathDisk = Path.GetTempFileName();
            using var fileStream = File.Create(pathDisk);
            streamCapsuleImage.CopyTo(fileStream);

            return pathDisk;
        }


        private static async Task<Stream> GetStreamOfCapsuleImageFromWebAsync(string pathWeb)
        {
            bool tryAgain;
            var streamCapsuleImage = Stream.Null;
            do
            {
                tryAgain = false;
                using var httpClient = new HttpClient();
                try
                {
                    streamCapsuleImage = await httpClient.GetStreamAsync(pathWeb);
                }
                catch (HttpRequestException exception)
                {
                    switch (exception.Message)
                    {
                        case "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (shared.akamai.steamstatic.com:443)":
                            // Too many requests, slow down
                            await Task.Delay(1000);
                            tryAgain = true;
                            break;
                        case "Response status code does not indicate success: 502 (Bad Gateway).":
                        case "The SSL connection could not be established, see inner exception.":
                            tryAgain = true;
                            break;
                        default:
                            throw;
                    }
                }
            } while (tryAgain);

            return streamCapsuleImage;
        }


        private static HashSet<string> GetGenresFromStoreAPIJSON(string jSONAppDetails)
        {
            var startPosition = jSONAppDetails.IndexOf('[', jSONAppDetails.IndexOf("\"genres\":["));
            var endPosition = jSONAppDetails.IndexOf(']', startPosition);
            var genresBlock = jSONAppDetails.Mid(startPosition, endPosition - startPosition);

            var genres = new HashSet<string>();
            var allGenresIndicatorPositions = genresBlock.GetAllIndicesOfSubstring("\"description\":\"");
            foreach (var genresIndicatorPosition in allGenresIndicatorPositions)
            {
                startPosition = genresIndicatorPosition + "\"description\":\"".Length;
                endPosition = genresBlock.IndexOf('\"', startPosition);
                genres.Add(genresBlock.Mid(startPosition, endPosition - startPosition));
            }

            return genres;
        }


        private static async Task UpdateTagsWithHTMLofSteamStorePageAsync(string appId)
        {
            var pageHtml = await Calculations.GetHTMLofSteamStorePageAsync(appId);

            if (!pageHtml.Contains("\"tagid\":") && !pageHtml.Contains("\"tags\":["))
            {
                // Age gate blocks scraping, e.g. Just Cause, Quake
                return;
            }

            var tags = Calculations.GetTagsFromHTMLOfSteamStorePage(pageHtml);

            var capsule = Calculations.CapsuleAndTagsByAppId[appId].Item1;
            Calculations.CapsuleAndTagsByAppId.Remove(appId);
            Calculations.CapsuleAndTagsByAppId.Add(appId, (capsule, tags));
        }


        private static async Task<string?> GetHTMLofSteamStorePageAsync(string appId)
        {
            bool tryAgain;
            string? pageHtml = null;
            do
            {
                tryAgain = false;
                using var httpClient = new HttpClient();
                try
                {
                    pageHtml = await httpClient.GetStringAsync("https://store.steampowered.com/app/" + appId + "/");
                }
                catch (HttpRequestException exception)
                {
                    switch (exception.Message)
                    {
                        case "Response status code does not indicate success: 302 (Moved Temporarily).":
                            // Game no longer has a store page, e.g. Blur
                            return null;
                        case "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. (store.steampowered.com:443)":
                            // Too many requests, slow down
                            await Task.Delay(1000);
                            tryAgain = true;
                            break;
                        default:
                            throw;
                    }
                }
            } while (tryAgain);

            return pageHtml;
        }


        private static HashSet<string> GetTagsFromHTMLOfSteamStorePage(string pageHtml)
        {
            var tags = new HashSet<string>();
            // In general, games that have news posts use "tagid" for their game tags and "tags" for their news posts.
            // However, older games use "tags" for their game tags.
            var startPositionIndicator = pageHtml.IndexOf("\"tagid\":");
            if (startPositionIndicator != -1)
            {
                var endPosition = pageHtml.IndexOf("</script>", startPositionIndicator);
                var tagBlock = pageHtml.Mid(startPositionIndicator, endPosition - startPositionIndicator);
                var startPositionIndicators = tagBlock.GetAllIndicesOfSubstring("\"name\":");
                foreach (var namePositionIndicator in startPositionIndicators)
                {
                    var startPosition = tagBlock.IndexOf(':', namePositionIndicator) + 2;
                    endPosition = tagBlock.IndexOf('\"', startPosition);
                    tags.Add(tagBlock.Mid(startPosition, endPosition - startPosition));
                }

                return tags;
            }
            else
            {
                startPositionIndicator = pageHtml.IndexOf("\"tags\":[");
                var startPosition = pageHtml.IndexOf('[', startPositionIndicator) + 2;
                var endPosition = pageHtml.IndexOf(']', startPosition) - 1;
                tags = pageHtml.Mid(startPosition, endPosition - startPosition).Split("\",\"").ToHashSet();

                return tags;
            }
        }


        private static void SetPlayerPercentageByMututallyOwnedTagByFriendSteamId()
        {
            ProgressBarManager.IncrementMaximum("Default", Calculations.SteamNamesAndAvatarsByFriendSteamId.Count);
            ProgressBarManager.SetLabelContentTask("Default", "Calculating player % of achievements by friend by tag");

            foreach (var friendSteamId in Calculations.SteamNamesAndAvatarsByFriendSteamId.Keys)
            {
                var achievementPlayerAndFriendCountsByTag = new Dictionary<string, (int, int)>();

                foreach (var appId in Calculations.MutuallyOwnedAppIdsByFriendSteamId[friendSteamId])
                {
                    if (!Calculations.CapsuleAndTagsByAppId.ContainsKey(appId))
                    {
                        continue;
                    }

                    var tags = Calculations.CapsuleAndTagsByAppId[appId].Item2;

                    foreach (var tag in tags)
                    {
                        if (achievementPlayerAndFriendCountsByTag.TryGetValue(tag, out var achievementPlayerAndFriendCounts))
                        {
                            (var playerAchievements, var friendAchievements) = achievementPlayerAndFriendCounts;
                            playerAchievements += Calculations.PlayerAchievementsByAppId[appId];
                            friendAchievements += Calculations.AchievementsByAppIdByFriendSteamId[friendSteamId][appId];
                            achievementPlayerAndFriendCountsByTag[tag] = (playerAchievements, friendAchievements);
                        }
                        else
                        {
                            achievementPlayerAndFriendCountsByTag.Add(
                                tag, (PlayerAchievementsByAppId[appId], AchievementsByAppIdByFriendSteamId[friendSteamId][appId])
                            );
                        }
                    }
                }

                var playerPercentageByMututallyOwnedTag = new Dictionary<string, int>();
                foreach (var tag in achievementPlayerAndFriendCountsByTag.Keys)
                {
                    var playerAchievementCountByTag = achievementPlayerAndFriendCountsByTag[tag].Item1;
                    var friendAchievementCountByTag = achievementPlayerAndFriendCountsByTag[tag].Item2;

                    var playerPercentage = (int)(
                        playerAchievementCountByTag /
                        (playerAchievementCountByTag + (double)friendAchievementCountByTag) * 100
                    );

                    playerPercentageByMututallyOwnedTag.Add(tag, playerPercentage);
                }

                var orderedPlayerPercentageByMututallyOwnedTagEnumberable = playerPercentageByMututallyOwnedTag.OrderByDescending(x => x.Value);
                var orderedPlayerPercentageByMututallyOwnedTag = new OrderedDictionary<string, int>(orderedPlayerPercentageByMututallyOwnedTagEnumberable);

                Calculations.PlayerPercentageByMututallyOwnedTagByFriendSteamId.Add(friendSteamId, orderedPlayerPercentageByMututallyOwnedTag);

                ProgressBarManager.IncrementValue("Default");
            }
        }


        private static void PrepareToDisplayResults(ViewModel viewModel)
        {
            var firstFriendSteamName = string.Empty;

            foreach (var keyValuePair1 in Calculations.PlayerPercentageByMututallyOwnedTagByFriendSteamId)
            {
                var friendSteamId = keyValuePair1.Key;
                var playerPercentageByMututallyOwnedTag = keyValuePair1.Value;

                foreach (var keyValuePair2 in playerPercentageByMututallyOwnedTag)
                {
                    var tag = keyValuePair2.Key;
                    var playerPercentage = keyValuePair2.Value;
                    var mutuallyOwnedGameNamesWithThisTag = Calculations.MutuallyOwnedGameNamesByTagByFriendSteamId[friendSteamId][tag];

                    _ = new ResultsManager(
                        friendSteamId, tag, playerPercentage, mutuallyOwnedGameNamesWithThisTag,
                        Calculations.MutuallyOwnedCapsuleImagesByTagByFriendSteamId[friendSteamId][tag]
                    );
                }

                Calculations.AddSteamNameAndAvatarToViewModel(Calculations.SteamNamesAndAvatarsByFriendSteamId[friendSteamId]);

                if (string.IsNullOrEmpty(firstFriendSteamName))
                {
                    firstFriendSteamName = Calculations.SteamNamesAndAvatarsByFriendSteamId[friendSteamId].Item1;
                }
            }

            Calculations.AddTagResultsBySteamNameToViewModel();
            viewModel.TagResults = ViewModel.TagResultsBySteamName[firstFriendSteamName];
        }


        public static void AddSteamNameAndAvatarToViewModel((string, string) steamNameAndAvatar)
        {
            var imageAvatar = new Image();
            var bitmapImageAvatar = new BitmapImage();
            bitmapImageAvatar.BeginInit();
            bitmapImageAvatar.UriSource = new Uri(steamNameAndAvatar.Item2);
            bitmapImageAvatar.EndInit();
            imageAvatar.Source = bitmapImageAvatar;
            imageAvatar.ToolTip = string.Empty;
            ViewModel.AvatarsBySteamName.Add(new KeyValuePair<string, Image>(steamNameAndAvatar.Item1, imageAvatar));
        }


        public static void AddTagResultsBySteamNameToViewModel()
        {
            foreach (var keyValuePair in Calculations.SteamNamesAndAvatarsByFriendSteamId)
            {
                var friendSteamId = keyValuePair.Key;
                var steamName = keyValuePair.Value.Item1;

                ViewModel.TagResultsBySteamName.Add(steamName, ResultsManager.TagResultsByFriendSteamId[friendSteamId]);
            }
        }
    }
}
