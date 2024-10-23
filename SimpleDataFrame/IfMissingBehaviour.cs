namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public enum IfMissingBehaviour
{
    Create,

    /// <summary>
    /// When dealing with incomplete data (where strict data integrity is not a primary concern or absence is a non-critical event)
    /// e.g. Incremental Data Updates; Optional Data Fields; Data Sync across systems; dirty data; incomplete user-driven content
    /// </summary>
    Continue,

    /// <summary>
    /// More usual case where we want to throw an exception if the data to updated can't be found
    /// </summary>
    Throw
}