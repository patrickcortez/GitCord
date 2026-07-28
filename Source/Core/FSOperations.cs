using Gitbot2.Source.Commands;
using Gitbot2.Source.Utils;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gitbot2.Source.Core
{
    

    internal class DiscordConfig
    {

        public string Token { get; set; }
        public string GeneralId { get; set; }

        public string Prefix { get; set; }
        public string Current { get; set; }

    }

    class Conf
    {
        public DiscordConfig Discord { get; set; } = new();
    }
    internal static class FSOperations
    {
        private static ILogger logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger("FSOperations");
        public static async Task<TaskStatus> SwitchRepo(string newRepo)
        {
            try
            {
                if (!Path.Exists(newRepo) || !Path.IsPathRooted(newRepo))
                {
                    logger.LogWarning("Repo {} doesn't exist.", newRepo);
                    return TaskStatus.Canceled;
                }

                string path = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
                Stream read = File.OpenRead(path);

                Conf conf = await JsonSerializer.DeserializeAsync<Conf>(read);

                read.Close();

                if (conf is null)
                {
                    logger.LogWarning("Json is null");
                    return TaskStatus.Canceled;
                }

                conf.Discord.Current = newRepo;

                Stream write = new FileStream(path, FileMode.Create,FileAccess.Write,FileShare.None,4096,FileOptions.Asynchronous);

                await JsonSerializer.SerializeAsync(write, conf, new JsonSerializerOptions() { WriteIndented=true });

                write.Close();

                return TaskStatus.RanToCompletion;

               

            }catch(Exception ex)
            {
                logger.LogError(ex, "Json Serializing/Deserializing Error");
                return TaskStatus.Faulted;
            }

            
        }

        public static string GetCurrent(IConfiguration config)
        {
            string? repo = config.GetValue<string>("Discord:Current");
            

            return (!string.IsNullOrEmpty(repo)) ? repo : string.Empty ;
        }

        public static string CommitRepo(IConfiguration config,string msg)
        {
            try
            {
                string? current = config.GetValue<string>("Discord:Current");

                Repository repo = new(current);
                var repostat = repo.RetrieveStatus();

                if (repostat.Untracked.Count() > 0 || repostat.Added.Count() > 0)
                {
                    if (repostat.Untracked.Count() > 0)
                    {
                        LibGit2Sharp.Commands.Stage(repo, "*");
                    }

                    var name = repo.Config.Get<string>("user.name");
                    var email = repo.Config.Get<string>("user.email");

                    repo.Commit(msg, new(name.Value, email.Value, DateTime.Now), new(name.Value, email.Value, DateTime.Now));

                    logger.LogInformation("Commited: {} , {} , {}", msg, name.Value, email.Value);

                    return "Repo Committed!";
                }

                return "Nothing to commit";
            }catch(Exception ex)
            {
                logger.LogError(ex, "Something Went wrong while committing");
                return "Something Went wrong while committing";
            }
        }

        public static string RepoStatus(IConfiguration config)
        {
            string? path = config.GetValue<string>("Discord:Current");

            Repository repo = new(path);

           var stat =  repo.RetrieveStatus();

            int ut =  stat.Untracked.Count(),added = stat.Added.Count();

            return (ut > 0 || added > 0)? $" Untracked Files: {ut}  Added: {added}" : "On a Clean Branch, Nothing to commit";

        }


        public static string Branches(IConfiguration config)
        {
            string? path = config.GetValue<string>("Discord:Current");

            Repository repo = new(path);

            var branches = repo.Branches;

            return  $" List of branches: {string.Join('\n',branches.Select(c => (repo.Head.FriendlyName == c.FriendlyName)? "*"+c.FriendlyName : c.FriendlyName ))}";

        }

        public static async Task<string> PopRepo(string target)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "repos.json");

            if (!RepoCache.PopElement(target))
            {
                return $"Failed to pop {target}";
            }

            bool success = await RepoCache.SaveCacheToFile();



            return (success)? $"Repository {target} has been popped" : "Failed to save to file";

        }

        public static async Task<string> AddRepo(string path)
        {
            RepoCache.AddElement(path);

            bool result = await RepoCache.SaveCacheToFile();

            return (result) ? "Repo Added!" : "Repo Failed to Add";
        }


        public static async Task<string> GitClone(string url,bool flag)
        {
            try
            {


                string path = Repository.Clone(url, RepoCache.workingdir);

                if (flag == true)
                {
                    RepoCache.AddElement(path);
                    await RepoCache.SaveCacheToFile();
                    return $"Clone successful at {path}, and added to list";
                }

                return $"Clone Successful at {path}";
            }catch(Exception ex)
            {
                logger.LogError(ex, "Something Went wrong while cloning");
                return "Clone failed";
            }
        }
        public static string Checkout(IConfiguration config, string branch)
        {
            try
            {
                string? path = config.GetValue<string>("Discord:Current");

                Repository repo = new(path);

                Branch? target = repo.Branches[branch]; // not null

                if(target is null) // null guard clause
                {
                  target =  repo.CreateBranch(branch);
                }

                Branch checkout = LibGit2Sharp.Commands.Checkout(repo, target);

                if (checkout is not null)
                {
                    return $"Checked out to {checkout.FriendlyName}";
                }

                return $"Failed to checkout to {branch}";
            }catch(Exception ex)
            {
                logger.LogError(ex, "Something went wrong while checking out");
                return $"Failed to Checkout {branch}";
            }
        }

    }
}
