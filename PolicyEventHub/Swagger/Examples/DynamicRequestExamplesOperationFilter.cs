using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace PolicyEventHub.Swagger.Examples
{
    public class DynamicRequestExamplesOperationFilter : IOperationFilter
    {
        private static readonly JsonSerializerOptions JsonOptions =
           new()
           {
               DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
           };

        private static readonly Dictionary<Type, List<Type>> ExampleMap = BuildExampleMap();
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content == null)
                return;

            if (!operation.RequestBody.Content.TryGetValue("application/json", out var json))
                return;

            var bodyType = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Source?.Id == "Body")
                ?.Type;

            if (bodyType == null)
                return;

            if (!ExampleMap.TryGetValue(bodyType, out var exampleTypes))
                return;

            json.Examples ??= new Dictionary<string, OpenApiExample>();

            foreach (var exampleType in exampleTypes)
            {
                var instance = Activator.CreateInstance(exampleType);
                var method = exampleType.GetMethod("GetExamples", BindingFlags.Public | BindingFlags.Instance);

                if (method == null)
                    continue;

                var exampleValue = method.Invoke(instance, null);
                if (exampleValue == null)
                    continue;

                json.Examples[exampleType.Name] = new OpenApiExample
                {
                    Summary = exampleType.Name.Replace("RequestExample", string.Empty),
                    Value = ToOpenApiAny(exampleValue)
                };
            }
        }
        private static Dictionary<Type, List<Type>> BuildExampleMap()
        {
            var map = new Dictionary<Type, List<Type>>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic);

            foreach (var type in assemblies.SelectMany(a => SafeGetTypes(a)))
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var iface = type.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IExamplesProvider<>));

                if (iface == null)
                    continue;

                var dtoType = iface.GetGenericArguments()[0];

                if (!map.TryGetValue(dtoType, out var list))
                    map[dtoType] = list = new List<Type>();

                list.Add(type);
            }

            return map;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static IOpenApiAny ToOpenApiAny(object value)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            return ConvertElement(doc.RootElement);
        }
        private static IOpenApiAny ConvertElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    {
                        var obj = new OpenApiObject();

                        foreach (var prop in element.EnumerateObject())
                        {
                            obj[prop.Name] = ConvertElement(prop.Value);
                        }

                        return obj;
                    }
                case JsonValueKind.Array:
                    {
                        var arr = new OpenApiArray();

                        foreach (var item in element.EnumerateArray())
                        {
                            arr.Add(ConvertElement(item));
                        }

                        return arr;
                    }
                case JsonValueKind.String:
                    return new OpenApiString(element.GetString()!);
                case JsonValueKind.Number:
                    return element.TryGetInt64(out var l)
                        ? new OpenApiLong(l)
                        : new OpenApiDouble(element.GetDouble());
                case JsonValueKind.True:
                    return new OpenApiBoolean(true);
                case JsonValueKind.False:
                    return new OpenApiBoolean(false);
                case JsonValueKind.Null:
                    return new OpenApiNull();
                default:
                    return new OpenApiString(element.ToString());
            }
        }

    }
}
