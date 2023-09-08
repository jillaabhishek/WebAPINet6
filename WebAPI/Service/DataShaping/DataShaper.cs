using Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Service.DataShaping
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }

        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldString)
        {
            var requiredProperties = GetRequiredProperties(fieldString);
            return FetchData(entities, requiredProperties);
        }

        public ExpandoObject ShapeData(T entities, string fieldString)
        {
            var requiredProperties = GetRequiredProperties(fieldString);
            return FetchDataForEntity(entities, requiredProperties);
        }

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldString)
        {

            var requiredProperties = new List<PropertyInfo>();

            if (string.IsNullOrEmpty(fieldString))
                return Properties.ToList();

            var fields = fieldString.Split(",", StringSplitOptions.RemoveEmptyEntries);

            foreach (var field in fields)
            {
                var property = Properties.FirstOrDefault(x => x.Name.Equals(field.Trim(), StringComparison.OrdinalIgnoreCase));

                if (property is null) continue;

                requiredProperties.Add(property);
            }

            return requiredProperties;
        }

        private IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<ExpandoObject>();

            foreach(var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        private ExpandoObject FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ExpandoObject();

            foreach(var property in requiredProperties)
            {
                var objectPropertiesValue = property.GetValue(entity);
                shapedObject.TryAdd(property.Name, objectPropertiesValue);
            }

            return shapedObject;
        }
    }
}
