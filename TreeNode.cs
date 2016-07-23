namespace Parser_C_Sharp
{
    public class TreeNode
    {
        public Token Data;
        public TreeNode Left;
        public TreeNode Right;

        public TreeNode(Token token, TreeNode l = null, TreeNode r = null)
        {
            Data = token;
            Left = l;
            Right = r;
        }

        /// <summary>
        /// ���������� ������, �������������� ������� ������.
        /// </summary>
        /// <returns>
        /// ������, �������������� ������� ������.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => Data.ToString();
    }
}