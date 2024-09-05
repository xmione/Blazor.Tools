using System.Text.Json;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class JsonExtensions
    {
        public static string GetFieldValue(this string propertyValue, string fieldName)
        {
            string fieldValue = string.Empty;
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(propertyValue);
                if (data != null)
                {
                    if (data.ContainsKey(fieldName))
                    {
                        fieldValue = data[fieldName]?.ToString() ?? "N/A";
                    }
                    else
                    {
                        fieldValue = "Field not found";
                    }
                }
            }
            catch (Exception ex)
            {
                fieldValue = "Error: " + ex.Message;
            }

            return fieldValue;
        }
    }
}
