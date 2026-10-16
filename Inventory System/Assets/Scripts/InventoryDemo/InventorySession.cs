using System;
using UnityEngine;

namespace InventoryDemo
{
    public enum InventoryArea
    {
        Backpack,
        Refresh,
        ForgeInput,
        ForgeOutput
    }

    public sealed class InventorySession
    {
        public InventoryModel Backpack { get; }
        public InventoryModel Refresh { get; }
        public InventoryModel ForgeInput { get; }
        public InventoryModel ForgeOutput { get; }

        public InventorySession(int backpackCapacity, int refreshCapacity, int forgeInputCapacity, int forgeOutputCapacity)
        {
            Backpack = new InventoryModel(backpackCapacity);
            Refresh = new InventoryModel(refreshCapacity);
            ForgeInput = new InventoryModel(forgeInputCapacity);
            ForgeOutput = new InventoryModel(forgeOutputCapacity);
        }

        public InventoryModel Get(InventoryArea area)
        {
            switch (area)
            {
                case InventoryArea.Backpack: return Backpack;
                case InventoryArea.Refresh: return Refresh;
                case InventoryArea.ForgeInput: return ForgeInput;
                case InventoryArea.ForgeOutput: return ForgeOutput;
                default: throw new ArgumentOutOfRangeException(nameof(area), area, null);
            }
        }
    }
}
