using DBGuard.Contracts.Models.GalliumData.Filters;
using DBGuard.DataAccess.Data.Entities;

namespace DBGuard.BLL.Interfaces.Services;

public interface IGalliumRepositoryService
{
    Task InitProject(List<Rule> rules);

    Task UpdateFilters(List<BaseFilterDto> filters);
}