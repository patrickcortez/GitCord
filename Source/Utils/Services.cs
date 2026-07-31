using Gitbot2.Source.Core;
using GitBot2.Source;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Gateway.JsonModels;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.Commands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace Gitbot2.Source.Utils
{
    internal static class Services
    {

        public static IHost CreateProvider(string categoryname = "")
        {
            try
            {

                var builder = Host.CreateApplicationBuilder(); // Build our Host

                builder.Services
                    .AddDiscordGateway(

                        option =>
                        {
                            option.Token = new BotToken(builder.Configuration["Discord:Token"]).RawToken;
                            option.Intents = GatewayIntents.GuildMessages
                            | GatewayIntents.DirectMessages
                            | GatewayIntents.MessageContent
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.DirectMessageReactions
                            | GatewayIntents.Guilds
                            | GatewayIntents.GuildUsers
                            | GatewayIntents.GuildPresences
                            | GatewayIntents.GuildMessageReactions;



                        }
                    ).AddCommands()
                    .AddApplicationCommands()
                    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
                    .AddComponentInteractions<ButtonInteraction,ButtonInteractionContext>()
                    .AddGatewayHandlers(typeof(Program).Assembly)
                    .AddSingleton<ILogger>(LoggerFactory.Create(c => c.AddConsole()).CreateLogger(categoryname))
                    .AddSingleton<AuthLoader>()
                    .AddLogging()
                    ;

                builder.Configuration.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "config.json"));
                builder.Services.Configure<_Roles>(builder.Configuration);



                return builder.Build();
            }catch(Exception ex)
            {
                ILogger logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger("Services");
                logger.LogError(ex, "Something went wrong while creating services");


                throw;
            }
        }
    }
}
