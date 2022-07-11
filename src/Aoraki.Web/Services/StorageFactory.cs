using System.Diagnostics.CodeAnalysis;
using Aoraki.Web.Contracts;
using Aoraki.Web.Options;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace Aoraki.Web.Services;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class StorageFactory : IStorageFactory
{
    private readonly ConnectionStrings _connectionStrings;

    public StorageFactory(IOptions<ConnectionStrings> storageOptions)
    {
        _connectionStrings = storageOptions.Value;
    }

    public TableClient GetTableClient(string tableName) => new(_connectionStrings.Storage, tableName);
}