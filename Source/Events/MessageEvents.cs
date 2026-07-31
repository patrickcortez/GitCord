using Gitbot2.Source.Commands;
using Gitbot2.Source.Utils;
using Gitbot2.Source.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Gitbot2.Source.Events
{

    //public class InteractionHandler(ILogger logger,RestClient client) : IInteractionCreateGatewayHandler
    //{
    //    public async ValueTask HandleAsync(Interaction interact)
    //    {
    //        try
    //        {
    //            User current = interact.User;
    //            if (interact is MessageComponentInteraction component)
    //            {
    //                if (RepoCache.isInRequests(current)) // might move this to host add component interaction in _host
    //                {
    //                    switch (component.Data.CustomId)
    //                    {
    //                        case "btn_yes":
                                
    //                            DMmanager dm = new(current, client);
    //                            await dm.SendModal(interact);
    //                            break;
    //                        case "btn_no":
    //                            return;


    //                        default:
    //                            return; // btn is not an option
    //                    }
    //                }
    //            }
    //            else if (interact is ModalInteraction modal)
    //            {
    //                //logger.LogInformation("Modal interaction recieved at {}",DateTime.Now);
    //                //var comps = modal.Data.Components;
    //                //Label lun = (Label)comps[0];
    //                //Label lap = (Label)comps[1];

    //                //(TextInput uname, TextInput pat) Components = ((TextInput)lun.Component, (TextInput)lap.Component);

    //                //RepoCache.SetContent((Components.uname.Value,Components.pat.Value));
    //            }
    //        }catch(Exception ex)
    //        {
    //            RepoCache.SetException(ex);
    //            logger.LogError(ex, "Something Went wrong while working on a response");
    //            return;
    //        }
    //    }
    //}

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
