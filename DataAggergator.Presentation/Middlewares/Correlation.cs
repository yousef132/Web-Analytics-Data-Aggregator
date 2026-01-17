namespace DataAggergator.Presentation.Middlewares
{
    public static class Correlation
    {
        public static AsyncLocal<Guid> Current = new AsyncLocal<Guid>();
    }
}
