/// <summary>
/// 使用此静态类来唯一地标识全局的背包源和开始Index
/// </summary>
public static class DragState<TItem, TSlot> 
    where TItem : class
    where TSlot : GenericSlot<TItem>
{
    public static int FromIndex = -1;
    public static View<TItem, TSlot> SourceView = null;

    public static void Reset()
    {
        FromIndex = -1;
        SourceView = null;
    }
}
