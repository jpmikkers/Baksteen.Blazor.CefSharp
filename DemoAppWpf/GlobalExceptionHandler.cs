using System.Net.Http;
using System.Reflection;

namespace DemoAppWpf;

public class GlobalExceptionHandler
{
    private GlobalExceptionHandler() { } // Do not allow to make instance of class

    public static void ProcessException(object? _, Exception exception)
    {
        try
        {
            exception = exception switch
            {
                HttpRequestException httpRequestException => httpRequestException.InnerException ?? httpRequestException,
                TargetInvocationException targetInvocationException => exception.InnerException ?? exception,
                AggregateException aggregateException => aggregateException.InnerExceptions.FirstOrDefault() ?? exception,
                _ => exception
            };

            //ignore task canceled exception
            if (exception is TaskCanceledException)
                return;

            return;
        }
        catch
        {
            //do nothing
        }
    }
}
