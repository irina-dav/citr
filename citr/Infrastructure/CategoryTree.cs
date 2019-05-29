using citr.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace citr.Infrastructure
{
    public class CategoryTree
    {
        private ApplicationDbContext context;

        public CategoryTree(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public string GetCategoriesJson(int? selectedNode = null)
        {
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            List<ResourceCategory> categories = context.ResourceCategories.ToList();

            foreach (ResourceCategory cat in categories)
            {
                nodes.Add(new TreeViewNode()
                {
                    id = cat.ID,
                    parent = cat.ParentCategoryID,
                    text = cat.Name,
                    state = selectedNode.HasValue && selectedNode.Value == cat.ID ? new TreeViewNodeState() { selected = true } : null,
                    children = new List<TreeViewNode>()
                });
            }
            Dictionary<int, TreeViewNode> dict = nodes.ToDictionary(n => n.id);
            foreach (TreeViewNode n in dict.Values)
            {
                if (n.parent != n.id)
                {
                    TreeViewNode parent = dict[n.parent];
                    parent.children.Add(n);
                }
            }

            TreeViewNode root = dict.Values.First(n => n.parent == n.id);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(root, settings);
            return json;
        }
    }

    public class TreeViewNodeState
    {
        public bool selected;
    }

    public class TreeViewNode
    {
        public int id { get; set; }
        [JsonIgnore]
        public int parent { get; set; }
        public string text { get; set; }
        public TreeViewNodeState state { get; set; }
        public List<TreeViewNode> children { get; set; }
    }

    public static class TreeNodeExtensions
    {
        public static TreeViewNode ToTree(this List<TreeViewNode> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            TreeViewNode root = list.SingleOrDefault(x => x.parent == -1);
            if (root == null)
            {
                throw new InvalidOperationException("root == null");
            }

            PopulateChildren(root, list);

            return root;
        }

        private static void PopulateChildren(TreeViewNode node, List<TreeViewNode> all)
        {
            List<TreeViewNode> childs = all.Where(x => x.parent.Equals(node.id)).ToList();

            foreach (TreeViewNode item in childs)
            {
                node.children.Add(item);
            }

            foreach (TreeViewNode item in childs)
            {
                all.Remove(item);
            }

            foreach (TreeViewNode item in childs)
            {
                PopulateChildren(item, all);
            }
        }
    }
}
