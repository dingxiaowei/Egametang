﻿using Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class StartConfig: Entity
	{
		public int AppId { get; set; }
		
		[BsonRepresentation(BsonType.String)]
		public AppType AppType { get; set; }
		
		public string ServerIP { get; set; }

		public StartConfig(): base(EntityType.Config)
		{
		}

		public object Clone()
		{
			return MongoHelper.FromJson<StartConfig>(MongoHelper.ToJson(this));
		}
	}
}
