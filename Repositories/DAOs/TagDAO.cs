using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;

namespace Repositories.DAOs
{
    public class TagDAO
    {
        private static TagDAO? _instance;
        private static readonly object _instanceLock = new object();

        private TagDAO() { }

        public static TagDAO Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new TagDAO();
                    }
                    return _instance;
                }
            }
        }

        public List<Tag> GetTags()
        {
            using var context = new FUNewsManagementDbContext();
            return context.Tags.OrderBy(t => t.TagName).ToList();
        }

        public Tag? GetTagById(int id)
        {
            using var context = new FUNewsManagementDbContext();
            return context.Tags.Find(id);
        }

        public int GetNextTagId()
        {
            using var context = new FUNewsManagementDbContext();
            if (!context.Tags.Any()) return 1;
            return context.Tags.Max(t => t.TagID) + 1;
        }

        public Tag AddTag(Tag tag)
        {
            using var context = new FUNewsManagementDbContext();
            if (tag.TagID <= 0)
            {
                tag.TagID = GetNextTagId();
            }
            context.Tags.Add(tag);
            context.SaveChanges();
            return tag;
        }
    }
}
