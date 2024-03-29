﻿using Nest;

using TheArchives.Server.Models.Elastic;

namespace TheArchives.Server.Extensions
{
    public static class IElasticClientExtensions
    {
        /// <summary>
        /// Creates a custom index using the NEST Fluent Mapping syntax
        /// </summary>
        public async static Task<CreateIndexResponse> CreateCustomIndex(this IElasticClient elasticClient, string? indexName, CancellationToken cancellationToken = default) =>
            await elasticClient.Indices.CreateAsync(indexName, createIndex => createIndex
                .Settings(settings => settings
                    .NumberOfReplicas(0)
                    .NumberOfShards(1)
                    .Analysis(analysis => analysis
                        .Analyzers(analyzers => analyzers
                            .Custom("custom_analyzer", customAnalyzer => customAnalyzer
                                .CharFilters("html_strip")
                                .Tokenizer("standard")
                                .Filters("lowercase", "stop", "snowball")
                            )
                        )
                    )
                )
                .Map<Content>(m => m
                    .Properties(ps => ps
                        .Text(s => s
                            .Name(n => n.Title)
                            .Analyzer("custom_analyzer")
                            .Boost(8)
                        )
                        .Text(s => s
                            .Name(n => n.Description)
                            .Analyzer("custom_analyzer")
                        )
                        .Text(s => s
                            .Name(n => n.Keywords)
                            .Boost(5)
                        )
                        .Text(s => s
                            .Name(n => n.Path)
                            .Index(false)
                        )
                        .Keyword(s => s
                            .Name(n => n.Author)
                            .Boost(3)
                        )
                        .Keyword(s => s
                            .Name(n => n.Brand)
                            .Boost(2)
                        )
                        .Keyword(s => s
                            .Name(n => n.Tags)
                            .Boost(13)
                        )
                    )
                ),
                cancellationToken
            );
    }
}