using MongoDB.Bson;
using NLog.Common;
using System.Collections.Generic;

namespace NLog.MongoAsync
{
    public static class LogEventInfoExtensions
    {
        /// <summary>
        /// Convert AsyncLogEventInfo array to BsonDocument array
        /// </summary>
        /// <param name="logEvents"></param>
        /// <param name="useFormattedMessage"></param>
        /// <returns></returns>
        public static BsonDocument[] ToBsonDocuments(this AsyncLogEventInfo[] logEvents, bool useFormattedMessage)
        {
            var documents = new BsonDocument[logEvents.Length];
            
            for (int i = 0; i < logEvents.Length; i++)
                documents[i] = logEvents[i].LogEvent.ToBsonDocument(useFormattedMessage);

            return documents;
        }

        /// <summary>
        /// Convert AsyncLogEventInfo to BsonDocument
        /// </summary>
        /// <param name="log"></param>
        /// <param name="useFormattedMessage"></param>
        /// <returns></returns>
        public static BsonDocument ToBsonDocument(this LogEventInfo log, bool useFormattedMessage)
        {
            var doc = new BsonDocument();

            Add(doc, "date", log.TimeStamp);
            Add(doc, "logger", log.LoggerName);
            Add(doc, "lvl", log.Level);
            Add(doc, "trace", log.StackTrace);
            Add(doc, "frame", log.UserStackFrame);
            Add(doc, "frameNb", log.UserStackFrameNumber);
            Add(doc, "ex", log.Exception);
            Add(doc, "data", log.Properties);

            if (useFormattedMessage)
            {
                Add(doc, "msg", log.FormattedMessage);
            }
            else
            {
                Add(doc, "msg", log.Message);
                Add(doc, "params", log.Parameters);
            }

            return doc;
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        private static void Add(BsonDocument doc, string name, string message, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                Add(doc, name, new object[] { message });
                return;
            }

            var msg = new object[parameters.Length + 1];
            msg[0] = message;
            parameters.CopyTo(msg, 1);
            Add(doc, name, msg);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="values"></param>
        private static void Add(BsonDocument doc, string name, IDictionary<object, object> values)
        {
            if (values == null || values.Count == 0)
                return;

            AddObject(doc, name, values);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="values"></param>
        private static void Add(BsonDocument doc, string name, object[] values)
        {
            if (values == null || values.Length == 0)
                return;

            AddObject(doc, name, values);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void Add(BsonDocument doc, string name, int value)
        {
            if (value != default(int))
                doc.Add(name, new BsonInt32(value));
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void Add(BsonDocument doc, string name, string value)
        {
            if (value != null)
                doc.Add(name, new BsonString(value));
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void Add(BsonDocument doc, string name, object value)
        {
            AddObject(doc, name, value);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void AddObject(BsonDocument doc, string name, object value)
        {
            if (value == null)
                return;

            BsonValue bsonValue;
            if (!BsonTypeMapper.TryMapToBsonValue(value, out bsonValue))
                bsonValue = BsonValue.Create(value.ToString());

            doc.Add(name, bsonValue);
        }
    }
}
