using System.Formats.Nrbf;
using HarmonyLib;
using VRage.GameServices;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class BinaryFormatterPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MyInventoryHelper), nameof(MyInventoryHelper.CheckItemData))]
    private static bool ReadInventoryItemsPrefix(byte[] checkData, out bool checkResult, out List<MyGameInventoryItem> __result)
    {
        __result = [];
        if (!NrbfDecoder.StartsWithPayloadHeader(checkData))
        {
            checkResult = false;
            return false;
        }

        var listRecord = NrbfDecoder.DecodeClassRecord(new MemoryStream(checkData));

        if (!listRecord.TypeNameMatches(typeof(List<MyGameInventoryItem>)) ||
            listRecord.GetArrayRecord("_items") is not { } itemsRecord ||
            itemsRecord.GetArray(typeof(MyGameInventoryItem[]), false) is not SerializationRecord[] items)
        {
            checkResult = false;
            return false;
        }

        __result.Capacity = listRecord.GetInt32("_size");
        
        foreach (var classRecord in items.OfType<ClassRecord>())
        {
            if (!classRecord.TypeNameMatches(typeof(MyGameInventoryItem)))
                continue;
            
            var itemDefinitionRecord = classRecord.GetClassRecord(PropertyName("ItemDefinition"));
            if (itemDefinitionRecord is null || !itemDefinitionRecord.TypeNameMatches(typeof(MyGameInventoryItemDefinition)))
                continue;
            
            var definitionTypeRecord = itemDefinitionRecord.GetClassRecord(PropertyName("DefinitionType"));
            if (definitionTypeRecord is null || !definitionTypeRecord.TypeNameMatches(typeof(MyGameInventoryItemDefinitionType)))
                continue;
            
            var itemSlotRecord = itemDefinitionRecord.GetClassRecord(PropertyName("ItemSlot"));
            if (itemSlotRecord is null || !itemSlotRecord.TypeNameMatches(typeof(MyGameInventoryItemSlot)))
                continue;
            
            var itemQualityRecord = itemDefinitionRecord.GetClassRecord(PropertyName("ItemQuality"));
            if (itemQualityRecord is null || !itemQualityRecord.TypeNameMatches(typeof(MyGameInventoryItemQuality)))
                continue;

            var itemDefinition = new MyGameInventoryItemDefinition
            {
                ID = itemDefinitionRecord.GetInt32(PropertyName("ID")),
                Name = itemDefinitionRecord.GetString(PropertyName("Name")),
                Tradable = itemDefinitionRecord.GetString(PropertyName("Tradable")),
                Marketable = itemDefinitionRecord.GetString(PropertyName("Marketable")),
                Description = itemDefinitionRecord.GetString(PropertyName("Description")),
                IconTexture = itemDefinitionRecord.GetString(PropertyName("IconTexture")),
                DisplayType = itemDefinitionRecord.GetString(PropertyName("DisplayType")),
                AssetModifierId = itemDefinitionRecord.GetString(PropertyName("AssetModifierId")),
                NameColor = itemDefinitionRecord.GetString(PropertyName("NameColor")),
                BackgroundColor = itemDefinitionRecord.GetString(PropertyName("BackgroundColor")),
                ToolName = itemDefinitionRecord.GetString(PropertyName("ToolName")),
                IsStoreHidden = itemDefinitionRecord.GetBoolean(PropertyName("IsStoreHidden")),
                Hidden = itemDefinitionRecord.GetBoolean(PropertyName("Hidden")),
                CanBePurchased = itemDefinitionRecord.GetBoolean(PropertyName("CanBePurchased")),
                Exchange = itemDefinitionRecord.GetString(PropertyName("Exchange")),
                DefinitionType = (MyGameInventoryItemDefinitionType)definitionTypeRecord.GetInt32("value__"),
                ItemSlot = (MyGameInventoryItemSlot)itemSlotRecord.GetInt32("value__"),
                ItemQuality = (MyGameInventoryItemQuality)itemQualityRecord.GetInt32("value__")
            };
            
            var id = classRecord.GetUInt64(PropertyName("ID"));
            var quantity = classRecord.GetUInt16(PropertyName("Quantity"));
            var isStoreFakeItem = classRecord.GetBoolean(PropertyName("IsStoreFakeItem"));
            var isNew = classRecord.GetBoolean(PropertyName("IsNew"));
            
            __result.Add(new MyGameInventoryItem
            {
                ID = id,
                Quantity = quantity,
                ItemDefinition = itemDefinition,
                IsStoreFakeItem = isStoreFakeItem,
                IsNew = isNew,
                UsingCharacters = []
            });
        }

        checkResult = true;
        return false;
    }

    private static string PropertyName(string name) => $"<{name}>k__BackingField";
}