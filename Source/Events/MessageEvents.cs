using Gitbot2.Source.Commands;
using Gitbot2.Source.Core;
using Gitbot2.Source.Utils;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Gitbot2.Source.Events
{
    public class MessageCreateHandler(ILogger<MessageCreateHandler> logger,RestClient client) : IMessageCreateGatewayHandler
    {
        private CommandHandler comm;

        List<string> BadWords = new()
        {

            "bitch","ghandi","nigger","hitler","fuck","shit","negro","skibidi","sex","handjob","blowjob",
            "bj","footjob","porn","rimjob","negroes","fucks"
        };

        public async ValueTask HandleAsync(Message message)
        {
            try
            {

                if (message.Author.IsBot || MessageToggle.Ignore)
                {
                    return;
                }

                _Roles role = Services.CreateProvider().Services.GetService<IOptions<_Roles>>().Value;

                if(role.IllegalWords.Count() > 0)
                {
                    BadWords.AddRange(role.IllegalWords); // Add all extra illegal words
                    logger.LogInformation("Illegal Words added {}", role.IllegalWords);
                }

                string msg = message.Content;

                if (BadWords.Any(c => msg.Contains(c, StringComparison.OrdinalIgnoreCase))) {

                    await client.DeleteMessageAsync(message.ChannelId, message.Id);
                    logger.LogWarning("Illegal message seized: \"{}\"", msg);
                    return;

                }

                // legacy code, everything moved to CommandModule

                //if (MessageToggle.Ignore)
                //{
                //    logger.LogInformation("Messages has been set to ignore");
                    
                //    return;
                //}

                //if (message.Author.IsBot)
                //{
                //    return;
                //}

                //logger.LogInformation("Current User: {}", message.Author.Username);


                //RoleStatus status = await Utility.isAllowed(client, message);

                //// add '/' prefix to commands, writing /ignore is annoying. 

                //if (status == RoleStatus.NotAllowed)
                //{

                //    await client.SendMessageAsync(message.ChannelId, $"{message.Author.Username} is not permitted");
                //    return;
                //} else if (status == RoleStatus.Error)
                //{
                //    await client.SendMessageAsync(message.ChannelId, "Failed to get user role,discontinuing"); // discontinue in an event of an error for safety
                //    return;
                //}

                

                //logger.LogInformation($"{message.Author.Username} {message.Content}");

                //string content = message.Content;

                ////if (content.EndsWith("/ignore", StringComparison.OrdinalIgnoreCase))
                ////{
                ////    logger.LogInformation("Ignoring {}", content);
                ////    return;
                ////}

                //if (content.Equals("help", StringComparison.OrdinalIgnoreCase))
                //{
                //    string help = @"
                //    Commands:
                //        list                    - lists all repositories
                //        switch                  - switch to a repository
                //        current                 - shows current repository
                //        commit <message>        - commit changes with a message
                //        merge <b1>              - merge current branch with chosen branch
                //        del <repo>              - deletes a repo
                //        checkout <br>           - checksout branch
                //        branches                - list all the branches
                //        status                  - get the status of the repository

                //    Flags:
                //        /ignore                 - ignores chat,
                //";

                //    await client.SendMessageAsync(message.ChannelId, help);
                //}
                //else if (content.Equals("get-users", StringComparison.OrdinalIgnoreCase))
                //{
                //    await client.SendMessageAsync(message.ChannelId, $"Users Joined: {UserJoinedHandler.GetUsers()}");
                //}
                //else if (content.Equals("hi", StringComparison.OrdinalIgnoreCase) || content.Equals("hello", StringComparison.OrdinalIgnoreCase))
                //{
                //    await client.SendMessageAsync(message.ChannelId, $"Hey {message.Author.Username}!");

                //}
                //else if (content.Equals("list", StringComparison.OrdinalIgnoreCase))
                //{
                //    string[] repositories = FileSystem.GetRepositories();

                //    string msg = (repositories != null) ? string.Join('\n', repositories).ToString() : string.Empty;

                //    comm = new(content, client, message.ChannelId);

                //    int success = await comm.ExecuteCommand();

                //    if (success != 0)
                //    {
                //        await client.SendMessageAsync(message.ChannelId, "Something went wrong, try again later");
                //    }

                //    logger.LogInformation("Command exited with {}", success);
                //}
                //else if (content.Equals("shutdown", StringComparison.OrdinalIgnoreCase))
                //{
                //    await client.SendMessageAsync(message.ChannelId, "Good bye!");
                //    Environment.Exit(0);
                //} else if (content.Equals("current", StringComparison.OrdinalIgnoreCase))
                //{
                //    await client.SendMessageAsync(message.ChannelId, $"Current Repository: {FSOperations.GetCurrent(config: Services.CreateProvider().Services.GetService<IConfiguration>())}");
                //} else if (content.StartsWith("switch", StringComparison.OrdinalIgnoreCase))
                //{
                //    if (content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Count() < 2)
                //    {
                //        await client.SendMessageAsync(message.ChannelId, "missing repo name!");

                //        return;
                //    }

                //    string repoName = content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ElementAt(1);



                //    if (!FileSystem.GetRepositories().Contains(Path.Combine(repoName)))
                //    {
                //        await client.SendMessageAsync(message.ChannelId, "Invalid repository, run `list` to see all listed repositories");
                //        return;
                //    }

                //    if (FSOperations.SwitchRepo(repoName).Result == TaskStatus.RanToCompletion)
                //    {
                //        await client.SendMessageAsync(message.ChannelId, $"Repo Switched to {repoName}");


                //    }
                //    else if (FSOperations.SwitchRepo(repoName).Result == TaskStatus.Canceled)
                //    {
                //        await client.SendMessageAsync(message.Id, $"Failed to Switch Repo");
                //    } else if (FSOperations.SwitchRepo(repoName).Result == TaskStatus.Faulted)
                //    {
                //        await client.SendMessageAsync(message.ChannelId,"Something Went wrong...");
                //    }
                //} else if (content.Equals("status", StringComparison.OrdinalIgnoreCase))
                //{
                //   string response = FSOperations.RepoStatus(Services.CreateProvider().Services.GetService<IConfiguration>());

                //    await client.SendMessageAsync(message.ChannelId, response);
                //} else if (content.StartsWith("commit", StringComparison.OrdinalIgnoreCase))
                //{
                //    Func<string, string[]> Tokenize = (line) =>
                //    {
                //        StringBuilder sb = new();
                //        List<string> tokens = new();
                //        bool inqoutes = false;

                //        foreach (char c in line)
                //        {

                //            if (c == '"')
                //            {
                //                inqoutes = !inqoutes;
                //                continue;
                //            }

                //            if(char.IsWhiteSpace(c) && !inqoutes)
                //            {
                //                tokens.Add(sb.ToString());
                //                sb.Clear();
                //                continue;
                //            }

                //            sb.Append(c);

                //        }

                //        if(sb.Length > 0)
                //        {
                //            tokens.Add(sb.ToString());
                //        }

                //        return tokens.ToArray();

                //    };

                //    string[] tokens = Tokenize(content);

                //    if (tokens.Count() < 2)
                //    {
                //        await client.SendMessageAsync(message.ChannelId, "You forgot to enter a commit message");
                //        return;
                //    }

                //   string response = FSOperations.CommitRepo(Services.CreateProvider().Services.GetService<IConfiguration>(), tokens[1]);

                //    await client.SendMessageAsync(message.ChannelId, response);
                //}else if (content.StartsWith("merge", StringComparison.OrdinalIgnoreCase))
                //{
                //    string branch = content.Substring("merge".Length,content.Length - "merge".Length).Trim();

                //    //logger.LogInformation("Branch chosen: {}", branch);
                //    comm = new(content.Substring(0, "merge".Length), client, message.ChannelId, branch);

                //    int exc = await comm.ExecuteCommand();

                //    if(exc != 0)
                //    {
                //        await client.SendMessageAsync(message.ChannelId, $"Branch {branch} failed to merge");
                //        return;
                //    }

                    
                //}else if (content.Equals("branches", StringComparison.OrdinalIgnoreCase))
                //{
                //    string response =  FSOperations.Branches(config: Services.CreateProvider().Services.GetService<IConfiguration>());

                //    await client.SendMessageAsync(message.ChannelId, response);
                //    return;
                //}else if (content.StartsWith("checkout", StringComparison.OrdinalIgnoreCase))
                //{
                //    logger.LogInformation("branch chosen: {}", content.Substring("checkout".Length, content.Length - "checkout".Length).Trim());
                //    string branch = content.Substring("checkout".Length, content.Length - "checkout".Length).Trim();

                //    if (string.IsNullOrWhiteSpace(branch))
                //    {
                //        await client.SendMessageAsync(message.ChannelId,"Branch cannot be empty");
                //        return;
                //    }

                //    string response = FSOperations.Checkout(config: Services.CreateProvider().Services.GetService<IConfiguration>(),branch: branch);
                //    await client.SendMessageAsync(message.ChannelId, response);
                //    return;
                //}
                //else
                //{
                //    await client.SendMessageAsync(message.ChannelId, "I dont know what to say to that");
                //}
            }catch(Exception ex)
            {
                logger.LogError(ex,"failed to send message");
            }

        }
    }

    public class MessageReactionHandler(ILogger<MessageReactionHandler> logger,RestClient client) : IMessageReactionAddGatewayHandler
    {
        public async ValueTask HandleAsync(MessageReactionAddEventArgs reaction)
        {
            if (MessageToggle.Ignore)
            {
                return;
            }

            User? user = reaction.User;
            IOptions<_Roles>? roles = Services.CreateProvider().Services.GetService<IOptions<_Roles>>();

            

            object value = roles.Value.GenId;

            if(value is string)
            {
                ulong GenId = ulong.Parse(value.ToString());
                await client.SendMessageAsync(GenId, $"{user.Username} reacted with {reaction.Emoji.Name}");
                return;
            }

            logger.LogError("GenId isnt a string");
            return;
        }
    }

}
