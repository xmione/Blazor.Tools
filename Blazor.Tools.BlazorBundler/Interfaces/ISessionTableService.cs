using Blazor.Tools.BlazorBundler.Entities;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ISessionTableService : ICommonService<SessionTable, ISessionTable, IReportItem>
    {
        Task UploadTableListAsync(List<TargetTable> model);
    }
}
