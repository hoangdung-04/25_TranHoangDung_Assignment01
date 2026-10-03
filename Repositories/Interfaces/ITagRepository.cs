using System.Collections.Generic;
using BusinessObjects;

namespace Repositories.Interfaces
{
    public interface ITagRepository
    {
        List<Tag> GetTags();
        Tag? GetTagById(int id);
        Tag AddTag(Tag tag);
    }
}
