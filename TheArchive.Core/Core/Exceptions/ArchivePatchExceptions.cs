using System;

namespace TheArchive.Core.Exceptions;

/// <summary>
/// An exception that's thrown if there's already a Feature with the same ID registered.
/// </summary>
public class ArchiveFeatureDuplicateIDException : Exception
{
    /// <inheritdoc />
    public ArchiveFeatureDuplicateIDException(string message) : base(message) { }
}

/// <summary>
/// An exception that's thrown if the provided method was not static.
/// </summary>
public class ArchivePatchMethodNotStaticException : Exception
{
    /// <inheritdoc />
    public ArchivePatchMethodNotStaticException(string message) : base(message) { }
}

/// <summary>
/// An exception that's thrown if the type that contains the method-to-patch has not been provided.
/// </summary>
public class ArchivePatchNoTypeProvidedException : Exception
{
    /// <inheritdoc />
    public ArchivePatchNoTypeProvidedException(string message) : base(message) { }
}

/// <summary>
/// An exception that's thrown if the method-to-patch has not been found.
/// </summary>
public class ArchivePatchNoOriginalMethodException : Exception
{
    /// <inheritdoc />
    public ArchivePatchNoOriginalMethodException(string message) : base(message) { }
}

/// <summary>
/// An exception that's thrown if there are no patch methods provided.
/// </summary>
public class ArchivePatchNoPatchMethodException : Exception
{
    /// <inheritdoc />
    public ArchivePatchNoPatchMethodException(string message) : base(message) { }
}