using System.Text;

namespace KinopoiskFilmReviewsParser.Parsers.LinksParsers;

public class BestFilmsSearchQuery
{
    private const string BaseUrl = "https://www.kinopoisk.ru/top/navigator/";
    private const string ResultsOrderUrlPart = "order/num_vote/";


    private int _minimumNumberOfReviews = 10;

    private float _minimumRatingOfFilm = 1f;
    private float _maximumRatingOfFilm = 10f;

    private int _minimumRatingOfFilmByCritics = 0;
    private int _maximumRatingOfFilmByCritics = 100;

    private float _minimumRatingOfFilmOnImdb = 1f;
    private float _maximumRatingOfFilmOnImdb = 10f;

    private int _minimumRatingPositiveReviewsOfFilm = 0;
    private int _maximumRatingPositiveReviewsOfFilm = 100;


    public int MinimumNumberOfReviews
    {
        get => _minimumNumberOfReviews;
        set => _minimumNumberOfReviews = value < 10? 
            throw new ArgumentException("the minimum possible number of reviews is 10") : value;
    }

    public (float, float) RatingOfFilm
    {
        get => (_minimumRatingOfFilm, _maximumRatingOfFilm);
        set
        {
            if (value.Item1 is < 1f or > 10f)
                throw new ArgumentException("the minimum rating of film must in range 1 to 10");
            if (value.Item2 is < 1f or > 10f)
                throw new ArgumentException("the maximum rating of film must in range 1 to 10");
            if (value.Item1 > value.Item2)
                throw new ArgumentException("the minimum rating of film must smaller or equivalent maximum");
            _minimumRatingOfFilm = value.Item1;
            _maximumRatingOfFilm = value.Item2;
        }
    }

    public (int, int) RatingOfFilmByCritics
    {
        get => (_minimumRatingOfFilmByCritics, _maximumRatingOfFilmByCritics);
        set
        {
            if (value.Item1 is < 0 or > 100)
                throw new ArgumentException("the minimum rating of film by critics must in range 0 to 100");
            if (value.Item2 is < 0 or > 100)
                throw new ArgumentException("the maximum rating of film by critics must in range 0 to 100");
            if (value.Item1 > value.Item2)
                throw new ArgumentException("the minimum rating of film by critics must smaller or equivalent maximum");
            _minimumRatingOfFilmByCritics = value.Item1;
            _maximumRatingOfFilmByCritics = value.Item2;
        }
    }
    
    public (float, float) RatingOfFilmOnImdb
    {
        get => (_minimumRatingOfFilmOnImdb, _maximumRatingOfFilmOnImdb);
        set
        {
            if (value.Item1 is < 1f or > 10f)
                throw new ArgumentException("the minimum rating of film on IMDB must in range 1 to 10");
            if (value.Item2 is < 1f or > 10f)
                throw new ArgumentException("the maximum rating of film on IMDB must in range 1 to 10");
            if (value.Item1 > value.Item2)
                throw new ArgumentException("the minimum rating of film on IMDB must smaller or equivalent maximum");
            _minimumRatingOfFilmOnImdb = value.Item1;
            _maximumRatingOfFilmOnImdb = value.Item2;
        }
    }

    public (int, int) RatingPositiveReviewsOfFilm
    {
        get => (_minimumRatingPositiveReviewsOfFilm, _maximumRatingPositiveReviewsOfFilm);
        set
        {
            if (value.Item1 is < 0 or > 100)
                throw new ArgumentException("the minimum rating of positive reviews of film must in range 0 to 100");
            if (value.Item2 is < 0 or > 100)
                throw new ArgumentException("the maximum rating of positive reviews of film must in range 0 to 100");
            if (value.Item1 > value.Item2)
                throw new ArgumentException("the minimum rating of positive reviews of film must smaller or equivalent maximum");
            _minimumRatingPositiveReviewsOfFilm = value.Item1;
            _maximumRatingPositiveReviewsOfFilm = value.Item2;
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
    private string FilmRatingUrlPart 
        => $"m_act[rating]/{_minimumRatingOfFilm}:{_maximumRatingOfFilm}/";
    private string FilmRatingByCriticsUrlPart 
        => $"m_act[tomat_rating]/{_minimumRatingOfFilmByCritics}:{_maximumRatingOfFilmByCritics}/";
    private string FilmRatingOnImdbUrlPart
        => $"m_act[ex_rating]/{_minimumRatingOfFilmOnImdb}:{_maximumRatingOfFilmOnImdb}/";
    private string FilmRatingOfPositiveReviewsUrlPart
        => $"m_act[review_procent]/{_minimumRatingPositiveReviewsOfFilm}:{_maximumRatingPositiveReviewsOfFilm}/";
}