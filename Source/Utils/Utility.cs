using Gitbot2.Source.Commands;
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
        public static async Task<RoleStatus> isAllowed(RestClient client,Message message) // legacy code, not in use
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
                RepoCache.SetException(ex);
                logger.LogError(ex, "Failed to get users role");
                return RoleStatus.Error;
            }
            
        }

        public static string[] TokenizeLine(string line)
        {
            bool isinQoutes = false;
            int Wdepth = 0;
            StringBuilder token = new();
            List<string> tmp = new();

            foreach(char c in line)
            {
                if (char.IsWhiteSpace(c))
                {
                    
                    if(token.Length > 0)
                    {
                        tmp.Add(token.ToString());
                        token.Clear();
                        continue;
                    }

                    continue;
                }

                if(c == '"')
                {
                    isinQoutes = !isinQoutes;
                    continue;
                }

                token.Append(c);
            }

            // Last check
            if(token.Length > 0)
            {
                tmp.Add(token.ToString());
                token.Clear();
            }

            return tmp.ToArray();

        }

        public static string ReplaceAt(string line, int startingPos, int length) // Quick helper function
        {
            StringBuilder newWord = new(line);
            newWord.Replace(' ', '*', startingPos, length);
            return newWord.ToString();
        }

        public static async Task WritetoFile(string path, IEnumerable<string> lines)
        {
            try
            {
                await File.WriteAllLinesAsync(path, lines);
            }catch(Exception ex)
            {
                RepoCache.SetException(ex);
                logger.LogError(ex, "An Error has occurred while Writting to file");
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
