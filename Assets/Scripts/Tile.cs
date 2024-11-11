using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;

    public Item item
    {
        get => this._item;

        set
        {
            if (this._item == value) return;

            this._item = value;
            icon.sprite = this._item.sprite;
        }
    }

    public Image icon;

    public Button button;

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }
}
