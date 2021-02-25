using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Core.Services
{
    public class TagService
    {
        private readonly IApplicationDbContext _dbContext;

        public TagService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Tag>> CreateTags(IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            var taggifyTags = tags.FormatTags();

            var tagEntities = await _dbContext.Tags
                .Where(tag => taggifyTags.Contains(tag.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var tag in taggifyTags.Where(tag => tagEntities.All(t => t.Id != tag)))
            {
                tagEntities.Add(new Tag{Id = tag});
            }

            return tagEntities;
        }
    }
}