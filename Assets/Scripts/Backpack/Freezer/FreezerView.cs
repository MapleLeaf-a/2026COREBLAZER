using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezerView : BackpackView<BackpackViewModel>
{
    protected override void BindOtherButtons() { }

    protected override BackpackViewModel CreateViewModel(BackpackModel model, int itemsPerPage)
    {
        return new BackpackViewModel(model, itemsPerPage);
    }
}
