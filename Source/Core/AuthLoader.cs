using Gitbot2.Source.Commands;
using System.Text.Json;

namespace Gitbot2.Source.Core
{
    internal sealed class AuthLoader
    {
        public async Task<Auths>? GetDB()
        {
            await using FileStream fs = new FileStream(RepoCache.authdb, FileMode.Open, FileAccess.Read, FileShare.Read);

            Auths? auths = await JsonSerializer.DeserializeAsync<Auths>(fs);

            return auths;
        }

        public async Task PushToDB(Auth auth)
        {
            Auths auths = new();
            auths.auths.Add(auth);
            await using FileStream fs = new FileStream(RepoCache.authdb, FileMode.Append, FileAccess.Write, FileShare.Write);

            await JsonSerializer.SerializeAsync<Auths>(fs, auths);
        }
    }
}
