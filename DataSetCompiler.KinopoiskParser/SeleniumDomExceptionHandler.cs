using System.Net;

namespace KinopoiskFilmReviewsParser;

public class SeleniumDomExceptionHandler
{
    public T MakeManyRequestsForDom<T>(Func<T> requestForDom, int attemptsNumber = 5)
    {
        for (int nTry = 0; nTry < attemptsNumber; nTry++)
        {
            try
            {
                return requestForDom.Invoke();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        throw new WebException("timeout of requests to DOM tree");
    }
    
    public void MakeManyRequestsForDom(Action requestForDom, int attemptsNumber = 5)
    {
        for (int nTry = 0; nTry < attemptsNumber; nTry++)
        {
            try
            {
                requestForDom.Invoke();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        throw new WebException("timeout of requests to DOM tree");
    }
    
    public T? MakeManyRequestsForDomWithHandler<T>(Func<T> requestForDom, Action? handler, int attemptsNumber = 5)
    {
        for (int nTry = 0; nTry < attemptsNumber; nTry++)
        {
            try
            {
                return requestForDom.Invoke();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        handler?.Invoke();
        return default;
    }
    
}