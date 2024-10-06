using Moq;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class MockExtensions
    {
        /// <summary>
        /// Creates a mock for a specific closed generic type.
        /// </summary>
        /// <typeparam name="T">The open generic type definition.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="typeArguments">The type arguments to close the generic type.</param>
        /// <returns>A mock of the specified closed generic type.</returns>
        public static Mock<T> CreateMockForGenericType<T>(this Mock<T> mock, params Type[] typeArguments) where T : class
        {
            if (typeArguments == null || !typeArguments.Any())
            {
                throw new ArgumentException("Type arguments cannot be null or empty.", nameof(typeArguments));
            }

            // Get the open generic type definition
            Type openGenericType = typeof(T).GetGenericTypeDefinition();
            if (openGenericType == null)
            {
                throw new InvalidOperationException("Unable to get the open generic type definition.");
            }

            // Create the closed generic type using the type arguments
            Type closedGenericType;
            try
            {
                closedGenericType = openGenericType.MakeGenericType(typeArguments);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create the closed generic type.", ex);
            }

            // Create and return a mock for the closed generic type
            try
            {
                Type mockType = typeof(Mock<>).MakeGenericType(closedGenericType);
                object? mockInstance = Activator.CreateInstance(mockType);

                if (mockInstance == null)
                {
                    throw new InvalidOperationException("The created mock instance is null.");
                }

                return (Mock<T>)mockInstance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create an instance of the mock for the closed generic type.", ex);
            }
        }
    }

}
