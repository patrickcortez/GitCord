using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Gitbot2.Source.Utils
{
    internal static class Utility
    {
        private static ILogger logger;
        private static IOptions<_Roles> config;
        static Utility(){

            if (!isJsonAvailable())
            {
                return;
            }

            logger = Services.CreateProvider("Utility").Services.GetRequiredService<ILogger>();
            config = Services.CreateProvider("Utility").Services.GetService<IOptions<_Roles>>();
        }
        public static async Task<RoleStatus> isAllowed(RestClient client,Message message)
        {
            try
            {

                var gUser = await client.GetGuildUserAsync(message.GuildId!.Value, message.Author.Id);

                object value = config.Value.Roles;

                RoleStatus final = RoleStatus.NotAllowed;

                if(value is string[] array)
                {
                    ulong[] roles = array.Select(ulong.Parse).ToArray();
                    
                    gUser.RoleIds.ToList().ForEach((id) =>
                    {
                        if (roles.Contains(id))
                        {
                            final = RoleStatus.Allowed;
                        }
                    });
                }



                return final;

            }catch(Exception ex)
            {
                logger.LogError(ex, "Failed to get users role");
                return RoleStatus.Error;
            }
            
        }

        public static async Task<object>? GetValueAsync(string key)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "config.json");

            using Stream stream = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None,4096);

            _Roles? roles = await JsonSerializer.DeserializeAsync<_Roles>(stream);


            if(key == "Roles") // Array
            {
                return roles.Roles;
            }else if(key == "GenId") // String
            {
                return roles.GenId;
            }

            return null; // If key is not valid
            
        }


        public static bool isJsonAvailable() // Will update this soon...
        {
            string path = Path.Combine(Environment.CurrentDirectory, "repos.json"); 

            return Path.Exists(path);
        }


        
    }
}
