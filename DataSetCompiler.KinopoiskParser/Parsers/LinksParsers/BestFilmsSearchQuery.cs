using System.Text;
using DataSetCompiler.Core.Interfaces;

namespace KinopoiskFilmReviewsParser.Parsers.LinksParsers;

public class BestFilmsSearchQuery
{
    private const string BaseUrl = "https://www.kinopoisk.ru/top/navigator/";
    private const string ResultsOrderUrlPart = "order/num_vote/";


    private int _minimumNumberOfReviews = 10;

    private (float, float)? _ratingOfFilm = null;

    private (int, int)? _ratingOfFilmByCritics = null;

    private (float, float)? _ratingOfFilmOnImdb = null;

    private (int, int)? _ratingPositiveReviewsOfFilm = null;


    public int MinimumNumberOfReviews
    {
        get => _minimumNumberOfReviews;
        set => _minimumNumberOfReviews = value < 10? 
            throw new ArgumentException("the minimum possible number of reviews is 10") : value;
    }

    public (float, float)? RatingOfFilm
    {
        get => _ratingOfFilm;
        set
        {
            if (value is not null)
            {
                if (value.Value.Item1 is < 1f or > 10f)
                    throw new ArgumentException("the minimum rating of film must in range 1 to 10");
                if (value.Value.Item2 is < 1f or > 10f)
                    throw new ArgumentException("the maximum rating of film must in range 1 to 10");
                if (value.Value.Item1 > value.Value.Item2)
                    throw new ArgumentException("the minimum rating of film must smaller or equivalent maximum");
            }
            _ratingOfFilm = value;
        }
    }

    public (int, int)? RatingOfFilmByCritics
    {
        get => _ratingOfFilmByCritics;
        set
        {
            if (value is not null)
            {
                if (value.Value.Item1 is < 0 or > 100)
                    throw new ArgumentException("the minimum rating of film by critics must in range 0 to 100");
                if (value.Value.Item2 is < 0 or > 100)
                    throw new ArgumentException("the maximum rating of film by critics must in range 0 to 100");
                if (value.Value.Item1 > value.Value.Item2)
                    throw new ArgumentException("the minimum rating of film by critics must smaller or equivalent maximum");
            }
            _ratingOfFilmByCritics = value;
        }
    }
    
    public (float, float)? RatingOfFilmOnImdb
    {
        get => _ratingOfFilmOnImdb;
        set
        {
            if (value is not null)
            {
                if (value.Value.Item1 is < 1f or > 10f)
                    throw new ArgumentException("the minimum rating of film on IMDB must in range 1 to 10");
                if (value.Value.Item2 is < 1f or > 10f)
                    throw new ArgumentException("the maximum rating of film on IMDB must in range 1 to 10");
                if (value.Value.Item1 > value.Value.Item2)
                    throw new ArgumentException("the minimum rating of film on IMDB must smaller or equivalent maximum");
            }
            _ratingOfFilmOnImdb = value;
        }
    }

    public (int, int)? RatingPositiveReviewsOfFilm
    {
        get => _ratingPositiveReviewsOfFilm;
        set
        {
            if (value is not null)
            {
                if (value.Value.Item1 is < 0 or > 100)
                    throw new ArgumentException("the minimum rating of positive reviews of film must in range 0 to 100");
                if (value.Value.Item2 is < 0 or > 100)
                    throw new ArgumentException("the maximum rating of positive reviews of film must in range 0 to 100");
                if (value.Value.Item1 > value.Value.Item2)
                    throw new ArgumentException("the minimum rating of positive reviews of film must smaller or equivalent maximum");
            }
            _ratingPositiveReviewsOfFilm = value;
        }
    }

    public string Url =>
        new StringBuilder(BaseUrl)
            .Append(MinimumNumberOfReviewsUrlPart)
            .Append(FilmRatingUrlPart)
            .Append(FilmRatingByCriticsUrlPart)
            .Append(FilmRatingOfPositiveReviewsUrlPart)
            .Append(FilmRatingOnImdbUrlPart)
            .Append(ResultsOrderUrlPart).ToString();

    private string MinimumNumberOfReviewsUrlPart 
        => $"m_act[num_vote]/{_minimumNumberOfReviews}/";

    private string FilmRatingUrlPart => _ratingOfFilm is null 
        ? string.Empty 
        : $"m_act[rating]/{_ratingOfFilm.Value.Item1}:{_ratingOfFilm.Value.Item2}/";
    
    private string FilmRatingByCriticsUrlPart => _ratingOfFilmByCritics is null
        ? string.Empty
        : $"m_act[tomat_rating]/{_ratingOfFilmByCritics.Value.Item1}:{_ratingOfFilmByCritics.Value.Item2}/";
    
    private string FilmRatingOnImdbUrlPart => _ratingOfFilmOnImdb is null
        ? string.Empty
        : $"m_act[ex_rating]/{_ratingOfFilmOnImdb.Value.Item1}:{_ratingOfFilmOnImdb.Value.Item2}/";
    
    private string FilmRatingOfPositiveReviewsUrlPart => _ratingPositiveReviewsOfFilm is null
        ? string.Empty
        : $"m_act[review_procent]/{_ratingPositiveReviewsOfFilm.Value.Item1}:{_ratingPositiveReviewsOfFilm.Value.Item2}/";
}