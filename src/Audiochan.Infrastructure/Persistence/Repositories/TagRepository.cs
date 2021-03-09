using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiochan.Core.Common.Extensions;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Audiochan.Infrastructure.Persistence.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<Tag> BaseQueryable => Context.Set<Tag>();


        public async Task<List<Tag>> InsertAsync(IEnumerable<string> tags,
            CancellationToken cancellationToken = default)
        {
            var taggifyTags = tags.FormatTags();

            var tagEntities = await BaseQueryable
                .Where(tag => taggifyTags.Contains(tag.Id))
                .ToListAsync(cancellationToken);

            foreach (var tag in taggifyTags.Where(tag => tagEntities.All(t => t.Id != tag)))
            {
                tagEntities.Add(new Tag {Id = tag});
            }

            return tagEntities;
        }
    }
}