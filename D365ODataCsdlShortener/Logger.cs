// Copyright (c) Oleksandr Dudarenko. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace D365ODataCsdlShortener
{
    public class Logger
    {
        public static ILoggerFactory ConfigureLogger(LogLevel logLevel)
        {
            // Configure logger options
#if DEBUG
            logLevel = logLevel > LogLevel.Debug ? LogLevel.Debug : logLevel;
#endif

            return LoggerFactory.Create((builder) =>
            {
                builder
                    .AddSimpleConsole(c =>
                    {
                        c.IncludeScopes = true;
                    })
#if DEBUG   
                    .AddDebug()
#endif
                    .SetMinimumLevel(logLevel);
            });
        }
    }
}
