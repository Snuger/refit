using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Refit;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Refit.YamlDotNet.Ymal
{
    public class YamlContentSerializer : IHttpContentSerializer
    {
        /// <summary>
        /// The <see cref="Lazy{T}"/> instance providing the JSON serialization settings to use
        /// </summary>
       private readonly Lazy<ISerializer> serializer;
       private  readonly Lazy<IDeserializer> deserializer;
        /// <summary>
        /// 
        /// </summary>
        public YamlContentSerializer() : this(null,null) { }

        public YamlContentSerializer(ISerializer serializer,IDeserializer deserializer)
        { 
            this.serializer = new Lazy<ISerializer>(() => serializer ?? new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build());
            this.deserializer = new Lazy<IDeserializer>(()=> deserializer?? new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build());
        }


        public async Task<T?> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
        {     
            using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            var contStr= await reader.ReadToEndAsync();      
            return deserializer.Value.Deserialize<T>(contStr);
        }

        public string GetFieldNameForProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.GetCustomAttributes<JsonPropertyAttribute>(true).Select(a => a.PropertyName).FirstOrDefault();
        }

        public HttpContent ToHttpContent<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            var content = new StringContent(serializer.Value.Serialize(item), Encoding.UTF8, "application/json");
            return content;
        }
    }
}
