using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using DataSetCompiler.Core.DomainEntities;
using DataSetCompiler.Core.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V85.DOM;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumStealth.NET.Clients.Extensions;

namespace KinopoiskFilmReviewsParser.Parsers.LinksParsers;

public class KinopoiskTop500FilmsLinksParser : ILinkParser
{
    #region Constants

    public const string FilmsTop500KinopoiskUrl = "https://www.kinopoisk.ru/lists/movies/top500/?sort=votes";

    private const int NumberFilms = 500;
    private const int NumberFilmLinksOnPage = 50;

    #endregion
    
    
    #region Fields

    private readonly IWebDriver _webDriver;

    #endregion


    #region Constructors

    public KinopoiskTop500FilmsLinksParser(Func<IWebDriver> webDriverFactory)
    {
        _webDriver = webDriverFactory?.Invoke() 
                     ?? throw new ArgumentNullException(nameof(webDriverFactory));
    }

    #endregion


    #region ILinkParser

    public async Task<List<string>> GetLinksAsync(LinksParserOptions options)
    {
        ValidateLinkParserOptions(options);
        int firstPageNumber = CalculateNumberFirstPageToParse(options.NumberOfLinksSkipped);
        int numberOfPages = CalculateNumberOfPagesToParse(options.NumberOfLinksSkipped,
            options.MaxLinksCount);
        List<string> filmLinks = new();
        
        for (int i = firstPageNumber; i <= numberOfPages + (firstPageNumber - 1); i++)
            filmLinks.AddRange(await GetFilmLinksFromPageAsync(i));
        
        int numberOfUnnecessaryLinks = options.NumberOfLinksSkipped % NumberFilmLinksOnPage;
        return options.MaxLinksCount is null 
            ? filmLinks[numberOfUnnecessaryLinks..] 
            : filmLinks[numberOfUnnecessaryLinks..((int)options.MaxLinksCount + numberOfUnnecessaryLinks)];
    }

    public async Task<List<string>> GetLinksWithPrintAsync(LinksParserOptions options,
        JsonSerializerOptions? serializerOptions = null,
        string outputFile = "LinksOnFilms.json")
    {
        if (String.IsNullOrEmpty(outputFile))
            throw new ArgumentException("Path to file was incorrect", nameof(outputFile));

        List<string> links = await GetLinksAsync(options);
        
        string linksJson = await Task.Run(() => JsonSerializer.Serialize(links, serializerOptions));
        using (var fs = new FileStream(outputFile, FileMode.Create))
        {
            byte[] buffer = Encoding.UTF8.GetBytes(linksJson);
            await fs.WriteAsync(buffer, 0, buffer.Length);
        }

        return links;
    }

    #endregion


    #region Methods

    private void ValidateLinkParserOptions(LinksParserOptions options)
    {
        try
        {
            ValidationContext context = new ValidationContext(options);
            Validator.ValidateObject(options, context, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<List<string>> GetFilmLinksFromPageAsync(int pageNumber)
    {
        List<string> filmLinks = new();
        
        await GoToFilmsTopPageAsync(pageNumber);

        return new SeleniumDomExceptionHandler().MakeManyRequestsForDom(() =>
        {
            List<string> filmLinks = new List<string>();
            ReadOnlyCollection<IWebElement> filmsLinksElements = _webDriver.FindElements(By.ClassName("styles_root__wgbNq"));
            
            foreach (var filmLinkElement in filmsLinksElements)
                filmLinks.Add(filmLinkElement.GetAttribute("href"));
            return filmLinks;
        });
    }

    private async Task GoToFilmsTopPageAsync(int pageNumber)
    {
        await _webDriver.Navigate().GoToUrlAsync($"{FilmsTop500KinopoiskUrl}&page={pageNumber}");
        WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(40));
        wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("styles_root__ti07r")));
        _webDriver.SpecialWait(new Random().Next(2000, 3000));
    }
    
    private int CalculateNumberOfPagesToParse(int numberOfLinksSkipped, int? maxLinksCount)
    {
        if (maxLinksCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxLinksCount));

        int filmsNumber = NumberFilms - numberOfLinksSkipped;
        filmsNumber = maxLinksCount is null ? filmsNumber : Math.Min(filmsNumber, (int)maxLinksCount);
        return (int)Math.Ceiling((decimal)filmsNumber / NumberFilmLinksOnPage);
    }
    
    private int CalculateNumberFirstPageToParse(int numberOfLinksSkipped)
        => numberOfLinksSkipped / NumberFilmLinksOnPage + 1;

    #endregion
}