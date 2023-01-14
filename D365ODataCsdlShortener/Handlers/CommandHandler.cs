// Copyright (c) Oleksandr Dudarenko. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;

namespace D365ODataCsdlShortener.Handlers
{
    internal class CommandHandler : ICommandHandler
    {
        public Option<string> CsdlSourceFilePath { get; set; }
        public Option<string> CsdlEntitySetFilter { get; set; }
        public Option<string> CsdlActionFilter { get; set; }
        public Option<FileInfo> CsdlTransformedFilePath { get; set; }
        public Option<LogLevel> LogLevelOption { get; set; }

        public int Invoke(InvocationContext context)
        {
            return InvokeAsync(context).GetAwaiter().GetResult();
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            string sourceFilePath = context.ParseResult.GetValueForOption(CsdlSourceFilePath);
            string entitySetFilter = context.ParseResult.GetValueForOption(CsdlEntitySetFilter);
            string actionFilter = context.ParseResult.GetValueForOption(CsdlActionFilter);
            FileInfo transformedFilePath = context.ParseResult.GetValueForOption(CsdlTransformedFilePath);
            LogLevel logLevel = context.ParseResult.GetValueForOption(LogLevelOption);

            CancellationToken cancellationToken = (CancellationToken)context.BindingContext.GetService(typeof(CancellationToken));

            using var loggerFactory = Logger.ConfigureLogger(logLevel);
            var logger = loggerFactory.CreateLogger<CsdlTransformer>();
            try
            {
                await CsdlTransformer.TransformCsdlDocument(sourceFilePath, entitySetFilter, actionFilter, transformedFilePath, logLevel, cancellationToken);

                return 0;
            }
            catch (Exception ex)
            {
#if DEBUG
                logger.LogCritical(ex, ex.Message);
                throw; // so debug tools go straight to the source of the exception when attached
#else
                logger.LogCritical( ex.Message);
                return 1;
#endif
            }
        }
    }
}
