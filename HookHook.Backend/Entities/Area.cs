﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HookHook.Backend.Services;

namespace HookHook.Backend.Entities
{
    /// <summary>
    /// An AREA data saved in database
    /// </summary>
    public class Area
    {
        /// <summary>
        /// Database User-Object Id
        /// </summary>
        //[JsonIgnore]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private init; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// Last successful Area update
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Number of minutes between updates
        /// </summary>
        public int MinutesBetween { get; set; }

        /// <summary>
        /// Action we are waiting for
        /// </summary>
        public IAction Action { get; private init; }

        /// <summary>
        /// Reactions executed if action
        /// </summary>
        public List<IReaction> Reactions { get; private init; }

        public Area(IAction action, IEnumerable<IReaction> reactions, int minutes)
        {
            Action = action;
            Reactions = new(reactions);
            MinutesBetween = minutes;
        }

        public async Task Launch(User user, MongoService _db)
        {
            // ? pass area ID here so the action can modify it in the db
            (string? actionInfo, bool actionValue) = await Action.Check(user);

            if (!actionValue)
                return;

            foreach (var reaction in Reactions)
                await reaction.Execute(user);
            _db.SaveUser(user);
        }
    }
}