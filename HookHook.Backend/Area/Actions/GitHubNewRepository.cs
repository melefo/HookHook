using HookHook.Backend.Utilities;
using HookHook.Backend.Entities;
using Octokit;
using HookHook.Backend.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace HookHook.Backend.Area.Actions
{
    [Service(Providers.GitHub, "new repository is created")]
    [BsonIgnoreExtraElements]
    public class GitHubNewRepository : IAction
    {
        public string Username {get; private init;}
        public string AccountId { get; set; }

        public List<long> StoredRepositories { get; private init; } = new();

        private GitHubClient _githubClient;

        [BsonConstructor]
        public GitHubNewRepository() =>
            _githubClient = new GitHubClient(new ProductHeaderValue("HookHook"));

        public GitHubNewRepository([ParameterName("Username")] string username, string accountId, Entities.User user) : this()
        {
            Username = username;
            AccountId = accountId;

            var currentRepositories = GetRepositories(user).GetAwaiter().GetResult();

            foreach (var repo in currentRepositories)
                StoredRepositories.Add(repo.Id);
        }

        private async Task<IReadOnlyList<Repository>> GetRepositories(Entities.User user)
        {
            _githubClient.Credentials = new Credentials(user.ServicesAccounts[Providers.GitHub].SingleOrDefault(acc => acc.UserId == AccountId)!.AccessToken);

            var repositoriesForUser = await _githubClient.Search.SearchRepo(new SearchRepositoriesRequest()
            {
                User = Username
            });

            return repositoriesForUser.Items;
        }

        public async Task<(string?, bool)> Check(Entities.User user)
        {
            var repositoriesForUser = await GetRepositories(user);

            foreach (var repository in repositoriesForUser) {
                if (StoredRepositories.Contains(repository.Id))
                    continue;

                StoredRepositories.Add(repository.Id);

                return (repository.Name, true);
            }
            return (null, false);
        }

    }
}