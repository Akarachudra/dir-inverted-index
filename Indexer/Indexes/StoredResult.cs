using Indexer.Helpers;

namespace Indexer.Indexes
{
    public class StoredResult
    {
        public string PathHash { get; set; }

        public int RowNumber { get; set; }

        public int ColNumber { get; set; }

        public override bool Equals(object obj)
        {
            return obj is StoredResult result &&
                   this.PathHash == result.PathHash &&
                   this.RowNumber == result.RowNumber &&
                   this.ColNumber == result.ColNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = -773674528;
            hashCode = hashCode * -1521134295 + StringHelper.GetHashCode(this.PathHash);
            hashCode = hashCode * -1521134295 + this.RowNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ColNumber.GetHashCode();
            return hashCode;
        }
    }
}