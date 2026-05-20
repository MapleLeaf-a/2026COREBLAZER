using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ViewModel<T> : INotifyPropertyChanged where T : class
{
    public Model<T> model;

    //��ǰҳ��
    protected int currentPage = 0;
    //��ҳ��
    protected int totalPages;
    //ÿҳ���е�Ԫ������
    protected int itemsPerPage;
    //��ǰҳѡ�е���Ʒ�ڵ�ǰҳ��index
    protected int selectecIndex = -1;

    ////string��Sprite��ӳ��,���ڶ�ȡÿ��item��ͼƬ
    //protected Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public ViewModel(Model<T> model, int itemsPerPage)
    { 
        this.model = model;
        this.itemsPerPage = itemsPerPage;
        this.totalPages = model.Capacity / itemsPerPage;

        InitDictionarySprites();
    }

    /// <summary>
    /// ��ȡ��ǰҳ��������Ʒ
    /// </summary>
    /// <returns></returns>
    public T[] CurrentPageItems
    {
        get
        {
            int start = currentPage * itemsPerPage;
            int end = (currentPage + 1) * itemsPerPage - 1;

            return model.GetItemRange(start, end);
        }
    }

    /// <summary>
    /// ��null��Ʒ������
    /// </summary>
    public int Count => model.Count;

    /// <summary>
    /// ��ǰѡ�е���Ʒ
    /// </summary>
    public T SelectedItem
    {
        get
        {
            T[] currenPageItems = CurrentPageItems;
            if (selectecIndex >= 0 && selectecIndex < currenPageItems.Length)
            { return currenPageItems[selectecIndex]; }
            return null;
        }
    }

    /// <summary>
    /// ��ǰѡ�е���Ʒ�ڵ�ǰҳ������
    /// </summary>
    public int SelectedIndex => selectecIndex;

    /// <summary>
    /// ��ǰҳ�ı��(��һ��ʼ)
    /// </summary>
    public int CurrentPageNumber => currentPage + 1;

    /// <summary>
    /// ��ҳ��
    /// </summary>
    public int TotalPages => totalPages;

    /// <summary>
    /// ������Ʒ
    /// </summary>
    public virtual void AddItem(T bagItem)
    {
        if (model.AddItem(bagItem)) //�����ӳɹ�
        {
            OnPropertyChanged(nameof(CurrentPageItems));
        }
    }

    /// <summary>
    /// ��ָ������λ��������Ʒ
    /// </summary>
    /// <param name="bagItem"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual void AddItemAt(T bagItem, int index)
    {
        if (model.AddItemAt(bagItem, index))
        {
            OnPropertyChanged(nameof(CurrentPageItems));
        }
    }

    /// <summary>
    /// ɾ����ǰҳindex����Ʒ
    /// </summary>
    /// <param name="itemIndexInCurrentPage"></param>
    /// <param name="quantity"></param>
    public virtual bool RemoveItemAt(int itemIndexInCurrentPage)
    {
        int indexInBackpack = currentPage * totalPages + itemIndexInCurrentPage;
        if (model.RemoveItemAt(indexInBackpack)) //��ɾ���ɹ�
        {
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(SelectedItem));
            return true;
        }
        return false;
    }

    /// <summary>
    /// ѡ����Ʒ
    /// </summary>
    public virtual T SelectItem(int index)
    {
        if (selectecIndex != index)
        {
            selectecIndex = index;

            OnPropertyChanged(nameof(SelectedItem));
        }

        return SelectedItem;
    }

    /// <summary>
    /// ��ȡ��ǰҳָ��index����Ʒ
    /// </summary>
    /// <returns></returns>
    public virtual T GetItemAt(int index)
    {
        return model.GetItemAt(itemsPerPage * currentPage + index);
    }

    /// <summary>
    /// �����ƶ���Ʒ(ͬһҳ��)
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns>�Ƿ�ɹ�</returns>
    public virtual bool TryMoveItem(int from, int to)
    {
        T fromItem = GetItemAt(from);
        T toItem = GetItemAt(to);

        if (fromItem == null) return false;

        model.SwapItem(currentPage * itemsPerPage + from, currentPage * itemsPerPage + to);

        OnPropertyChanged(nameof(CurrentPageItems));

        return true;
    }

    /// <summary>
    /// ��������һ�����ƶ���Ʒ
    /// </summary>
    /// <param name="anotherBackpack"></param>
    /// <param name="fromInCurrent">��ҳindex</param>
    /// <param name="toInTarget">Ŀ�걳������ҳ��index</param>
    /// <returns></returns>
    public virtual bool TryTransferTo(ViewModel<T> anotherBackpack, int fromInCurrent, int toInTarget)
    {
        if (anotherBackpack == null) return false;

        T fromItem = GetItemAt(fromInCurrent);
        T toItem = anotherBackpack.GetItemAt(toInTarget);

        if (fromItem == null) return false;


        if (toItem == null)
        {
            anotherBackpack.AddItemAt(fromItem, toInTarget);
        }
        else //�ǿ���Ϊ�����ƶ�
        {
            return false;
        }

        RemoveItemAt(fromInCurrent); //��ǰ���������ԭ�����ĸ���

        return true;
    }

    public virtual void RefreshAll()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(CurrentPageNumber));
        OnPropertyChanged(nameof(SelectedItem));
    }

    //ʵ�ֽӿ�,MVVM�ĺ��Ľӿ�,�����Ա仯��֪ͨUI����
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) //[CallerMemberName] ����������,����ʱ�Զ���ȡ���������ߵ�������
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// ��ʼ���ֵ��ͼƬ
    /// </summary>
    protected virtual void InitDictionarySprites()
    { 
        
    }

    //��sprites���Ӽ�ֵ��
    protected void AddPairToSprites(string path)
    {
        SpriteStatic.AddPairToSprites(path);
    }

    public Sprite GetSprite(string path)
    {
        return SpriteStatic.GetSprite(path);
    }
}
