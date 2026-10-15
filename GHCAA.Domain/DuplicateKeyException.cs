namespace GHCAA.Domain
{
    // A save would give a row the same natural key as another row (62.56g). Controllers map it to a
    // 400 with the message. It is its own type so a catch does not also swallow the
    // InvalidOperationException that EF and Npgsql throw for unrelated faults.
    public class DuplicateKeyException : InvalidOperationException
    {
        public DuplicateKeyException(string message) : base(message) { }
    }
}
