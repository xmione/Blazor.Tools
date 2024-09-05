using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;

namespace Blazor.Tools.BlazorBundler.SessionManagement.Interfaces
{
    public interface ISessionTableService : ICommonService<SessionTable, ISessionTable, IReportItem>
    {
        Task UploadTableListAsync(List<TargetTable> model);
    }
}
