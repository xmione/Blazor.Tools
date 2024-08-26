using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Components.Grid;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class ComponentExtensions
    {
        public static Dictionary<string, T> ExtractComponents<T>(this List<RenderFragment> fragments, Func<T, string> keySelector) where T : class, IComponent
        {
            var components = new Dictionary<string, T>();

            foreach (var fragment in fragments)
            {
                var htmlString = fragment.RenderToHtmlString();
                var builder = new RenderTreeBuilder();
                fragment(builder);

                // Parse the builder to extract components of type T
                ParseComponents(builder, components, keySelector);
            }

            return components;
        }

        private static void ParseComponents<T>(RenderTreeBuilder builder, Dictionary<string, T> components, Func<T, string> keySelector) where T : class, IComponent
        {
            var frames = builder.GetFrames();
            int frameCount = frames.Count;

            // Iterate over the frames to construct the HTML string
            for (int i = 0; i < frameCount; i++)
            {
                var frame = frames.Array[i];
                if (frame.FrameType == RenderTreeFrameType.Component)
                {
                    var component = GetComponentInstance<T>(frame);
                    if (component != null)
                    {
                        var key = keySelector(component);
                        components[key] = component;
                    }
                }
            }
        }

        private static T? GetComponentInstance<T>(RenderTreeFrame frame) where T : class, IComponent
        {
            // Accessing private members of RenderTreeFrame to get component instance
            var componentInstanceField = typeof(RenderTreeFrame).GetField("_component", BindingFlags.NonPublic | BindingFlags.Instance);
            if (componentInstanceField == null)
            {
                //throw new InvalidOperationException("Unable to find _component field in RenderTreeFrame.");
                Console.WriteLine("Unable to find _component field in RenderTreeFrame.");
            }

            var component = componentInstanceField?.GetValue(frame) as T;
            return component;
        }
    }
}
