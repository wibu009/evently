using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Evently.Modules.Ticketing.Infrastructure.Database;

/// <summary>
/// Exposes the underlying provider transaction to callers while keeping the EF Core
/// transaction tracking in sync. Committing, rolling back, or disposing this instance goes
/// through the <see cref="IDbContextTransaction"/>, so the context never ends up with a
/// stale transaction that a subsequent <c>SaveChanges</c> would try to enlist in.
/// </summary>
internal sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : DbTransaction
{
    private readonly DbTransaction _transaction = transaction.GetDbTransaction();

    protected override DbConnection DbConnection => _transaction.Connection!;

    public override IsolationLevel IsolationLevel => _transaction.IsolationLevel;

    public override void Commit() => transaction.Commit();

    public override Task CommitAsync(CancellationToken cancellationToken = default) =>
        transaction.CommitAsync(cancellationToken);

    public override void Rollback() => transaction.Rollback();

    public override Task RollbackAsync(CancellationToken cancellationToken = default) =>
        transaction.RollbackAsync(cancellationToken);

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                // Disposing without an explicit commit rolls the transaction back,
                // which is the expected behavior for handlers that return early.
                transaction.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await transaction.DisposeAsync();
        }
        finally
        {
            await base.DisposeAsync();
        }
    }
}
