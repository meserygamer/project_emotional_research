using System.Text.Json;
using DataSetCompiler.Core.DomainEntities;

namespace DataSetCompiler.Core.Interfaces;

public interface ILinkParser
{
    Task<List<string>> GetLinksAsync(LinksParserOptions options);
    
    Task<List<string>> GetLinksWithPrintAsync(LinksParserOptions options,
        JsonSerializerOptions? serializerOptions,
        string outputFile);
}