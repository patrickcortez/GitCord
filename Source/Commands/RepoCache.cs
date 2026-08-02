using Gitbot2.Source.Utils;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using NetCord;

using DataEntry = (string uname,string pat,string email,string username); // Declare our data alias

namespace Gitbot2.Source.Commands
{
    internal static class RepoCache
    {
        private static List<string> Cache;
        private static List<User> requests;
        private static string filepath;
        private static string currentRepo;

        private static DataEntry ResponseContent;

        public static string taskpath { get;private set; }
        public static string workingdir { get; } = Path.Combine(Environment.CurrentDirectory, "Repos");
        public static string authdb { get; } = Path.Combine(Environment.CurrentDirectory, "authdb.json");
        private static ILogger logger;
        private static Exception CurrentException;
        private static Auths userAuths;
        

        public static void SetCache(List<string> Primary,string FullPath = "") // Initializer Method
        {
            Cache = new(Primary);
            filepath = FullPath;
            logger = Services.CreateProvider().Services.GetService<ILogger>();
            taskpath = Path.Combine(Environment.CurrentDirectory, "tasks.txt");
            

            if (!Path.Exists(taskpath))
            {
                File.Create(taskpath);
                logger.LogInformation("Created task list at {}", taskpath);
            }

            if (!Path.Exists(workingdir))
            {
                Directory.CreateDirectory(workingdir);
                logger.LogInformation("Created working directory made at {}", taskpath);
            }

            if (!Path.Exists(authdb))
            {
                File.Create(authdb);
                logger.LogInformation("Auth db made at {}", authdb);
            }
        }

        public static void SetContent(DataEntry content)
        {
            ResponseContent = content;
        }

        public static void SetAuths(Auths newAuths)
        {
            userAuths = newAuths;
        }

        public static Auth? GetAuthByUser(string key)
        {
            Auth target = new();
            userAuths.auths.ForEach((item) => { if (item.Username == key) { target = item; } });

            if (target.PAT is null || target.PAT == string.Empty)
            {
                return null;
            }

            return target;
        }

        public static Auths GetAuths()
        {
            return userAuths;
        }

        public static Auth GetAuth(int index)
        {
            return userAuths.auths[index];
        }

        public static DataEntry GetRecentContent()
        {
            return ResponseContent;
        }
        public static void SetRequests()
        {
            requests = new();
        }

        public static bool isInRequests(User user)
        {
            return requests.Contains(user);
        }

        public static void AddRequest(User user)
        {
            requests.Add(user);
        }

        public static void PopRequest(User user)
        {
            requests.Remove(user);
        }

        public static void ClearRequests()
        {
            requests.Clear();
        }

        public static void SetException(Exception exception)
        {
            CurrentException = exception;
        }

        public static void SetCurrentRepo(string path)
        {
            currentRepo = path;
        }

       


        public static Exception GetRecentException()
        {
            return CurrentException;
        }

        public static Repository GetCurrentRepo()
        {
            return new(currentRepo);
        }
        

        public static string GetValue(int index) // Unused for now
        {
            return (index >= 0 && index < Cache.Count) ? Cache[index] : string.Empty;

        }


        public static bool ElementExists(string Element) // Unused for now
        {
            return Cache.Contains(Element);
        }

        public static List<string>? GetCache()
        {
            return (Cache is not null && Cache.Count > 0)? Cache : null;
        }

        public static bool PopElement(string Element)
        {
            return Cache.Remove(Element);
        }

        public static void AddElement(string Element)
        {

            if (!Path.IsPathRooted(Element))
            {
                logger.LogWarning("Path: {} has to be rooted", Element);
                return;
            }

            if (!Path.Exists(Element))
            {
                logger.LogWarning("Path: {} does not exist", Element);
                return;
            }



            Cache.Add(Element);
        }

        public static async Task<bool> SaveCacheToFile()
        {
            try
            {
                string content = await File.ReadAllTextAsync(filepath);
                FileStream fs = new(filepath,FileMode.Create, FileAccess.Write, FileShare.Write);
                

                Repositories repos = JsonSerializer.Deserialize<Repositories>(content);

                if(repos is null || (Cache is null || Cache.Count < 1))
                {
                    logger.LogWarning("repos instance is null, terminating..");
                    return false;
                }

                repos.Repos = Cache;

                await JsonSerializer.SerializeAsync<Repositories>(fs, repos);

                return true;
            }catch(Exception ex)
            {
                logger.LogError(ex, "Something went wrong while writing to {}", filepath);
                return false;
            }
        }
    }
}
