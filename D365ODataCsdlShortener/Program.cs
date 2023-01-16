// Copyright (c) Oleksandr Dudarenko. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;
using D365ODataCsdlShortener.Handlers;

namespace D365ODataCsdlShortener
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var csdlSourceFilePathOption = new Option<string>("--csdlSourceFilePath", "- a CSDL file path in the local filesystem or a valid URL hosted on an HTTPS server");
            csdlSourceFilePathOption.AddAlias("-cs");

            var csdlEntitySetFilterOption = new Option<string>("--csdlEntitySetFilter", "- a filter parameter that user can use to select a subset of data entities from a large CSDL file. By providing a comma separated list of EntitySet names that appear in the EntityContainer");
            csdlEntitySetFilterOption.AddAlias("-csfe");

            var csdlActionFilterOption = new Option<string>("--csdlActionFilter", "- a filter parameter that user can use to select a subset of data entity actions from a large CSDL file.By providing a comma separated list of Action names");
            csdlActionFilterOption.AddAlias("-csfa");

            var csdlTransformedFilePathOption = new Option<FileInfo>("--csdlTransformedFilePath", "- an output directory path for the transformed file") { Arity = ArgumentArity.ZeroOrOne };
            csdlTransformedFilePathOption.AddAlias("-o");

            var logLevelOption = new Option<LogLevel>("--log-level", () => LogLevel.Information, "- the log level to use when logging messages to the main output");
            logLevelOption.AddAlias("-ll");


            var rootCommand = new RootCommand()
            {
                csdlSourceFilePathOption,
                csdlEntitySetFilterOption,
                csdlActionFilterOption,
                csdlTransformedFilePathOption,
                logLevelOption
            };

            rootCommand.Handler = new CommandHandler()
            {
                CsdlSourceFilePath = csdlSourceFilePathOption,
                CsdlEntitySetFilter = csdlEntitySetFilterOption,
                CsdlActionFilter = csdlActionFilterOption,
                CsdlTransformedFilePath = csdlTransformedFilePathOption,
                LogLevelOption = logLevelOption
            };

            rootCommand.Description = @"Dynamics 365 Finance / Supply Chain Management (D365F&O/SCM) OData CSDL Shortener is a command line tool that makes it easy to make a CSDL file shorter.";

            // Parse the incoming args and invoke the handler
            await rootCommand.InvokeAsync(args);

            //// Wait for logger to write messages to the console before exiting
            await Task.Delay(10);
        }        
    }
}
