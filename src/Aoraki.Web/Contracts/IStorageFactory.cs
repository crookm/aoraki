using Azure.Data.Tables;

namespace Aoraki.Web.Contracts;

public interface IStorageFactory
{
    /// <summary>
    /// Constructs a table client
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>A table client</returns>
    TableClient GetTableClient(string tableName);
}