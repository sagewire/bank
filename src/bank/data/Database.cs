using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Elasticsearch.Net;
using Mustache;
using bank.data.elasticsearch.responses;
using bank.Properties;

namespace bank.data.elasticsearch
{
    public static class Database
    {

        public static ElasticLowLevelClient Client()
        {
            var node = new Uri(Settings.ElasticsearchHost);
            IConnectionPool connectionPool;

            //the sniffing connection pool doesn't work over the VPN
            if (Settings.ElasticsearchHost.Contains("192.168.3."))
            {
                connectionPool = new SingleNodeConnectionPool(node);
            }
            else
            {
                connectionPool = new SniffingConnectionPool(new[] { node });
            }

            var settings = new ConnectionConfiguration(connectionPool)
                                .SniffLifeSpan(TimeSpan.FromMinutes(5))
                                .PingTimeout(new TimeSpan(0, 0, 0, 0, 500))
                                .RequestTimeout(new TimeSpan(0, 0, 0, 0, 120000));

            settings.BasicAuthentication("elastic", Settings.ElasticsearchPassword);

            var client = new ElasticLowLevelClient(settings);

            return client;

        }

        //private static T LoadJsonFromLocalCache<T>(string key, dynamic parameters, bool cache = false, TimeSpan ttl = new TimeSpan(), string index = "entities", string type = "entity") where T : class
        //{

        //    string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"..\json-cache");

        //    string filename = key;

        //    if (parameters != null)
        //    {
        //        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(parameters.GetType()))
        //        {
        //            var name = property.Name;
        //            var value = property.GetValue(parameters).ToString();

        //            filename = string.Format("{0}-{1}-{2}", filename, name, value);
        //        }
        //    }

        //    filename = filename + ".json";

        //    path = Path.Combine(path, filename);
        //    string jsonCache = null;
        //    bool refreshCache = true;

        //    if (File.Exists(path))
        //    {
        //        //load from file cache
        //        jsonCache = File.ReadAllText(path);


        //        if (ttl.TotalMilliseconds > 0)
        //        {
        //            var lastmod = File.GetLastWriteTime(path);
        //            var diff = DateTime.Now - lastmod;

        //            if (diff.TotalMilliseconds <= ttl.TotalMilliseconds)
        //            {
        //                refreshCache = false;
        //            }
        //        }
        //    }

        //    if (refreshCache)
        //    {
        //        var task = Task.Factory.StartNew(() =>
        //        {
        //            jsonCache = ExecuteJsonResource<string>(
        //                key,
        //                parameters,
        //                index: index,
        //                type: type,
        //                cache: cache);

        //            //jsonCache = ExecuteJsonResource(key, parameters, cache);
        //            File.WriteAllText(path, jsonCache);
        //        });

        //        if (jsonCache == null)
        //        {
        //            task.Wait();
        //        }
        //    }

        //    if (typeof(T).Name == "String")
        //    {
        //        return jsonCache as T;
        //    }
        //    else
        //    {
        //        return JsonConvert.DeserializeObject<T>(jsonCache);
        //    }
        //}

        public static DeleteResponse Delete(string index, string type, string id)
        {
            var response = Client().Delete<DeleteResponse>(index, type, id);
            return response.Body;
        }

        public static IndexResponse Index(object obj, string id, string index, string type)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Index(json, id, index, type);
        }

        public static IndexResponse Index(string json, string id, string index, string type)
        {
            PostData<object> post = new PostData<object>(json);

            var response = Client().Index<IndexResponse>(index, type, id, post);

            return response.Body;
            //return System.Text.Encoding.UTF8.GetString(response.ResponseRaw);
        }

        private static T Get<T>(string index, string type, object id) where T : class
        {

            var response = Client().Get<string>(index, type, id.ToString());

            if (response.Body == null)
            {
                return null;
            }

            var result = JsonConvert.DeserializeObject<T>(response.Body);

            return result;
        }

        public static string ExecuteJsonResource(string key, string index, string type, dynamic parameters = null, bool cache = false)
        {
            return ExecuteJsonResource<string>(key, index, type, parameters, cache);
        }

        public static T ExecuteJsonResource<T>(string key, string index, string type, dynamic parameters = null, bool cache = false) where T : class
        {

            var searchParams = new SearchRequestParameters();
            if (cache)
            {
                //throw new NotSupportedException();
                //searchParams.SearchType(Elasticsearch.Net.SearchType.Count);
                //searchParams.RequestCache(true);
                //searchParams.QueryCache(true);
            }



            string json;

            try
            {
                //var enc = new UTF8Encoding();
                var resource = (byte[])Resources.ResourceManager.GetObject(key.Replace("-", "_"));
                json = System.Text.Encoding.UTF8.GetString(resource);
                //var json = enc.GetString((byte[])Resources.ResourceManager.GetObject(key.Replace("-", "_")));
            }
            catch (Exception ex)
            {
                throw;
            }

            var compiler = new FormatCompiler();
            var generator = compiler.Compile(json);
            json = generator.Render(parameters);


            ElasticsearchResponse<string> response;

            try
            {
                response = Client().Search<string>(index, type, json, x => searchParams);

                if (typeof(T).Name == "String")
                {
                    return response.Body as T;
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<T>(response.Body);
                    return result;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
