namespace DataAggergator.Application.Dtos
{
    public record AllPagesOverViewDto : BaseOverViewDto
    {
        public string Page { get; set; } = default!;
    }

}
