using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;

namespace Stockpile.DataProvider.Lucandrew
{
    public class Database : IDisposable
    {
        private readonly string _path;
        private readonly IndexWriter _indexWriter;
        private IndexReader _indexReader;
        private Searcher _searcher;
        private readonly StandardAnalyzer _analyzer;

        private const string BlobKey = "_BLOB";
        private const string ClassKey = "_CLASS";
        private const string IdKey = "_ID";
        private const Lucene.Net.Util.Version Version = Lucene.Net.Util.Version.LUCENE_30;

        private readonly List<object> _loopDetector = new List<object>();

        public Database(string path)
        {
            _path = path;

            bool newIndex = false;

            if (!System.IO.Directory.Exists(_path))
            {
                System.IO.Directory.CreateDirectory(_path);
                newIndex = true;
            }
            
            _analyzer = new StandardAnalyzer(Version);
            _indexWriter = new IndexWriter(FSDirectory.Open(_path), _analyzer, newIndex, IndexWriter.MaxFieldLength.UNLIMITED);
            _indexReader = _indexWriter.GetReader();
            _searcher = new IndexSearcher(_indexReader);
        }

        public ObjectReference<T> Store<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Document doc = GetDocumentFromObject(obj);
            _indexWriter.AddDocument(doc);
            return GetObjectFromDocument<T>(doc);
        }

        public IEnumerable<ObjectReference<T>> Search<T>(object obj = null)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            ReopenIndexIfNeeded();
            Type type = typeof(T);
            string objectClass = type.ToString();

            Query query = GetQueryFromObject(obj, objectClass);

            TopDocs docs = _searcher.Search(query, _searcher.MaxDoc > 0 ? _searcher.MaxDoc : int.MaxValue);

            foreach (ScoreDoc scoreDoc in docs.ScoreDocs)
            {
                yield return GetObjectFromDocument<T>(_searcher.Doc(scoreDoc.Doc));
            }
        }

        public void Update<T>(ObjectReference<T> reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            _indexWriter.UpdateDocument(new Term(IdKey, reference.Id), GetDocumentFromObject(reference.Object, reference.Id));
        }

        public void Delete(ObjectReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            _indexWriter.DeleteDocuments(new Term(IdKey, reference.Id));
        }

        private ObjectReference<T> GetObjectFromDocument<T>(Document document)
        {
            string idValue = document.Get(IdKey);

            if (idValue == null)
                throw new ApplicationException("Could not determine the _ID of the object");

            string jsonBlob = document.Get(BlobKey);

            T obj = JsonConvert.DeserializeObject<T>(jsonBlob, new JsonSerializerSettings() { });

            return new ObjectReference<T>(this, idValue, obj);
        }

        private Document GetDocumentFromObject(object obj, string id = null)
        {
            string objClass = GetClassFromObject(obj);
            var values = GetPropertiesFromObject(obj);

            Document document = new Document();

            string blobData = JsonConvert.SerializeObject(obj);

            document.Add(new Field(ClassKey, objClass, Field.Store.NO, Field.Index.ANALYZED));
            document.Add(new Field(IdKey, id ?? Guid.NewGuid().ToString("N"), Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(BlobKey, blobData, Field.Store.YES, Field.Index.NO));

            foreach (KeyValuePair<string, string> keyValuePair in values)
            {
                document.Add(new Field(keyValuePair.Key, keyValuePair.Value, Field.Store.NO, Field.Index.ANALYZED));
            }

            return document;
        }

        private string GetClassFromObject(object obj)
        {
            return obj.GetType().ToString();
        }

        private Dictionary<string, string> GetPropertiesFromObject(object obj)
        {
            if (_loopDetector.Contains(obj) || obj == null)
                return null;
            _loopDetector.Add(obj);
            Dictionary<string, string> properties = new Dictionary<string, string>();

            Type objectType = obj.GetType();
            PropertyInfo[] props = objectType.GetProperties();

            foreach (PropertyInfo propertyInfo in props)
            {
                object value = propertyInfo.GetValue(obj);
                if (propertyInfo.PropertyType == typeof(string))
                {
                    //Do nothing
                }
                else if (value is DateTime)
                {
                    value = DateTools.DateToString((DateTime)value, DateTools.Resolution.MILLISECOND);
                }
                else if (value is Stream)
                {
                    value = null;
                }
                else if (propertyInfo.PropertyType.IsClass)
                {
                    Dictionary<string, string> subObjectProperties = GetPropertiesFromObject(value);
                    if (subObjectProperties != null)
                    {
                        foreach (KeyValuePair<string, string> subObjectProperty in subObjectProperties)
                        {
                            properties.Add(string.Format("{0}.{1}", propertyInfo.Name, subObjectProperty.Key), subObjectProperty.Value);
                        }
                    }
                    value = null;
                }
                if (value != null)
                    properties.Add(propertyInfo.Name, value.ToString());
            }
            _loopDetector.Remove(obj);
            return properties;
        }

        private Query GetQueryFromObject(object obj, string className = null)
        {
            BooleanQuery booleanQuery = new BooleanQuery();

            if (className != null)
            {
                QueryParser classParser = new QueryParser(Version, ClassKey, _analyzer);
                Query classQuery = classParser.Parse(className);
                booleanQuery.Add(classQuery, Occur.MUST);
            }

            if (obj == null)
                return booleanQuery;

            Dictionary<string, string> props = GetPropertiesFromObject(obj);
            foreach (KeyValuePair<string, string> keyValuePair in props)
            {
                QueryParser propParser = new QueryParser(Version, keyValuePair.Key, _analyzer);
                Query propQuery = propParser.Parse(keyValuePair.Value);

                booleanQuery.Add(propQuery, Occur.MUST);
            }
            return booleanQuery;
        }

        private void ReopenIndexIfNeeded()
        {
            if (!_indexReader.IsCurrent())
            {
                _indexReader = _indexWriter.GetReader();
                _searcher = new IndexSearcher(_indexReader);
            }
        }

        public void Dispose()
        {
            _searcher.Dispose();
            _indexReader.Dispose();
            _indexWriter.Commit();
            _indexWriter.Dispose();
        }
    }
}
