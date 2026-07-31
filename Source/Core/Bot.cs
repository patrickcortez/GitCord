using Gitbot2.Source.Commands;
using Gitbot2.Source.Utils;
using GitBot2.Source;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using System.Text;
using System.Text.Json;

namespace Gitbot2.Source.Core
{

    internal class Bot
    {
        private IHost _host;
        private GatewayClient client;
        private IConfiguration config;
        private ILogger logger;
        private RestClient Rclient;
        private AuthLoader authloader;
            

        private int InitializeMembers(bool reconnect)
        {
            try
            {
                // Initialize all our members
                _host = Services.CreateProvider();
                logger = _host.Services.GetService<ILogger>();
                client = _host.Services.GetService<GatewayClient>();
                config = _host.Services.GetService<IConfiguration>();
                Rclient = _host.Services.GetService<RestClient>();
                authloader = _host.Services.GetService<AuthLoader>();

                // Configure our events
                client.Connect += async () => {
                    try
                    {
                        logger.LogInformation("GitBot connected at {}", DateTime.Now);

                        Rclient.SendMessageAsync(config.GetValue<ulong>("Discord:GeneralId"), "Hello Everyone!");

                    }catch(Exception ex)
                    {
                        logger.LogError(ex, "An Error has occured while getting users");
                    }
                };

                

                client.Disconnect += async (_) =>
                {
                    logger.LogInformation("Gitbot disconnected at {}", DateTime.Now);

                    if (reconnect) { // is reconnect flag is on, attempt to reconnect

                        logger.LogInformation("Reconnecting...");

                        await _host.StartAsync();

                        

                    }
                };

                client.Close += () => // its simple for now, might expand in the future
                {
                    logger.LogInformation("Discord Bot Closed at {}",DateTime.Now);
                    return ValueTask.CompletedTask;
                };


                client.GuildCreate += async (guild) =>
                {
                    try
                    {
                    
                        var list = guild.Guild!.Presences.Values;


                        List<User> _users = new();

                        Presence[] presences = list!.Where(c => c.Status == UserStatusType.Online || 
                        c.Status == UserStatusType.Idle || 
                        c.Status == UserStatusType.DoNotDisturb).ToArray();



                        presences.ToList().ForEach((s) =>
                        {
                            if (guild.Guild.Users.TryGetValue(s.User.Id,out GuildUser? xuser))
                            {
                                if(xuser is not null)
                                {
                                    _users.Add(xuser);
                                }
                            }
                        }); 

                        logger.LogInformation("Online users: {}",_users.Count);
                        logger.LogInformation("{}", _users.Select(c => c.Username));

                        StringBuilder msg = new();

                        _users.ForEach((s) => {

                            if (s.Username == "GitCord")
                            {
                                // Do nothing, since we cant use continue anyways
                            }
                            else
                            {
                               
                               
                                msg.AppendLine($"Hello <@{s.Id}>!");
                            }
                        
                        });

                        await Rclient.SendMessageAsync(config.GetValue<ulong>("Discord:GeneralId"), msg.ToString());
                        await Rclient.SendMessageAsync(config.GetValue<ulong>("Discord:GeneralId"), "Type `/help` for all the available commands");
                        
                      //  return ValueTask.CompletedTask;
                    }catch(Exception ex)
                    {
                        logger.LogError(ex, "An Error has occured");
                        //return ValueTask.CompletedTask;
                    }
                };

                
               
                

                return 0;
            }catch(Exception ex)
            {

                return 1;
            }
        } 
        

        public Bot(bool reconnect = false) // Constructor
        {
            InitializeMembers(reconnect);
            ConsoleEvent.RegisterConsoleEvents(_host,logger);
            
        }

        private async Task SetHost(IHost _host)
        {
            _host.AddComponentInteraction<ModalInteractionContext>("auth", async (ModalInteractionContext context) => {

                try
                {
                    logger.LogInformation("Modal interaction recieved at {}", DateTime.Now);
                    var comps = context.Components;
                    Label lun = (Label)comps[0];
                    Label lap = (Label)comps[1];

                    (TextInput uname, TextInput pat) Components = ((TextInput)lun.Component, (TextInput)lap.Component);

                    RepoCache.SetContent((Components.uname.Value, Components.pat.Value,context.User.Username));

                    Auth auth = new();
                    auth.PAT = RepoCache.GetRecentContent().pat;
                    auth.GitName = RepoCache.GetRecentContent().uname;
                    auth.Username = RepoCache.GetRecentContent().username;

                    await authloader.PushToDB(auth);

                    await context.Interaction.SendResponseAsync(InteractionCallback.Message("Information saved! Thank you for trusting GitCord"));
                }
                catch (Exception ex)
                {
                    RepoCache.SetException(ex);
                    logger.LogError(ex, "Something went wrong while interacting");
                }

            });

            _host.AddComponentInteraction<ButtonInteractionContext>("btn_yes", async (ButtonInteractionContext interact) => {
                try
                {
                    User current = interact.User;
                    DMmanager dm = new(current, interact.Client.Rest);
                    await dm.SendModal(interact.Interaction);
                }catch(Exception ex)
                {
                    RepoCache.SetException(ex);
                    logger.LogError(ex, "Something went wrong while interacting");
                }
                
            });

            _host.AddComponentInteraction<ButtonInteractionContext>("btn_no", async (ButtonInteractionContext interact) => {
                try
                {
                    await interact.Interaction.SendResponseAsync(InteractionCallback.Message("Cancelling Operation"));
                    return;
                }catch(Exception ex)
                {
                    RepoCache.SetException(ex);
                    logger.LogError(ex, "Something went wrong while interacting");
                }
            });

        }

        public async Task<int> RunAsync(bool isLog = false) // Start Bot
        {
            try
            {
                string path = Path.Combine(Environment.CurrentDirectory, "repos.json");
                string content = await File.ReadAllTextAsync(path);

                Repositories? repo = JsonSerializer.Deserialize<Repositories>(content);
                RepoCache.SetCache(repo.Repos,path); // set our repo cache

                await SetHost(_host);

                _host.AddModules(typeof(Program).Assembly);
                RepoCache.SetException(new("No Exceptions Yet"));
                RepoCache.SetCurrentRepo(config.GetValue<string>("Discord:Current"));
                RepoCache.SetRequests();
                RepoCache.SetContent(("None","None","None"));
                RepoCache.SetAuths(await authloader.GetDB());


                await _host.StartAsync();

                await Task.Delay(-1);

                return 0;

            }catch(Exception ex)
            {
                logger.LogError(ex, "Gitbot: An Error has occured");

                string fullex = ex.ToString(),
                       errP = Path.Combine(Environment.CurrentDirectory, "error.log")
                    ;

                await File.AppendAllTextAsync(errP, $"[{DateTime.Now}]\n{fullex}\n{"-".PadRight(20)}\n");

                logger.LogError("Details written to {}",errP);

                return 1;
            }
        }
    }
}
