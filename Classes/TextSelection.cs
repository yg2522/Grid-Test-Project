using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1.Classes
{
    public class TextSelection
    {
        #region Constructors
        public TextSelection(int start, string item)
        {
            Start = start;
            Item = item;
        }
        #endregion

        #region Properties
        public int Start { get; set; }
        public string Item { get; set; }
        #endregion

        #region Methods
        public bool Equals(TextSelection other)
        {
            return equals(this, other);
        }
        #endregion

        #region Operators
        public static bool operator ==(TextSelection x, TextSelection y)
        {
            return equals(x, y);
        }

        public static bool operator !=(TextSelection x, TextSelection y)
        {
            return !equals(x, y);
        }
        #endregion

        #region Private Methods
        private static bool equals(TextSelection x, TextSelection y)
        {
            if ((!object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null)) || (object.ReferenceEquals(x, null) && !object.ReferenceEquals(y, null))) 
                return false;
            if (object.ReferenceEquals(x, null) && object.ReferenceEquals(y, null)) 
                return true;

            return (x.GetHashCode() == y.GetHashCode());
        }
        #endregion

        #region Object Overrides
        public override bool Equals(object obj)
        {
            return equals(this, obj as TextSelection);
        }

        public override int GetHashCode()
        {
            TextSelectionComparer comparer = new TextSelectionComparer();
            return comparer.GetHashCode(this);
        }
        #endregion

        public class TextSelectionComparer  : IEqualityComparer<TextSelection>
        {
            #region IEqualityComparer<TextSelection> Members
            public bool Equals(TextSelection x, TextSelection y)
            {
                return equals(x, y);
            }

            public int GetHashCode(TextSelection obj)
            {
                if (obj == null)
                    return 0;

                //get a unique number depending on start and length: example given by http://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
                //start + length would be different than length + start using this method
                int hash = 23;
                hash = hash * 31 + obj.Start.GetHashCode();
                hash = hash * 31 + obj.Item.GetHashCode();

                return hash;
            }
            #endregion
        }
    }
}
