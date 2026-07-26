using Gitbot2.Source.Core;
using Gitbot2.Source.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace Gitbot2.Source.Commands
{

    // Utilities
    public class CommandModules : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("ping","Get a message of pong!")] // for fun, and first slash command I've ever added
        public Task<string> Pong() => Task.FromResult("Pong!");

        [SlashCommand("ignore", "Enable/Disable to Parse messages")]
        public string Ignore(bool result) // turn off/on global state machine for parsing messages
        {
                if (result)
                {
                    MessageToggle.Ignore = true;
                    return "Ignoring Messages";
                }
                else
                {
                    MessageToggle.Ignore = false;
                    return "Parsing Messages";
                }
        }

    }

    // Git Command Module

    [SlashCommand("git","Perform git operations on the current repository")] // all git repository operations
    public class GitModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("list","Lists all listed repositories")]
        public string ListRepos()
        {
            StringBuilder sb = new();
            sb.AppendLine("List of Repositories:");

            FileSystem.GetRepositories().ToList().ForEach((c) =>
            {
                sb.AppendLine($"- {c}");
            });

            sb.AppendLine("----------------------------");

            return sb.ToString();
        }

        [SubSlashCommand("switch","Switch current repository with one of the repos in the list")]
        public async Task<string> SwitchRepos(string target)
        {
            TaskStatus status = await FSOperations.SwitchRepo(target);

            if(status == TaskStatus.RanToCompletion)
            {
                return $"Current Repository switched to {target} ";
            }
            else
            {
                return "Failed to switch repository";
            }
        }

        [SubSlashCommand("current","Gets current repository")]
        public string CurrentRepo()
        {
            string current = FSOperations.GetCurrent(Services.CreateProvider().Services.GetService<IConfiguration>());

            return (current != string.Empty)? $"Current repository: {current}" : "Failed to get current repository";
        }

        [SubSlashCommand("status","Get the status of the repository")]
        public string Status()
        {
            string status = FSOperations.RepoStatus(Services.CreateProvider().Services.GetService<IConfiguration>());


            return status;
        }

        [SubSlashCommand("commit","Commit changes of the current repository")]
        public string CommitRepo(string msg)
        {
            string commit = FSOperations.CommitRepo(Services.CreateProvider().Services.GetService<IConfiguration>(), msg);

            return commit;
        }


        [SubSlashCommand("branches","list all the branches in the repository")]

        public string Branches()
        {
           string branches = FSOperations.Branches(Services.CreateProvider().Services.GetService<IConfiguration>());

            return branches;
        }

        [SubSlashCommand("checkout","checkout a branch of the current repository")]
        public string CheckoutBranch(string branch)
        {
            string msg = FSOperations.Checkout(config: Services.CreateProvider().Services.GetService<IConfiguration>(),branch: branch);

            return msg;
        }
        



    }


}
