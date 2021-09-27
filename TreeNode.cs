using System.Collections.Generic;

namespace DevKitFileDescriptions
{
    public class TreeNode
    {
        public string key = "";
        public string label = "";
        public string data = "";
        public IList<TreeNode> children = null;
    }
}
