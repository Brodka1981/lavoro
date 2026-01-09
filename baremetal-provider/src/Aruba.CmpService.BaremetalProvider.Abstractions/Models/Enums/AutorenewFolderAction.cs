namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

public enum AutorenewFolderAction
{
    /// <summary>
    /// Remove resource from safe folders 
    /// </summary>
    RemoveFromFolder,

    /// <summary>
    /// Keep resource in the folders and disable safe folders
    /// </summary>
    DisableSafeFolder
}
