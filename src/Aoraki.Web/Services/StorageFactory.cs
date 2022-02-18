using System;
using System.Diagnostics.CodeAnalysis;
using Aoraki.Web.Contracts;
using Aoraki.Web.Options;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace Aoraki.Web.Services;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class StorageFactory : IStorageFactory
{
    private readonly StorageOptions _storageOptions;

    public StorageFactory(IOptions<StorageOptions> storageOptions)
    {
        _storageOptions = storageOptions.Value;
    }

    public TableClient GetTableClient(string tableName) => new(
        new Uri($"https://{_storageOptions.AccountName}.table.core.windows.net/"),
        tableName, new DefaultAzureCredential());
}