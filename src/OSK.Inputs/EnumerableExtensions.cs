using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace OSK.Inputs;

public static class EnumerableExtensions
{
    public static T? FirstOrDefaultByString<T>(this IEnumerable<T> enumerable, Func<T, string> getString, string str,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return enumerable.FirstOrDefault(item => string.Equals(getString(item), str, comparison));
    }

    public static T FirstByString<T>(this IEnumerable<T> enumerable, Func<T, string> getString, string str,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return enumerable.First(item => string.Equals(getString(item), str, comparison))!;
    }

    public static bool AnyByString<T>(this IEnumerable<T> enumerable, Func<T, string> getString, string str,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return enumerable.Any(item => string.Equals(getString(item), str, comparison))!;
    }

    #region Concurrent Execution

    public static Task<IEnumerable<TResult>> ExecuteConcurrentResultAsync<T, TResult>(this IEnumerable<T> source,
        Func<T, Task<TResult>> action, int maxDegreesOfConcrruency, CancellationToken cancellationToken = default)
        => source.ExecuteConcurrentResultAsync(action, maxDegreesOfConcrruency, false, cancellationToken);

    public static async Task<IEnumerable<TResult>> ExecuteConcurrentResultAsync<T, TResult>(this IEnumerable<T> source,
        Func<T, Task<TResult>> action, int maxDegreesOfConcrruency, bool isParallel, CancellationToken cancellationToken = default)
    {
        var taskListResult = await source.ExecuteConcurrentAsync(action, maxDegreesOfConcrruency, isParallel, cancellationToken);
        return taskListResult.Select(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                return task.Result;
            }

            throw task.Exception ?? throw new InvalidOperationException("An unknown error prevented task execution.");
        });
    }

    public static Task<IList<Task<TResult>>> ExecuteConcurrentAsync<T, TResult>(this IEnumerable<T> source,
        Func<T, Task<TResult>> action, int maxDegreesOfConcrruency, CancellationToken cancellationToken = default)
        => source.ExecuteConcurrentAsync(action, maxDegreesOfConcrruency, false, cancellationToken);

    public static async Task<IList<Task<TResult>>> ExecuteConcurrentAsync<T, TResult>(this IEnumerable<T> source,
        Func<T, Task<TResult>> action, int maxDegreesOfConcrruency, bool isParallel, CancellationToken cancellationToken = default)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (maxDegreesOfConcrruency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDegreesOfConcrruency));
        }

        Task<TResult> ExecuteActionAsync(T value, bool isParallel)
        {
            return isParallel
                ? Task<Task<TResult>>.Factory.StartNew(state => action((T)state), value, cancellationToken,
                    TaskCreationOptions.DenyChildAttach | TaskCreationOptions.PreferFairness, TaskScheduler.Default)
                    .Unwrap()
                : action(value);
        }

        if (source is IList<T> list && list.Count <= maxDegreesOfConcrruency)
        {
            if (list.Count == 0)
            {
                return Array.Empty<Task<TResult>>();
            }

            return source.Select(item => ExecuteActionAsync(item, isParallel)).ToList();
        }

        var semaphore = new SemaphoreSlim(maxDegreesOfConcrruency);
        var results = new List<Task<TResult>>();
        foreach (var item in source)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            var result = ExecuteActionAsync(item, isParallel);
            results.Add(result);
            _ = result.ContinueWith(_ => semaphore.Release(), cancellationToken);
        }

        try
        {
            await Task.WhenAll(results).ConfigureAwait(false);
        }
        catch
        {

        }

        return results;
    }

    #endregion
}
