using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public static class RenderFragmentExtensions
    {
        public static string RenderToHtmlString(this RenderFragment fragment)
        {
            var result = string.Empty;

            // Create a RenderTreeBuilder to build the render tree
            var builder = new RenderTreeBuilder();

            // Render the fragment into the builder
            fragment(builder);

            // Use StringWriter to capture the output
            using (var writer = new StringWriter())
            {
                // Get the frames from the builder
                var frames = builder.GetFrames();
                int frameCount = frames.Count;

                // Iterate over the frames to construct the HTML string
                for (int i = 0; i < frameCount; i++)
                {
                    var frame = frames.Array[i];

                    // Capture only MarkupContent
                    if (frame.FrameType == RenderTreeFrameType.Component)
                    {
                        writer.Write(frame.MarkupContent);
                    }
                }

                // Return the captured HTML string
                result = writer.ToString();
            }

            return result;
        }
    }
}
