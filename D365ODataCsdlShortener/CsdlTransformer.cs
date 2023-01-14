// Copyright (c) Oleksandr Dudarenko. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm;
using Microsoft.Extensions.Logging;

namespace D365ODataCsdlShortener
{
    public class CsdlTransformer
    {
        public static async Task TransformCsdlDocument(string csdlSourceFilePath, string csdlEntitySetFilter, string csdlActionFilter, FileInfo csdlTransformedFilePath, LogLevel logLevel, CancellationToken cancellationToken)
        {
            using var loggerFactory = Logger.ConfigureLogger(logLevel);
            var logger = loggerFactory.CreateLogger<CsdlTransformer>();
            try
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                int filterEntitiesCount = string.IsNullOrEmpty(csdlEntitySetFilter) ? 0 : csdlEntitySetFilter.Split(",").Count();
                int filterActionsCount = string.IsNullOrEmpty(csdlActionFilter) ? 0 : csdlActionFilter.Split(",").Count();
                Stream csdlSourceFileStream;
                Stream csdlTransformedFileStream;

                using (logger.BeginScope($"Read the input CSDL: {csdlSourceFilePath}", csdlSourceFilePath))
                {
                    csdlSourceFileStream = await GetStream(csdlSourceFilePath, logger, cancellationToken);
                    
                    logger.LogTrace($"Finished CSDL input read in {stopwatch.ElapsedMilliseconds}ms");
                }


                using (logger.BeginScope($"Convert CSDL with {filterEntitiesCount} entities and {filterActionsCount} actions", filterEntitiesCount, filterActionsCount))
                {
                    StreamReader inputStreamReader = new StreamReader(csdlSourceFileStream);
                    csdlTransformedFileStream = await ApplyFilter(inputStreamReader, csdlEntitySetFilter, csdlActionFilter, cancellationToken);

                    logger.LogTrace($"Finished CSDL filtration in {stopwatch.ElapsedMilliseconds}ms");
                }


                using (logger.BeginScope($"Output the filtered CSDL to {csdlTransformedFilePath}", csdlTransformedFilePath.FullName))
                {
                    using (var fileStream = File.Create(csdlTransformedFilePath.FullName))
                    {
                        csdlTransformedFileStream.Seek(0, SeekOrigin.Begin);
                        csdlTransformedFileStream.CopyTo(fileStream);
                    }

                    logger.LogTrace($"Finished filtered CSDL output in {stopwatch.ElapsedMilliseconds}ms");
                }

                stopwatch.Stop();
                logger.LogTrace("{timestamp}ms: Filtered CSDL with {entities} data entities and {actions} actions", stopwatch.ElapsedMilliseconds, filterEntitiesCount, filterActionsCount);
            }
            catch (TaskCanceledException)
            {
                Console.Error.WriteLine("CTRL+C pressed, aborting the operation.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not transform the document, reason: {ex.Message}", ex);
            }
        }

        protected static async Task<Stream> ApplyFilter(StreamReader inputStreamReader, string csdlEntitySetFilter, string csdlActionFilter, CancellationToken cancellationToken)
        {
            string csdlText = await inputStreamReader.ReadToEndAsync(cancellationToken);
            IEdmModel edmModel = CsdlReader.Parse(XElement.Parse(csdlText).CreateReader());

            HashSet<string> hashSetEntitySets = new HashSet<string>();
            HashSet<string> hashSetActions = new HashSet<string>();
            HashSet<string> entityTypes = new HashSet<string>();
            HashSet<string> entityFullTypes = new HashSet<string>();

            if (!string.IsNullOrEmpty(csdlEntitySetFilter))
            {
                hashSetEntitySets = new HashSet<string>(csdlEntitySetFilter.Split(",").ToList());
            }

            if (!string.IsNullOrEmpty(csdlActionFilter))
            {
                hashSetActions = new HashSet<string>(csdlActionFilter.Split(",").ToList());
            }

            foreach (var entitySet in edmModel.EntityContainer.EntitySets().Where(x => hashSetEntitySets.ToList().Contains(x.Name)))
            {
                IEdmEntityType edmEntityType = entitySet.EntityType();
                entityTypes.Add(edmEntityType.Name);
                entityFullTypes.Add(edmEntityType.FullTypeName());

                foreach (IEdmNavigationSource entitySetNavSource in entitySet.NavigationPropertyBindings.Select(x => x.Target))
                {
                    entityTypes.Add(entitySetNavSource.EntityType().Name);
                    entityFullTypes.Add(entitySetNavSource.EntityType().FullTypeName());
                }
            }


            var actions = edmModel.SchemaElements.OfType<IEdmAction>();
            foreach (var action in actions.Where(x => x.SchemaElementKind == EdmSchemaElementKind.Action && hashSetActions.ToList().Contains(x.Name)))
            {
                foreach (var actionParameterType in action.Parameters.Select(x => x.Type.Definition.AsElementType().FullTypeName()))
                {
                    entityTypes.Add(actionParameterType.Split(".").Last());
                    entityFullTypes.Add(actionParameterType);
                }

                var actionReturnType = action.ReturnType.Definition.AsElementType().FullTypeName();
                entityTypes.Add(actionReturnType.Split(".").Last());
                entityFullTypes.Add(actionReturnType);
            }

            string entitySetsStr = string.Join(",", hashSetEntitySets);
            string setActionsStr = string.Join(",", hashSetActions);
            string entityTypesStr = string.Join(",", entityTypes);
            string entityFullTypesStr = string.Join(",", entityFullTypes);

            Stream filteredStream = ApplyXslTransformation(csdlText, entitySetsStr, setActionsStr, entityTypesStr, entityFullTypesStr);

            return filteredStream;
        }

        protected static Stream ApplyXslTransformation(string csdlText, string entitySetsStr, string setActionsStr, string entityTypesStr, string entityFullTypesStr)
        {
            XmlReader inputXmlReader = XElement.Parse(csdlText).CreateReader();
            MemoryStream filteredStream = new();
            StreamWriter writer = new(filteredStream);

            XsltArgumentList args = new();
            args.AddParam("entitySets", "", entitySetsStr);
            args.AddParam("setActions", "", setActionsStr);
            args.AddParam("entityTypes", "", entityTypesStr);
            args.AddParam("entityFullTypes", "", entityFullTypesStr);

            XslCompiledTransform transform = GetKeepUsedTypesTransform();
            transform.Transform(inputXmlReader, args, writer);

            return filteredStream;
        }

        private static XslCompiledTransform GetKeepUsedTypesTransform()
        {
            XslCompiledTransform transform = new();
            Assembly assembly = typeof(CsdlTransformer).GetTypeInfo().Assembly;
            Stream xslt = assembly.GetManifestResourceStream("D365ODataCsdlShortener.CsdlCleanupOrphanedSchema.xslt");
            transform.Load(new XmlTextReader(new StreamReader(xslt)));
            return transform;
        }

        /// <summary>
        /// Reads stream from file system or makes HTTP request depending on the input string
        /// </summary>
        private static async Task<Stream> GetStream(string input, ILogger logger, CancellationToken cancellationToken)
        {
            Stream stream;
            using (logger.BeginScope("Reading input stream"))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                if (input.StartsWith("http"))
                {
                    try
                    {
                        var httpClientHandler = new HttpClientHandler()
                        {
                            SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                        };
                        using var httpClient = new HttpClient(httpClientHandler)
                        {
                            DefaultRequestVersion = HttpVersion.Version20
                        };
                        stream = await httpClient.GetStreamAsync(input, cancellationToken);
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new InvalidOperationException($"Could not download the file at {input}", ex);
                    }
                }
                else
                {
                    try
                    {
                        var fileInput = new FileInfo(input);
                        stream = fileInput.OpenRead();
                    }
                    catch (Exception ex) when (ex is FileNotFoundException ||
                        ex is PathTooLongException ||
                        ex is DirectoryNotFoundException ||
                        ex is IOException ||
                        ex is UnauthorizedAccessException ||
                        ex is SecurityException ||
                        ex is NotSupportedException)
                    {
                        throw new InvalidOperationException($"Could not open the file at {input}", ex);
                    }
                }
                stopwatch.Stop();
                logger.LogTrace("{timestamp}ms: Read file {input}", stopwatch.ElapsedMilliseconds, input);
            }
            return stream;
        }

        private static ILoggerFactory ConfigureLoggerInstance(LogLevel loglevel)
        {
            // Configure logger options
#if DEBUG
            loglevel = loglevel > LogLevel.Debug ? LogLevel.Debug : loglevel;
#endif

            return Microsoft.Extensions.Logging.LoggerFactory.Create((builder) => {
                builder
                    .AddSimpleConsole(c => {
                        c.IncludeScopes = true;
                    })
#if DEBUG   
                    .AddDebug()
#endif
                    .SetMinimumLevel(loglevel);
            });
        }
    }
}
