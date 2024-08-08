using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using System.Core.Extensions;
using System.Diagnostics;
using System.Extensions;
using System.Linq.Expressions;

namespace GuildWarsPartySearch.Server.Services.Azure;

public class NamedTableClient<TOptions> : TableClient
{
    private readonly ILogger<NamedTableClient<TOptions>> logger;

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, Uri endpoint, TableClientOptions? options = null) : base(endpoint, options)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, string connectionString, string tableName) : base(connectionString, tableName)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, Uri endpoint, AzureSasCredential credential, TableClientOptions? options = null) : base(endpoint, credential, options)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, Uri endpoint, string tableName, TableSharedKeyCredential credential) : base(endpoint, tableName, credential)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, string connectionString, string tableName, TableClientOptions? options = null) : base(connectionString, tableName, options)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, Uri endpoint, string tableName, TableSharedKeyCredential credential, TableClientOptions? options = null) : base(endpoint, tableName, credential, options)
    {
        this.logger = logger.ThrowIfNull();
    }

    public NamedTableClient(ILogger<NamedTableClient<TOptions>> logger, Uri endpoint, string tableName, TokenCredential tokenCredential, TableClientOptions? options = null) : base(endpoint, tableName, tokenCredential, options)
    {
        this.logger = logger.ThrowIfNull();
    }

    public override Response<TableItem> Create(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.Create(cancellationToken), nameof(this.Create));
    }

    public override Response<TableItem> CreateIfNotExists(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.CreateIfNotExists(cancellationToken), nameof(this.CreateIfNotExists));
    }

    public override Response AddEntity<T>(T entity, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.AddEntity(entity, cancellationToken), nameof(this.AddEntity));
    }

    public override Response Delete(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.Delete(cancellationToken), nameof(this.Delete));
    }

    public override Response DeleteEntity(string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.DeleteEntity(partitionKey, rowKey, ifMatch, cancellationToken), nameof(this.DeleteEntity));
    }

    public override Response UpdateEntity<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.UpdateEntity(entity, ifMatch, mode, cancellationToken), nameof(this.UpdateEntity));
    }

    public override Response UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.UpsertEntity(entity, mode, cancellationToken), nameof(this.UpsertEntity));
    }

    public override Response<T> GetEntity<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.GetEntity<T>(partitionKey, rowKey, select, cancellationToken), nameof(this.GetEntity));
    }

    public override NullableResponse<T> GetEntityIfExists<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.GetEntityIfExists<T>(partitionKey, rowKey, select, cancellationToken), nameof(GetEntityIfExists));
    }

    public override Pageable<T> Query<T>(string? filter = null, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.Query<T>(filter, maxPerPage, select, cancellationToken), nameof(this.Query));
    }

    public override Pageable<T> Query<T>(Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.Query(filter, maxPerPage, select, cancellationToken), nameof(this.Query));
    }

    public override Response<IReadOnlyList<Response>> SubmitTransaction(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.SubmitTransaction(transactionActions, cancellationToken), nameof(this.SubmitTransaction));
    }

    public override Task<Response<TableItem>> CreateAsync(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.CreateAsync(cancellationToken), nameof(this.CreateAsync));
    }

    public override Task<Response<TableItem>> CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.CreateIfNotExistsAsync(cancellationToken), nameof(this.CreateIfNotExistsAsync));
    }

    public override Task<Response> AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.AddEntityAsync(entity, cancellationToken), nameof(this.AddEntityAsync));
    }

    public override Task<Response> DeleteAsync(CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.DeleteAsync(cancellationToken), nameof(this.DeleteAsync));
    }

    public override Task<Response> DeleteEntityAsync(string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.DeleteEntityAsync(partitionKey, rowKey, ifMatch, cancellationToken), nameof(this.DeleteEntityAsync));
    }

    public override Task<Response> UpdateEntityAsync<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken), nameof(this.UpdateEntityAsync));
    }

    public override Task<Response> UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.UpsertEntityAsync(entity, mode, cancellationToken), nameof(this.UpsertEntityAsync));
    }

    public override Task<Response<T>> GetEntityAsync<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.GetEntityAsync<T>(partitionKey, rowKey, select, cancellationToken), nameof(this.GetEntityAsync));
    }

    public override Task<NullableResponse<T>> GetEntityIfExistsAsync<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.GetEntityIfExistsAsync<T>(partitionKey, rowKey, select, cancellationToken), nameof(this.GetEntityIfExistsAsync));
    }

    public override AsyncPageable<T> QueryAsync<T>(Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.QueryAsync(filter, maxPerPage, select, cancellationToken), nameof(this.QueryAsync));
    }

    public override AsyncPageable<T> QueryAsync<T>(string? filter = null, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.QueryAsync<T>(filter, maxPerPage, select, cancellationToken), nameof(this.QueryAsync));
    }

    public override Task<Response<IReadOnlyList<Response>>> SubmitTransactionAsync(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        return this.LogOperation(() => base.SubmitTransactionAsync(transactionActions, cancellationToken), nameof(this.SubmitTransactionAsync));
    }

    private Response LogOperation(Func<Response> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch(RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private Response<T> LogOperation<T>(Func<Response<T>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.GetRawResponse().Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private NullableResponse<T> LogOperation<T>(Func<NullableResponse<T>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.GetRawResponse().Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private Pageable<T> LogOperation<T>(Func<Pageable<T>> func, string operationName)
        where T : notnull
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        var response = func();
        return new InterceptingPageable<T>(response, page =>
        {
            scopedLogger.LogInformation("<< {0} {1}ms", page.GetRawResponse().Status, sw.ElapsedMilliseconds);
        }, () =>
        {
            scopedLogger.LogInformation("<< 200 {1}ms", sw.ElapsedMilliseconds);
        });
    }
    
    private Response<IReadOnlyList<Response>> LogOperation(Func<Response<IReadOnlyList<Response>>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = func();
            foreach (var action in response.Value)
            {
                scopedLogger.LogInformation("<< {0} {1}ms", action.Status, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<Response> LogOperation(Func<Task<Response>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = await func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<Response<T>> LogOperation<T>(Func<Task<Response<T>>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = await func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.GetRawResponse().Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<NullableResponse<T>> LogOperation<T>(Func<Task<NullableResponse<T>>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = await func();
            scopedLogger.LogInformation("<< {0} {1}ms", response.GetRawResponse().Status, sw.ElapsedMilliseconds);
            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private AsyncPageable<T> LogOperation<T>(Func<AsyncPageable<T>> func, string operationName)
        where T : notnull
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        var response = func();
        return new InterceptingAsyncPageable<T>(response, page =>
        {
            scopedLogger.LogInformation("<< {0} {1}ms", page.GetRawResponse().Status, sw.ElapsedMilliseconds);
        }, () =>
        {
            scopedLogger.LogInformation("<< 200 {1}ms", sw.ElapsedMilliseconds);
        });
    }

    private async Task<Response<IReadOnlyList<Response>>> LogOperation(Func<Task<Response<IReadOnlyList<Response>>>> func, string operationName)
    {
        var scopedLogger = this.logger.CreateScopedLogger(operationName, string.Empty);
        var sw = Stopwatch.StartNew();
        scopedLogger.LogInformation(">> {0}", this.Uri);
        try
        {
            var response = await func();
            foreach (var action in response.Value)
            {
                scopedLogger.LogInformation("<< {0} {1}ms", action.Status, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (RequestFailedException ex)
        {
            scopedLogger.LogInformation("<< {0} {1}ms", ex.Status, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
