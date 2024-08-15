using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using DataSetCompiler.Core.DomainEntities;
using DataSetCompiler.Core.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumStealth.NET.Clients.Extensions;

namespace KinopoiskFilmReviewsParser.Parsers.LinksParsers;

public class KinopoiskTopBestFilmsLinksParser : ILinkParser
{
    private const int NumberFilmLinksOnPage = 200;
    
    
    private readonly string _postfixOfNumberOfMoviesPerPageUrl = $"perpage/{NumberFilmLinksOnPage}/";
    
    private IWebDriver _webDriver;
    
    
    public KinopoiskTopBestFilmsLinksParser(IWebDriver webDriver, BestFilmsSearchQuery filmsSearchQuery)
    {
        _webDriver = webDriver ?? throw new ArgumentNullException(nameof(webDriver));
        FilmsSearchQuery = filmsSearchQuery ?? throw new ArgumentNullException(nameof(filmsSearchQuery));
    }
    
    
    public async Task<List<string>> GetLinksAsync(LinksParserOptions options)
    {
        ValidateLinkParserOptions(options);
        int firstPageNumber = CalculateNumberFirstPageToParse(options.NumberOfLinksSkipped);
        int numberOfPages = await CalculateNumberOfPagesToParseAsync(options.NumberOfLinksSkipped,
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
        string outputFile = "TopControversialFilmsLinks.json")
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


    public BestFilmsSearchQuery FilmsSearchQuery { get; }

    private string MainUrlOfFilmsSearchResults => FilmsSearchQuery.Url + _postfixOfNumberOfMoviesPerPageUrl;


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
    
    private int CalculateNumberFirstPageToParse(int numberOfLinksSkipped)
        => numberOfLinksSkipped / NumberFilmLinksOnPage + 1;
    
    private async Task<int> CalculateNumberOfPagesToParseAsync(int numberOfLinksSkipped, int? maxLinksCount)
    {
        if (maxLinksCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxLinksCount));

        int filmsNumber = await GetFilmsNumberAsync();
        int useFullFilmsNumber = filmsNumber - numberOfLinksSkipped;
        if (useFullFilmsNumber <= 0)
            return 0;
        
        useFullFilmsNumber = maxLinksCount is null 
            ? useFullFilmsNumber 
            : Math.Min(useFullFilmsNumber, (int)maxLinksCount);
        
        return (int)Math.Ceiling((decimal)useFullFilmsNumber / NumberFilmLinksOnPage);
    }

    private async Task<List<string>> GetFilmLinksFromPageAsync(int pageNumber)
    {
        List<string> filmLinks = new();
        
        await GoToFilmsTopPageAsync(pageNumber);
        return new SeleniumDomExceptionHandler().MakeManyRequestsForDom(() =>
        {
            List<string> filmLinks = new List<string>();
            ReadOnlyCollection<IWebElement> filmsLinksElements = _webDriver.FindElement(By.Id("itemList"))
                .FindElements(By.ClassName("js-ott-widget"));
            foreach (var filmLinkElement in filmsLinksElements)
                filmLinks.Add($"https://www.kinopoisk.ru/film/{filmLinkElement.GetAttribute("data-kp-film-id")}/");
            return filmLinks;
        });
    }
    
    private async Task GoToFilmsTopPageAsync(int pageNumber = 0)
    {
        if(pageNumber <= 0)
            await _webDriver.Navigate().GoToUrlAsync(MainUrlOfFilmsSearchResults);
        else
            await _webDriver.Navigate().GoToUrlAsync(MainUrlOfFilmsSearchResults + $"page/{pageNumber}/");
        
        WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(40));
        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("itemList")));
        _webDriver.SpecialWait(new Random().Next(2000, 3000));
    }

    private async Task<int> GetFilmsNumberAsync()
    {
        await GoToFilmsTopPageAsync();

        string pageData = new SeleniumDomExceptionHandler()
            .MakeManyRequestsForDom(() => _webDriver.FindElement(By.ClassName("pagesFromTo")).Text);
        return Convert.ToInt32(pageData.Split(" ")[^1]);
    }
}