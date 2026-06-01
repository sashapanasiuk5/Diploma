using DBGuard.Contracts.Models.GalliumData;
using DBGuard.Contracts.Models.GalliumData.Filters;

namespace DBGuard.BLL.Helpers;

public static class FilterPathHelper
{
    public static (string filterGroup, string filterFileName) GetPathParts(FilterGroup filterGroup)
    {
        string groupFolderName;
        string jsonFileName;
        
        switch (filterGroup)
        {
            case FilterGroup.Request:
                groupFolderName = "request_filters";
                jsonFileName = "request_filter.json";
                break;

            case FilterGroup.Response:
                groupFolderName = "response_filters";
                jsonFileName = "response_filter.json";
                break;

            case FilterGroup.Connection:
                groupFolderName = "connection_filters";
                jsonFileName = "connection_filter.json";
                break;

            case FilterGroup.Duplex:
                groupFolderName = "duplex_filters";
                jsonFileName = "duplex_filter.json";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(FilterGroup));
        }
        
        return (groupFolderName, jsonFileName);
    }
    
    public static string GetFilterJsonPath(
        string projectPath,
        BaseFilterDto filter)
    {
        var metadata = GetPathParts(filter.FilterGroup);

        return Path.Combine(
            projectPath,
            metadata.filterGroup,
            filter.FilterType,
            metadata.filterFileName);
    }
}