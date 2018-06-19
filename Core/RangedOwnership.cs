using System.Text;

namespace Core
{
    public class RangedOwnership {
        public class Node {
            public long Value;
            public byte Owner;
            public Node Left, Right;
            public Node(long value, byte owner = 0) {
                Value = value;
                Owner = owner;
            }
        }

        readonly Node _root;
        readonly long Size;

        public RangedOwnership(long size) {
            Size = size;
            _root = new Node(size / 2);
        }

        byte _getOwner(Node cnode, long pos) {
            do {
                if (cnode == null)
                    return 0;

                if (cnode.Value == pos)
                    return cnode.Owner;

                cnode = pos < cnode.Value ? cnode.Left : cnode.Right;
            } while (true);
        }

        public byte GetOwner(long pos) {
            return _getOwner(_root, pos);
        }

        void _setOwner(Node cnode, long pos, byte owner) {
            do {
                if (cnode.Value == pos) {
                    cnode.Owner = owner;
                    return;
                }

                if (pos < cnode.Value) {
                    if (cnode.Left == null) {
                        cnode.Left = new Node(pos);
                    }
                    cnode = cnode.Left;
                } else {
                    if (cnode.Right == null) {
                        cnode.Right = new Node(pos);
                    }
                    cnode = cnode.Right;
                }
            } while (true);
        }

        public void SetOwner(long pos, byte owner) {
            _setOwner(_root, pos, owner);
        }

        void _stringify(Node cnode, StringBuilder builder) {
            if (cnode.Left != null) _stringify(cnode.Left, builder);
            builder.AppendLine();
            builder.AppendFormat("\t0x{0:X8}: {1}", cnode.Value, cnode.Owner);
            if (cnode.Right != null) _stringify(cnode.Right, builder);
        }

        public override string ToString() {
            var builder = new StringBuilder();
            if (_root.Owner == 0 && _root.Left == null && _root.Right == null) {
                builder.AppendLine();
                builder.Append("\tNo owners on all memory.");
            } else {
                if (_root.Owner == 0) {
                    if (_root.Left != null) _stringify(_root.Left, builder);
                    if (_root.Right != null) _stringify(_root.Right, builder);
                } else {
                    _stringify(_root, builder);
                }
            }
            return builder.ToString();
        }
    }
}
