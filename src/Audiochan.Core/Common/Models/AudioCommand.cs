using System.Collections.Generic;

namespace Audiochan.Core.Common.Models
{
    public abstract record AudioCommand
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public bool? IsPublic { get; init; }
        public bool? IsLoop { get; init; }
        public string Genre { get; init; }
        public List<string> Tags { get; init; } = new();
    }
}