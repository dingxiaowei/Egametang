﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Xml;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for DateTimeOffsets.
    /// </summary>
    public class DateTimeOffsetSerializer : BsonBaseSerializer
    {
        // private static fields
        private static DateTimeOffsetSerializer __instance = new DateTimeOffsetSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the DateTimeOffsetSerializer class.
        /// </summary>
        public DateTimeOffsetSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Array))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the DateTimeOffsetSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static DateTimeOffsetSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(DateTimeOffset));

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            long ticks;
            TimeSpan offset;
            switch (bsonType)
            {
                case BsonType.Array:
                    bsonReader.ReadStartArray();
                    ticks = bsonReader.ReadInt64();
                    offset = TimeSpan.FromMinutes(bsonReader.ReadInt32());
                    bsonReader.ReadEndArray();
                    return new DateTimeOffset(ticks, offset);
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    bsonReader.ReadDateTime("DateTime"); // ignore value
                    ticks = bsonReader.ReadInt64("Ticks");
                    offset = TimeSpan.FromMinutes(bsonReader.ReadInt32("Offset"));
                    bsonReader.ReadEndDocument();
                    return new DateTimeOffset(ticks, offset);
                case BsonType.String:
                    return XmlConvert.ToDateTimeOffset(bsonReader.ReadString());
                default:
                    var message = string.Format("Cannot deserialize DateTimeOffset from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            // note: the DateTime portion cannot be serialized as a BsonType.DateTime because it is NOT in UTC
            var dateTimeOffset = (DateTimeOffset)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Array:
                    bsonWriter.WriteStartArray();
                    bsonWriter.WriteInt64(dateTimeOffset.Ticks);
                    bsonWriter.WriteInt32((int)dateTimeOffset.Offset.TotalMinutes);
                    bsonWriter.WriteEndArray();
                    break;
                case BsonType.Document:
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteDateTime("DateTime", BsonUtils.ToMillisecondsSinceEpoch(dateTimeOffset.UtcDateTime));
                    bsonWriter.WriteInt64("Ticks", dateTimeOffset.Ticks);
                    bsonWriter.WriteInt32("Offset", (int)dateTimeOffset.Offset.TotalMinutes);
                    bsonWriter.WriteEndDocument();
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(XmlConvert.ToString(dateTimeOffset));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid DateTimeOffset representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}
