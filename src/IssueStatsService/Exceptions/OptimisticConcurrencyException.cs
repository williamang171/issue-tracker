namespace IssueStatsService.Exceptions;

public class OptimisticConcurrencyException(string message) : Exception(message)
{
}