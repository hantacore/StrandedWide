using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace StrandedDeepModsUtils
{
    public class VersionChecker
    {
        internal class InfoJson
        {
            public string Id { get; set; }
            public string Version { get; set; }
        }

        public class Repository
        {
            [Serializable]
            public class Release
            {
                public string Id;
                public string Version;
                public string DownloadUrl;
                public string Homepage;
            }

            public Release[] Releases;
        }

        internal static void CheckVersion(UnityModManager.ModEntry currentMod, string infoJsLocation)
        {
            try
            {
                Debug.Log(currentMod.Info.DisplayName + " : checking for updates");
                HttpWebRequest webRequest = null;
                string strContent = "";

                if (!String.IsNullOrEmpty(infoJsLocation))
                {
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(infoJsLocation);
                    webRequest.UserAgent = "StrandedDeepMod";

                    using (var response = webRequest.GetResponse())
                    {
                        using (var content = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(content))
                            {
                                strContent = reader.ReadToEnd();
                            }
                        }
                    }
                    InfoJson info = TinyJson.JSONParser.FromJson<InfoJson>(strContent);
                    Debug.Log(currentMod.Info.DisplayName + " : CheckVersion : " + info.Version);

                    Version remoteVersion = Version.Parse(info.Version.Replace("-beta", ""));

                    if (remoteVersion > currentMod.Version)
                    {
                        currentMod.CustomRequirements = "<color=orange>Update available (" + info.Version + ")</color>";
                    }
                    else
                    {
                        currentMod.CustomRequirements = "<color=green>Up to date</color>";
                    }
                }

                // hack to avoid checking the repository for each mod
                if (!currentMod.HasUpdate
                    && !String.IsNullOrEmpty(currentMod.Info.Repository))
                {
                    Debug.Log(currentMod.Info.DisplayName + " : Repository : " + currentMod.Info.Repository);
                    webRequest = (HttpWebRequest)HttpWebRequest.Create(currentMod.Info.Repository);
                    webRequest.UserAgent = "StrandedDeepMod";
                    using (var response = webRequest.GetResponse())
                    {
                        using (var content = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(content))
                            {
                                strContent = reader.ReadToEnd();
                            }
                        }
                    }
                    Repository repinfo = TinyJson.JSONParser.FromJson<Repository>(strContent);
                    if (repinfo == null)
                    {
                        Debug.Log(currentMod.Info.DisplayName + " : could not parse repository");
                    }
                    else
                    {
                        foreach (Repository.Release release in repinfo.Releases)
                        {
                            try
                            {
                                //Debug.Log(currentMod.Info.DisplayName + " : looking for " + release.Id);
                                UnityModManager.ModEntry modEntry = UnityModManager.FindMod(release.Id);
                                if (modEntry != null)
                                {
                                    // hack to avoid checking the repository for each mod
                                    modEntry.HasUpdate = true;

                                    Debug.Log(currentMod.Info.DisplayName + " : repository advertised mod version  " + release.Id + " / " + release.Version);
                                    if (string.IsNullOrEmpty(modEntry.CustomRequirements))
                                    {
                                        Version remoteVersionRepo = Version.Parse(release.Version.Replace("-beta", ""));
                                        //Debug.Log(currentMod.Info.DisplayName + " : remoteVersionRepo " + remoteVersionRepo);
                                        Version localVersion = Version.Parse(modEntry.Info.Version.Replace("-beta", ""));
                                        //Debug.Log(currentMod.Info.DisplayName + " : localVersion " + localVersion);
                                        if (remoteVersionRepo > localVersion)
                                        {
                                            modEntry.CustomRequirements = "<color=orange>Update available (" + release.Version + ")</color>";
                                        }
                                        else
                                        {
                                            modEntry.CustomRequirements = "<color=green>Up to date</color>";
                                        }
                                    }
                                    if (string.IsNullOrEmpty(modEntry.Info.HomePage)
                                        && !string.IsNullOrEmpty(release.Homepage))
                                    {
                                        modEntry.Info.HomePage = release.Homepage;
                                    }
                                }
                                else
                                {
                                    Debug.Log(currentMod.Info.DisplayName + " : " + release.Id + " not installed");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Log(currentMod.Info.DisplayName + " : error while handling modEntry : " + ex);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                currentMod.CustomRequirements = "Could not check for updates";
                Debug.Log(currentMod.Info.DisplayName + " : error on CheckVersion : " + e);
            }
        }
    }
}
