using MediatR;

namespace Audiochan.Core.Common.Models
{
    public record PaginationQuery<TResponse> : IRequest<PagedList<TResponse>>
    {
        public int Page { get; init; } = 1;
        public int Size { get; init; } = 30;
    }
}