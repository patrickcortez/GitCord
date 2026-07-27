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
