using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Blazor.Tools.Components.TableGrid
{
    public static class RenderFragmentExtensions
    {
        public static string RenderToHtmlString(this RenderFragment fragment)
        {
            var result = string.Empty;

            // Create a RenderTreeBuilder
            var builder = new RenderTreeBuilder();

            // Render the fragment into the builder
            fragment(builder);

            // Use StringWriter to capture the output
            using (var writer = new StringWriter())
            {
                // Iterate over the frames in the builder to capture the output
                var frames = builder.GetFrames();
                int frameCount = frames.Count;
                for (int i = 0; i < frameCount; i++)
                {
                    var frame = frames.Array[i];

                    if (frame.FrameType == RenderTreeFrameType.Component)
                    {
                        var componentFrame = (RenderTreeFrame)frame;
                        var renderFragment = new RenderFragment(builder =>
                        {
                            builder.AddMarkupContent(0, componentFrame.MarkupContent);
                        });
                     
                    }
                }

                // Convert StringWriter to string
                result = writer.ToString();
            }

            return result;
        }
    }

}
