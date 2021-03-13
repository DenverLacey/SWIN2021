using System;
namespace SwinAdventure
{
    public interface IHasInventory
    {
        GameObject Locate(string id);
        string Name { get; }
    }
}
