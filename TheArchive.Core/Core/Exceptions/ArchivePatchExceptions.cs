using System;

namespace TheArchive.Core.Exceptions;

public class ArchivePatchDuplicateIDException : Exception
{
    public ArchivePatchDuplicateIDException(string message) : base(message) { }
}

public class ArchivePatchMethodNotStaticException : Exception
{
    public ArchivePatchMethodNotStaticException(string message) : base(message) { }
}

public class ArchivePatchNoTypeProvidedException : Exception
{
    public ArchivePatchNoTypeProvidedException(string message) : base(message) { }
}

public class ArchivePatchNoOriginalMethodException : Exception
{
    public ArchivePatchNoOriginalMethodException(string message) : base(message) { }
}

public class ArchivePatchNoPatchMethodException : Exception
{
    public ArchivePatchNoPatchMethodException(string message) : base(message) { }
}