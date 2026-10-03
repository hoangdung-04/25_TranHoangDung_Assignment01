using System.Collections.Generic;
using BusinessObjects;
using Repositories.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class TagRepository : ITagRepository
    {
        public List<Tag> GetTags() => TagDAO.Instance.GetTags();

        public Tag? GetTagById(int id) => TagDAO.Instance.GetTagById(id);

        public Tag AddTag(Tag tag) => TagDAO.Instance.AddTag(tag);
    }
}
