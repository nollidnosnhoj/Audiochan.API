using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Features.Tags.CreateTags
{
    public record CreateTagsCommand(IEnumerable<string> Tags) : IRequest<List<Tag>>
    {
    }
    
    public class CreateTagsCommandHandler : IRequestHandler<CreateTagsCommand, List<Tag>>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateTagsCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<List<Tag>> Handle(CreateTagsCommand request, CancellationToken cancellationToken)
        {
            var taggifyTags = request.Tags.FormatTags();

            var tags = await _dbContext.Tags
                .Where(tag => taggifyTags.Contains(tag.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var tag in taggifyTags.Where(tag => tags.All(t => t.Id != tag)))
            {
                tags.Add(new Tag{Id = tag});
            }

            return tags;
        }
    }
}