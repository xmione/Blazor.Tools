using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class LinqExtensions
    {
        public static async Task<List<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            var tasks = source.Select(selector).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }

        public static object? GetPropertyValue<T>(this T model, string propertyName)
        {
            object? propertyValue = default!;
            try
            {
                if (model != null)
                {
                    propertyValue = model.GetType().GetProperty(propertyName)?.GetValue(model, null);

                }

                return propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPropertyValue(object obj, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return propertyValue;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(obj);
                if (value is T typedValue)
                {
                    return typedValue;
                }
                else if (value != null && typeof(T) == typeof(string))
                {
                    return (T)(object)value.ToString()!;
                }
            }
            return default!;
        }

        public static decimal GetPropertyDecimalValue<T>(this T model, string propertyName)
        {
            decimal propertyValue = default!;
            try
            {
                if (model != null)
                {
                    string decimalValue = model.GetType().GetProperty(propertyName)?.GetValue(model, null)?.ToString() ?? string.Empty;
                    propertyValue = decimal.Parse(decimalValue);

                }

                return propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPropertyValue(object obj, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return propertyValue;
        }

        public static object? GetPropertyInfoValue<T>(this PropertyInfo info, T model, T previousModel, string propertyName)
        {
            object? propertyValue = default!;
            try
            {
                if (model != null)
                {
                    var currentValue = model.GetType().GetProperty(propertyName)?.GetValue(model, null);
                    var previousValue = previousModel != null ? previousModel.GetType().GetProperty(propertyName)?.GetValue(previousModel, null) : null;

                    propertyValue = currentValue != null && currentValue.Equals(previousValue) ? "" : currentValue;
                }

                return propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPropertyValue(object obj, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return propertyValue;
        }

        public static string SafeGetString<T>(this T model, string propertyName)
        {
            string value = model?.GetPropertyValue(propertyName)?.ToString() ?? string.Empty;

            return value;
        }

        public static DateTime? GetDateTimePropertyValue<T>(this T model, string propertyName)
        {
            DateTime? propertyValue = default!;
            try
            {
                if (model != null)
                {
                    string dateTimeValue = model.GetType().GetProperty(propertyName)?.GetValue(model, null)?.ToString() ?? string.Empty;
                    propertyValue = DateTime.Parse(dateTimeValue);
                }

                return propertyValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDateTimePropertyValue<T>(this T model, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return propertyValue;
        }

        public static DateOnly GetDateOnlyPropertyValue<T>(this T model, string propertyName)
        {
            DateOnly propertyValue = DateOnly.MinValue!;
            try
            {
                if (model != null)
                {
                    if (model.GetType().GetProperty(propertyName)?.GetValue(model, null) is DateTime dateTimeValue)
                    {
                        propertyValue = DateOnly.FromDateTime(dateTimeValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDateTimePropertyValue<T>(this T model, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return propertyValue;
        }

        public static bool InBetWeenDate<T>(this T model, string propertyName, DateOnly fromDate, DateOnly toDate)
        {
            bool isInbetween = false;
            DateOnly propertyValue = DateOnly.MinValue!;
            try
            {
                if (model != null)
                {
                    if (model.GetType().GetProperty(propertyName)?.GetValue(model, null) is DateTime dateTimeValue)
                    {
                        propertyValue = DateOnly.FromDateTime(dateTimeValue);
                        isInbetween = propertyValue >= fromDate && propertyValue <= toDate;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDateTimePropertyValue<T>(this T model, string propertyName): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return isInbetween;
        }
    }
}
