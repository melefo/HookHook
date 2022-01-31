using HookHook.Backend.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace HookHook.Backend.Services
{
    /// <summary>
    /// Mongo service database wrapper
    /// </summary>
    public class MongoService
    {
        /// <summary>
        /// Mongo client
        /// </summary>
        private readonly MongoClient _client;
        /// <summary>
        /// Mongo Database
        /// </summary>
        private readonly IMongoDatabase _db;
        /// <summary>
        /// Mongo User Collection
        /// </summary>
        private readonly IMongoCollection<User> _usersCollection;


        /// <summary>
        /// Mongo constructor
        /// </summary>
        /// <param name="config">Host configuration</param>
        public MongoService(IConfiguration config)
        {
            _client = new MongoClient(config["Mongo:Client"]);
            _db = _client.GetDatabase(config["Mongo:Database"]);

            _usersCollection = _db.GetCollection<User>("Users");
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of user</returns>
        public List<User> GetUsers() =>
            _usersCollection.Find(x => true).ToList();

        /// <summary>
        /// Get an user by its username
        /// </summary>
        /// <param name="username">User's username</param>
        /// <returns>User account</returns>
        public User? GetUser(string id) =>
            _usersCollection.Find(x => x.Id == id).SingleOrDefault();

        /// <summary>
        /// Get user by its authentication informations
        /// </summary>
        /// <param name="identifier">User username or email</param>
        /// <returns>User account</returns>
        public User? GetUserByIdentifier(string identifier) =>
            _usersCollection.Find(x => x.Username == identifier || x.Email == identifier).SingleOrDefault();

        public User? GetUserByDiscord(string id) =>
            _usersCollection.Find(x => x.Discord != null && x.Discord.UserId == id).SingleOrDefault();

        public User? GetUserByGitHub(string id) =>
            _usersCollection.Find(x => x.GitHub != null && x.GitHub.UserId == id).SingleOrDefault();

        // public User GetUserByGoogle(string id) =>
        //     _usersCollection.Find(x => x.Google != null && x.Google.UserId == id).SingleOrDefault();

        /// <summary>
        /// Create and insert user inside database
        /// </summary>
        /// <param name="u">User accoutn</param>
        public void CreateUser(User u) => 
            _usersCollection.InsertOne(u);

        /// <summary>
        /// Delete an user account inside database
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>true if successful</returns>
        public bool DeleteUser(string id)
        {
            var result = _usersCollection.DeleteOne(x => x.Id == id);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public bool SaveUser(User user)
        {
            var result = _usersCollection.ReplaceOne(x => x.Id == user.Id, user);
            return result.IsAcknowledged && result.ModifiedCount == 1;
        }

    }

}
