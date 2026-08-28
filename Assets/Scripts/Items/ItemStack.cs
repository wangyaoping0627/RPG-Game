/// 背包里的一格：道具数据 + 数量（同类可堆叠）
[System.Serializable]
public class ItemStack
{
    public ItemData data;
    public int count;

    public ItemStack(ItemData data, int count = 1)
    {
        this.data = data;
        this.count = count;
    }
}
