using DBGuard.Common.Models;
using DBGuard.Contracts.Models.GalliumData.Filters;

namespace DBGuard.BLL.Interfaces.Helpers;

public interface IProjectStructureFactory
{
    public void CreateProjectStructure(
        string basePath,
        ProjectModel project,
        CryptoModel crypto,
        ConnectionModel connection,
        List<BaseFilterDto> filters);
}