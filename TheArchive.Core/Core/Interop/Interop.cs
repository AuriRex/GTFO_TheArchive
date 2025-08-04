using System;
using TheArchive.Utilities;

namespace TheArchive.Core.Interop;

internal static class Interop
{
    internal static void OnDataBlocksReady()
    {
        try
        {
            LocaliaCoreInterop.TryApplyPatch();
        }
        catch (Exception ex)
        {
            ArchiveLogger.Exception(ex);
        }
    }
}