namespace Greggs.Products.Api;

public static class Constants
{
    public static class ErrorMessages
    {
        public const string PageStartNegative = "pageStart must be zero or greater.";
        public const string PageSizeNegative = "pageSize must be zero or greater.";
        public const string CurrencyNotSupported = "Currency '{0}' is not supported.";

        public const string InvalidRequestTitle = "Invalid request";
        public const string UnexpectedErrorTitle = "An unexpected error occurred.";
    }

    public static class Defaults
    {
        public const string Currency = "GBP";
        public const int PageStart = 0;
        public const int PageSize = 5;
        public const string ProblemContentType = "application/problem+json";
    }
}