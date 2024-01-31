// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Runtime.ExceptionServices;
using System.Windows;

namespace Baksteen.Blazor.CefSharpWPF;

/// <summary>
/// Dispatcher implementation for Windows Forms that invokes methods on the UI thread. The <see cref="Dispatcher"/>
/// class uses the async <see cref="Task"/> pattern so everything must be mapped from the <see cref="IAsyncResult"/>
/// pattern using techniques listed in https://docs.microsoft.com/dotnet/standard/asynchronous-programming-patterns/interop-with-other-asynchronous-patterns-and-types.
/// </summary>
public class BSWPFDispatcher : Dispatcher
{
    private static Action<Exception> RethrowException = exception => ExceptionDispatchInfo.Capture(exception).Throw();
    private readonly System.Windows.Threading.Dispatcher _dispatcher;

    /// <summary>
    /// Creates a new instance of <see cref="BSWPFDispatcher"/>.
    /// </summary>
    /// <param name="dispatchThreadControl">A control that was created on the thread from which UI dispatches must
    /// occur. This can typically be any control because all controls must have been created on the UI thread to
    /// begin with.</param>
    public BSWPFDispatcher(UIElement dispatchThreadControl)
    {
        if (dispatchThreadControl is null)
        {
            throw new ArgumentNullException(nameof(dispatchThreadControl));
        }

        _dispatcher = dispatchThreadControl.Dispatcher;
    }

    public override bool CheckAccess() => _dispatcher.Thread == Thread.CurrentThread;

    public override async Task InvokeAsync(Action workItem)
    {
        try
        {
            if (CheckAccess())
            {
                workItem();
            }
            else
            {
                await _dispatcher.InvokeAsync(workItem);
            }
        }
        catch (Exception ex)
        {
            // TODO: Determine whether this is the right kind of rethrowing pattern
            // You do have to do something like this otherwise unhandled exceptions
            // throw from inside Dispatcher.InvokeAsync are simply lost.
            _ = _dispatcher.BeginInvoke(RethrowException, ex);
            throw;
        }
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        try
        {
            if (CheckAccess())
            {
                return workItem();
            }
            else
            {
                return _dispatcher.InvokeAsync(async () => await workItem()).Task;
            }
        }
        catch (Exception ex)
        {
            // TODO: Determine whether this is the right kind of rethrowing pattern
            // You do have to do something like this otherwise unhandled exceptions
            // throw from inside Dispatcher.InvokeAsync are simply lost.
            _ = _dispatcher.BeginInvoke(RethrowException, ex);
            throw;
        }
    }

    public override async Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        try
        {
            if (CheckAccess())
            {
                return workItem();
            }
            else
            {
                return await _dispatcher.InvokeAsync(workItem);
            }
        }
        catch (Exception ex)
        {
            // TODO: Determine whether this is the right kind of rethrowing pattern
            // You do have to do something like this otherwise unhandled exceptions
            // throw from inside Dispatcher.InvokeAsync are simply lost.
            _ = _dispatcher.BeginInvoke(RethrowException, ex);
            throw;
        }
    }

    public override async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        try
        {
            if (CheckAccess())
            {
                return await workItem();
            }
            else
            {
                var t = await _dispatcher.InvokeAsync(workItem);
                return await t;
            }
        }
        catch (Exception ex)
        {
            // TODO: Determine whether this is the right kind of rethrowing pattern
            // You do have to do something like this otherwise unhandled exceptions
            // throw from inside Dispatcher.InvokeAsync are simply lost.
            _ = _dispatcher.BeginInvoke(RethrowException, ex);
            throw;
        }
    }
}
