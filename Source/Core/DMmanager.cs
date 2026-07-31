using Gitbot2.Source.Commands;
using NetCord;
using NetCord.Rest;

namespace Gitbot2.Source.Core
{
    internal class DMmanager(User user ,RestClient client)
    {
        public async Task SendDM()
        {
            try
            {
                DMChannel dm = await client.GetDMChannelAsync(user.Id);

                IMessageComponentProperties[] prop = [new ActionRowProperties([new ButtonProperties("btn_yes", "Yes", ButtonStyle.Success), 
                    new ButtonProperties("btn_no", "No", ButtonStyle.Danger)])];

                MessageProperties content = new()
                {

                    Content="Are you ready to proceed?",
                    Components = prop
                };

                RepoCache.AddRequest(user);

                var message = await client.SendMessageAsync(dm.Id,content);

          
                
            }catch(Exception ex)
            {
                RepoCache.SetException(ex);
            }
          
            

            
        }

        public async Task SendModal(Interaction interact)
        {
           

            var modal = new ModalProperties("auth", "Git User Information", [new LabelProperties("Enter Username",new TextInputProperties("txt_uname",TextInputStyle.Short)),
            new LabelProperties("Enter PAT",new TextInputProperties("txt_pat",TextInputStyle.Paragraph))]);




            await interact.SendResponseAsync(InteractionCallback.Modal(modal));
        }
    }
}
