namespace Ratio.LogWork.Service
{
    public interface IExportData
    {
        Task ExportFileDataAsync<T>(IEnumerable<T> data, string filePath) where T : class;
    }
}
