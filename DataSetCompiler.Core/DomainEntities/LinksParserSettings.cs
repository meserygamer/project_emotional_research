using System.ComponentModel.DataAnnotations;

namespace DataSetCompiler.Core.DomainEntities;

public record LinksParserOptions ()
{
    [Range(0, int.MaxValue)]
    public int? MaxLinksCount { get; set; } = null;

    [Range(0, int.MaxValue)]
    public int NumberOfLinksSkipped { get; set; } = 0;
}