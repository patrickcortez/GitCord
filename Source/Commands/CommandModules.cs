using Gitbot2.Source.Core;
using Gitbot2.Source.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCord.Rest;
using NetCord.Services;
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

        [SlashCommand("help","get acquianted with all the GitCord commands")]
        public string Help()
        {
           
            string help = @"
                  Git Commands:
                        switch                  - switch to a repository.
                        current                 - shows current repository.
                        commit <message>        - commit changes with a message.
                        merge <b1>              - merge current branch with chosen branch.
                        checkout <br>           - checksout branch.
                        branches                - list all the branches.
                        status                  - get the status of the repository.
                  
                  Repo-List Commands:
                        list                    - list all repos in the list.
                        pop <repo>              - delete a repo from the list.
                        add <repo>              - add a repo to the list.
                    
                  Task Commands:
                        add                     - Adds a task
                        pop                     - Crosses out a Task
                        list                    - Lists all tasks

                  Other Commands:
                        ignore                  - Ignores swears and offensive terminologies.
                        current_exception       - Displays the most recent exception.
                        help                    - displays this message.
                ";

            return help;
        }

        [SlashCommand("current_exception","Shows last known Exception")]
        public string GetExc()
        {
            Exception ex = RepoCache.GetRecentException();
            string message = $@"
            [{DateTime.Now}]
            
            [Exception Message]
            {ex.Message}
            ------------
            [Stack Trace]
            {ex.StackTrace}
            ------------
            [Source]
            {ex.Source}
            ";

            return message;
        }

        [SlashCommand("auth", "Sets Git username and pat")]
        public async Task<string> Authenticate()
        {
            DMmanager dm = new(Context.User, Context.Client.Rest);
            await dm.SendDM();

            return "DM sent!";
        }

        [SlashCommand("recent_auth","For debugging only, dislays recent auth")] // Will be deleted soon
        public string rec_auth(bool ShowJson)
        {
            try
            {
                if (ShowJson)
                {
                    StringBuilder json = new();
                    using (StreamReader reader = new(RepoCache.authdb))
                    {
                        string? line = "";

                        while ((line = reader.ReadLine()) != null)
                        {
                            json.AppendLine(line);
                        }


                    }

                    if(json.Length < 1)
                    {
                        return "Database is empty";
                    }

                    return json.ToString();
                }


                return $"Recent auth: {RepoCache.GetRecentContent()}";
            }catch(Exception ex)
            {
                RepoCache.SetException(ex);
                return "Failed to show recent auth";
            }
        }

        

    }

    // Git Command Module

    [SlashCommand("git","Perform git operations on the current repository")] // all git repository operations
    public class GitModule : ApplicationCommandModule<ApplicationCommandContext>
    {


        [SubSlashCommand("switch","Switch current repository with one of the repos in the list")]
        public async Task<string> SwitchRepos(string target)
        {
            TaskStatus status = await FSOperations.SwitchRepo(target);
            RepoCache.SetCurrentRepo(target);

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

        [SubSlashCommand("merge","Merge a branch with current branch")]
        public async Task<string> MergeBranch(string branch)
        {
            IOptions<_Roles> roles = Services.CreateProvider().Services.GetService<IOptions<_Roles>>();
            CommandHandler coms = new("merge", Services.CreateProvider().Services.GetService<RestClient>(), ulong.Parse(roles.Value.GenId));

            int exc = await coms.ExecuteCommand();

            if(exc == 0)
            {
                return $"Branch {branch} merged";
            }

            return $"Failed to merge {branch}";
        }

        [SubSlashCommand("clone","Clones repository")]
        public async Task<string> Clone(string url,string filename,bool flag)
        {
            return await FSOperations.GitClone(url,filename,flag);
        }

        [SubSlashCommand("remotes","Lists all remote urls and their names")]
        public string GetRemotes()
        {
           return FSOperations.ListRemotes(RepoCache.GetCurrentRepo());
        }

        [SubSlashCommand("push","Push a commit to the repository")]

        // Find a way to make GitCord useful before adding push and pull
        // Add Import command to import files/folders to the local repository.


    }

    // Repository List Commands

    [SlashCommand("repo","manage your repo-list")]
    public class RepoModule : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("pop","deletes a repository from list")]
        public async Task<string> Pop([SlashCommandParameter(Name = "target",Description = "Name of repository you want to pop")] string target)
        {
            string msg = await FSOperations.PopRepo(target);

            return msg;
        }

        [SubSlashCommand("add","adds a new repository to the list")]
        public async Task<string> Add([SlashCommandParameter(Name ="repo",Description = "A new repository to be added")] string Repo)
        {
            return await FSOperations.AddRepo(Repo);
        }

        [SubSlashCommand("list", "Lists all listed repositories")]
        public string ListRepos()
        {
            StringBuilder sb = new();
            sb.AppendLine("List of Repositories:");

            RepoCache.GetCache().ForEach((c) =>
            {
                sb.AppendLine($"- {c}");
            });

            sb.AppendLine("----------------------------");

            return sb.ToString();
        }
    }

    [SlashCommand("task","A collection of tasks")]
    public class GitTask : ApplicationCommandModule<ApplicationCommandContext>
    {
        /*
         Syntax:
            [ ] Task1
            [*] Task2
         
         */



        [SubSlashCommand("add","Adds a task")]
        public async Task<string> Add(string _Task)
        {
            char completion = ' '; // space for incomplete, '*' for complete
            string fulltask = $"[{completion}]: {_Task}"; // [ ] <Task>

            using (StreamWriter sw = new(RepoCache.taskpath, true))
            {
                 await sw.WriteLineAsync(fulltask);
            }

            return "Task Added";
        }

        [SubSlashCommand("pop", "crosses out a task")]
        public async Task<string> Pop(int index) // need to write a tokenizer -_-
        {
            Tokenizer token = new(RepoCache.taskpath);

            var tokens = await token.GetTokensAsync();

            if(index < 0 || index > tokens.Count)
            {
                return "index cannot be higher/lower than total of tasks";
            }

            var item = tokens.ElementAt(index);
            item.Item1 = true;
            item.Item2 = Utility.ReplaceAt(item.Item2, 1, 1);

            tokens[index] = item;




            List<string> temp = new();

            tokens.ForEach((item) =>
            {
                temp.Add(item.Item2);
            });
  
            await Utility.WritetoFile(RepoCache.taskpath, temp);

            return "Task Popped";

        }

        [SubSlashCommand("list", "lists all tasks")]
        public async Task<string> List()  // Iterate through all lines, store in a list of strings then display using forloop
        {
            StringBuilder list = new();

            Tokenizer token = new(RepoCache.taskpath);

            var tokens = await token.GetTokensAsync();

            List<string> lines = tokens.Select(c => c.Item2).ToList();

            lines.ForEach((item) =>
            {
                list.AppendLine(item.ToString());
            });


            return $"List of Tasks:\n {list.ToString()}";
        }
    }


}
